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
        [SerializeField] private Cell sampleWall;
        [SerializeField] private Cell luckyWall;
        [SerializeField] private Cell ironWall;
        [SerializeField] private GameObject frameBlock;
        [SerializeField] private GameObject spawn1, spawn2, spawn3, spawn4;
        [SerializeField] private GameObject tank;
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject gameControls;
        [SerializeField] private GameObject camera;
        private static Cell _sampleWall;
        private static Cell _luckyWall;
        private static Cell _ironWall;
        private static int _type;
        private static readonly Vector3 CellSize = new Vector3(20, 0, 20);
        private static Maze _maze;
        private static Stack<int> _ids;
        private static GameObject _frameBlock;
        private static GameObject _spawn1, _spawn2, _spawn3, _spawn4;
        private static GameObject _tank;
        private static GameObject _background;
        private static GameObject _gameControls;
        private static GameObject _camera;

        private void Awake()
        {
            _spawn1 = spawn1;
            _spawn2 = spawn2;
            _spawn3 = spawn3;
            _spawn4 = spawn4;
            _tank = tank;
            _ids = new Stack<int>();
            _sampleWall = sampleWall;
            _luckyWall = luckyWall;
            _ironWall = ironWall;
            _frameBlock = frameBlock;
            _background = background;
            _gameControls = gameControls;
            _camera = camera;
        }

        public static void Spawn()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Thread.Sleep(100);
            // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
            var generator = new MazeGenerator();
            _maze = generator.GenerateMaze();

            for (var x = 0; x < _maze.Cells.GetLength(0); x++)
            {
                for (var y = 0; y < _maze.Cells.GetLength(1); y++)
                {
                    Cell c;
                    _type = Random.Range(0, 15);
                    if (_type == 0)
                    {
                        c = PhotonNetwork.Instantiate(_luckyWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    else if (_type > 0 && _type < 4)
                    {
                        c = PhotonNetwork.Instantiate(_ironWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    else
                    {
                        c = PhotonNetwork.Instantiate(_sampleWall.name,
                            new Vector3(x * CellSize.x, 0.125f, y * CellSize.z),
                            Quaternion.identity).GetComponent<Cell>();
                    }
                    var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                    var sendOptions = new SendOptions {Reliability = true};
                    if (x == _maze.Cells.GetLength(0) - 1 && y == _maze.Cells.GetLength(1) - 1)
                    {
                        var position = c.wallCenter.transform.position;
                        _spawn2.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(2, _spawn2.transform.position, options, sendOptions);
                    }

                    if (x == _maze.Cells.GetLength(0) - 1 && y == 0)
                    {
                        var position = c.wallLeft.transform.position;
                        _spawn4.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(4, _spawn4.transform.position, options, sendOptions);
                    }

                    if (x == 0 && y == _maze.Cells.GetLength(1) - 1)
                    {
                        var position = c.wallBottom.transform.position;
                        _spawn3.transform.position = new Vector3(position.x, position.y + 1, position.z);
                        PhotonNetwork.RaiseEvent(3, _spawn3.transform.position, options, sendOptions);
                    }

                    if (!_maze.Cells[x, y].WallBottom)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallBottom.GetPhotonView().ViewID, options, sendOptions);
                        _ids.Push(c.wallBottom.GetPhotonView().ViewID);
                    }

                    if(!_maze.Cells[x, y].WallLeft)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallLeft.GetPhotonView().ViewID, options, sendOptions);
                        _ids.Push(c.wallLeft.GetPhotonView().ViewID);
                    }
                    if(!_maze.Cells[x, y].WallCenter)
                    {
                        PhotonNetwork.RaiseEvent(69, c.wallCenter.GetPhotonView().ViewID, options, sendOptions);
                        _ids.Push(c.wallCenter.GetPhotonView().ViewID);
                    }
                }
            }

            for (var x = 0; x <= _maze.Cells.GetLength(0) * 2 - 1; x++)
            {
                for (var y = 0; y <= _maze.Cells.GetLength(1) * 2 - 1; y++)
                {
                    if (x == 0 || y == 0 || x == _maze.Cells.GetLength(0) * 2 - 1 ||
                        y == _maze.Cells.GetLength(1) * 2 - 1)
                    {
                        PhotonNetwork
                            .Instantiate(_frameBlock.name, new Vector3(x * 10, 0.125f, y * 10), Quaternion.identity);
                    }
                }
            }
            var cnt = _ids.Count;
            for (var i = 0; i < cnt; i++)
            {
                var destroy = PhotonView.Find(_ids.Pop()).gameObject;
                Destroy(destroy);
                //destroy.SetActive(false);
            }
            _background.SetActive(false);
            _gameControls.SetActive(Application.platform == RuntimePlatform.Android);
            _camera.SetActive(false);

            PhotonNetwork.RaiseEvent(99, null, new RaiseEventOptions {Receivers = ReceiverGroup.Others}, new SendOptions {Reliability = true});
            if (PhotonNetwork.LocalPlayer.ActorNumber == 1)
            {
                var spawned = PhotonNetwork.Instantiate(_tank.name, _spawn1.transform.position,
                    Quaternion.identity);
                spawned.transform.Rotate(0, 45, 0);
            }
        }
        
        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 69:
                    _ids.Push((int)photonEvent.CustomData);
                    break;
                case 99:
                    var cnt = _ids.Count;
                    for (var i = 0; i < cnt; i++)
                    {
                        var destroy = PhotonView.Find(_ids.Pop()).gameObject;
                        Destroy(destroy);
                        //destroy.SetActive(false);
                    }

                    GameObject spawned;

                    switch (PhotonNetwork.LocalPlayer.ActorNumber)
                    { 
                        case 2:
                            spawned = PhotonNetwork.Instantiate(_tank.name, _spawn2.transform.position, Quaternion.identity);
                            spawned.transform.Rotate(0, -135, 0);
                            break;
                        case 3:
                            spawned = PhotonNetwork.Instantiate(_tank.name, _spawn3.transform.position, Quaternion.identity);
                            spawned.transform.Rotate(0, 135, 0);
                            break;
                        case 4:
                            spawned = PhotonNetwork.Instantiate(_tank.name, _spawn4.transform.position, Quaternion.identity);
                            spawned.transform.Rotate(0, -45, 0);
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