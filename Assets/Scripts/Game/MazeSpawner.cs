using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable MemberCanBePrivate.Global

namespace Game
{
    public class MazeSpawner : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static Cell SampleWall;
        public Cell sampleWall;
        public static Cell LuckyWall;
        public Cell luckyWall;
        public static Cell IronWall;
        public Cell ironWall;
        private static int _type;
        public static GameObject FrameBlock;
        public GameObject frameBlock;
        private static readonly Vector3 CellSize = new Vector3(20, 0, 20);

        public static Maze Maze;

        public static Stack<int> Ids;

        private static GameObject _spawn1, _spawn2, _spawn3, _spawn4;
        public GameObject spawn1, spawn2, spawn3, spawn4;

        private static GameObject _tank;
        public GameObject tank;

        private void Start()
        {
            _spawn1 = spawn1;
            _spawn2 = spawn2;
            _spawn3 = spawn3;
            _spawn4 = spawn4;
            _tank = tank;
            Ids = new Stack<int>();
            SampleWall = sampleWall;
            LuckyWall = luckyWall;
            IronWall = ironWall;
            FrameBlock = frameBlock;
        }

        public static void Spawn()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Thread.Sleep(100);
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            var generator = new MazeGenerator();
            Maze = generator.GenerateMaze();

            for (var x = 0; x < Maze.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < Maze.Cells.GetLength(1); y++)
                {
                    Cell c;
                    _type = Random.Range(0, 15);
                    if (_type == 0)
                    {
                        c = PhotonNetwork.Instantiate(LuckyWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    else if (_type > 0 && _type < 4)
                    {
                        c = PhotonNetwork.Instantiate(IronWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    else
                    {
                        c = PhotonNetwork.Instantiate(SampleWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                    var sendOptions = new SendOptions {Reliability = true};
                    if (x == Maze.Cells.GetLength(0) - 1 && y == Maze.Cells.GetLength(1) - 1)
                    {
                        var position = c.wallCenter.transform.position;
                        _spawn2.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(2, _spawn2.transform.position, options, sendOptions);
                    }

                    if (x == Maze.Cells.GetLength(0) - 1 && y == 0)
                    {
                        var position = c.wallLeft.transform.position;
                        _spawn4.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(4, _spawn4.transform.position, options, sendOptions);
                    }

                    if (x == 0 && y == Maze.Cells.GetLength(1) - 1)
                    {
                        var position = c.wallBottom.transform.position;
                        _spawn3.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(3, _spawn3.transform.position, options, sendOptions);
                    }

                    if (!Maze.Cells[x, y].WallBottom)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallBottom.GetPhotonView().ViewID, options, sendOptions);
                        Ids.Push(c.wallBottom.GetPhotonView().ViewID);
                    }

                    if(!Maze.Cells[x, y].WallLeft)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallLeft.GetPhotonView().ViewID, options, sendOptions);
                        Ids.Push(c.wallLeft.GetPhotonView().ViewID);
                    }
                    if(!Maze.Cells[x, y].WallCenter)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallCenter.GetPhotonView().ViewID, options, sendOptions);
                        Ids.Push(c.wallCenter.GetPhotonView().ViewID);
                    }
                }
            }

            for (var x = 0; x <= Maze.Cells.GetLength(0) * 2 - 1; x++)
            {
                for (var y = 0; y <= Maze.Cells.GetLength(1) * 2 - 1; y++)
                {
                    if (x == 0 || y == 0 || x == Maze.Cells.GetLength(0) * 2 - 1 ||
                        y == Maze.Cells.GetLength(1) * 2 - 1)
                    {
                        PhotonNetwork
                            .Instantiate(FrameBlock.name, new Vector3(x * 10, 0.125f, y * 10), Quaternion.identity);
                    }
                }
            }
            var cnt = Ids.Count;
            for (var i = 0; i < cnt; i++)
            {
                var destroy = PhotonView.Find(Ids.Pop()).gameObject;
                destroy.SetActive(false);
            }

            PhotonNetwork.RaiseEvent(99, null, new RaiseEventOptions {Receivers = ReceiverGroup.Others}, new SendOptions {Reliability = true});
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                var spawned = PhotonNetwork.Instantiate(_tank.name, _spawn1.transform.position,
                    Quaternion.identity);
                spawned.transform.Rotate(0, -135, 0);
            }
        }
        
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 69:
                    Ids.Push((int)photonEvent.CustomData);
                    break;
                case 99:
                    var cnt = Ids.Count;
                    for (var i = 0; i < cnt; i++)
                    {
                        var destroy = PhotonView.Find(Ids.Pop()).gameObject;
                        destroy.SetActive(false);
                    }

                    GameObject spawned_;

                    switch (PhotonNetwork.LocalPlayer.ActorNumber)
                    { 
                        case 2:
                            spawned_ = PhotonNetwork.Instantiate(_tank.name, _spawn2.transform.position, Quaternion.identity);
                            spawned_.transform.Rotate(0, 45, 0);
                            break;
                        case 3:
                            spawned_ = PhotonNetwork.Instantiate(_tank.name, _spawn3.transform.position, Quaternion.identity);
                            spawned_.transform.Rotate(0, -45, 0);
                            break;
                        case 4:
                            spawned_ = PhotonNetwork.Instantiate(_tank.name, _spawn4.transform.position, Quaternion.identity);
                            spawned_.transform.Rotate(0, 135, 0);
                            break;
                    }
                    break;
                case 2:
                    _spawn2.transform.position = (Vector3)photonEvent.CustomData;
                    break;
                case 3:
                    _spawn3.transform.position = (Vector3)photonEvent.CustomData;
                    break;
                case 4:
                    _spawn4.transform.position = (Vector3)photonEvent.CustomData;
                    break;
            }
        }

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }
}