using System.Collections;
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

        public static Stack<int> Destroying;
        public int destLength;

        public static Maze Maze;

        private void Start()
        {
            Destroying = new Stack<int>();
            SampleWall = sampleWall;
            LuckyWall = luckyWall;
            IronWall = ironWall;
            FrameBlock = frameBlock;
            Spawn();
        }

        public static void Spawn()
        {
            if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom.PlayerCount != 2) return;
            Thread.Sleep(100);
            MazeGenerator generator = new MazeGenerator();
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
                    if (!Maze.Cells[x, y].WallBottom && c.wallBottom.GetPhotonView().IsMine)
                        PhotonNetwork.RaiseEvent(69, c.wallBottom.GetPhotonView().ViewID, options, sendOptions);
                    if(!Maze.Cells[x, y].WallLeft && c.wallLeft.GetPhotonView().IsMine)
                        PhotonNetwork.RaiseEvent(69, c.wallLeft.GetPhotonView().ViewID, options, sendOptions);
                    if(!Maze.Cells[x, y].WallCenter && c.wallCenter.GetPhotonView().IsMine)
                        PhotonNetwork.RaiseEvent(69, c.wallCenter.GetPhotonView().ViewID, options, sendOptions);
                    c.wallBottom.SetActive(Maze.Cells[x, y].WallBottom);
                    c.wallLeft.SetActive(Maze.Cells[x, y].WallLeft);
                    c.wallCenter.SetActive(Maze.Cells[x, y].WallCenter);
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
        }
        
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 69:
                    GameObject toDestroy;
                    toDestroy = PhotonView.Find((int) photonEvent.CustomData).gameObject;
                    toDestroy.SetActive(false);
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