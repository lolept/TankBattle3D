using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityStandardAssets.Cameras;

namespace Game
{
    public class MazeSpawner : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private  int _type;
        private  readonly Vector3 _cellSize = new Vector3(20, 0, 20);
        private  Maze _maze;
        
        private  Stack<int[]> _ids;
        
        [SerializeField] private Dictionary<string, Tank.Tank> tanks;
        
        [Header("Ui")]
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject inGameUi;
        
        [Header("Cells")]
        [SerializeField] private Cell ironWall;
        [SerializeField] private Cell luckyWall;
        [SerializeField] private Cell sampleWall;
        
        [Header("Game objects")]
        [SerializeField] private GameObject frameBlock;
        [SerializeField] private GameObject tank;
        private GameObject _spawn1, _spawn2, _spawn3, _spawn4;

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 2:
                    _spawn2.transform.position = (Vector3) photonEvent.CustomData;
                    break;
                case 3:
                    _spawn3.transform.position = (Vector3) photonEvent.CustomData;
                    break;
                case 4:
                    _spawn4.transform.position = (Vector3) photonEvent.CustomData;
                    break;
                case 5:
                    var data = photonEvent.CustomData.Unwrap<int[]>();
                    _ids.Push(data);
                    break;
                case 6:
                    var cnt = _ids.Count;
                    for (var i = 0; i < cnt; i++)
                    {
                        var toDestroy = _ids.Pop();
                        switch (toDestroy[1])
                        {
                            case 1:
                                var destroy1 = PhotonView.Find(toDestroy[0]).gameObject.GetComponent<Cell>().wallBottom;
                                Destroy(destroy1);
                                break;
                            case 2:
                                var destroy2 = PhotonView.Find(toDestroy[0]).gameObject.GetComponent<Cell>().wallLeft;
                                Destroy(destroy2);
                                break;
                            case 3:
                                var destroy3 = PhotonView.Find(toDestroy[0]).gameObject.GetComponent<Cell>().wallCenter;
                                Destroy(destroy3);
                                break;
                        }
                    }

                    var id = 1;
                    var players = (Player[])photonEvent.CustomData;
                    foreach (var t in players)
                    {
                        tanks.Add(t.NickName, null);
                    }
                    foreach (var variable in players)
                    {
                        if (variable.UserId == PhotonNetwork.LocalPlayer.UserId)
                        {
                            GameObject spawned;
                            switch (id)
                            {
                                case 1:
                                    spawned = PhotonNetwork.Instantiate(tank.name, _spawn1.transform.position,
                                        Quaternion.identity);
                                    spawned.transform.Rotate(0, 45, 0);
                                    break;
                                case 2:
                                    spawned = PhotonNetwork.Instantiate(tank.name, _spawn2.transform.position,
                                        Quaternion.identity);
                                    spawned.transform.Rotate(0, -135, 0);
                                    break;
                                case 3:
                                    spawned = PhotonNetwork.Instantiate(tank.name, _spawn3.transform.position,
                                        Quaternion.identity);
                                    spawned.transform.Rotate(0, 135, 0);
                                    break;
                                case 4:
                                    spawned = PhotonNetwork.Instantiate(tank.name, _spawn4.transform.position,
                                        Quaternion.identity);
                                    spawned.transform.Rotate(0, -45, 0);
                                    break;
                            }
                            break;
                        }
                        id++;
                    }
                    var spawnedTanks = FindObjectsOfType<Tank.Tank>();
                    foreach (var variable in spawnedTanks)
                    {
                        tanks[variable.gameObject.GetPhotonView().Owner.NickName] = variable;
                    }
                    break;
                case 10:
                    background.SetActive(false);
                    inGameUi.SetActive(true);
                    break;
                case 13:
                    var size = (int)photonEvent.CustomData;
                    for (var x = 0; x <= size; x++)
                    for (var y = 0; y <= size; y++)
                        if (x == 0 || y == 0 || x == size || y == size)
                            Instantiate(frameBlock, new Vector3(x * 10, 0.125f, y * 10), Quaternion.identity);
                    break;
            }
        }

        private void Awake()
        {
            tanks = new Dictionary<string, Tank.Tank>();
            var spawns = FindObjectsOfType<Spawn>();
            _spawn1 = spawns[1].gameObject;
            _spawn2 = spawns[2].gameObject;
            _spawn3 = spawns[0].gameObject;
            _spawn4 = spawns[3].gameObject;
            _ids = new Stack<int[]>();
        }

        public void Spawn()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var gm = gameObject.GetComponent<GameManagement>();
            gm.started = true;
            var generator = gameObject.AddComponent<MazeGenerator>();
            _maze = generator.GenerateMaze();

            for (var x = 0; x < _maze.Cells.GetLength(0); x++)
            for (var y = 0; y < _maze.Cells.GetLength(1); y++)
            {
                Cell c;
                _type = Random.Range(0, 15);
                if (_type == 0)
                {
                    c = PhotonNetwork.InstantiateRoomObject(luckyWall.name,
                        new Vector3(x * _cellSize.x, 0.125f, y * _cellSize.z),
                        Quaternion.identity).GetComponent<Cell>(); 
                }
                else if (_type > 0 && _type < 4)
                {
                    c = PhotonNetwork.InstantiateRoomObject(ironWall.name,
                        new Vector3(x * _cellSize.x, 0.125f, y * _cellSize.z),
                        Quaternion.identity).GetComponent<Cell>();
                }
                else
                {
                    c = PhotonNetwork.InstantiateRoomObject(sampleWall.name,
                        new Vector3(x * _cellSize.x, 0.125f, y * _cellSize.z),
                        Quaternion.identity).GetComponent<Cell>();
                }
                var options = new RaiseEventOptions {Receivers = ReceiverGroup.Others};
                var sendOptions = new SendOptions {Reliability = true};
                if (x == _maze.Cells.GetLength(0) - 1 && y == _maze.Cells.GetLength(1) - 1)
                {
                    var position = c.wallCenter.transform.position;
                    _spawn2.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);
                    PhotonNetwork.RaiseEvent(2, _spawn2.transform.position, options, sendOptions);
                }

                if (x == _maze.Cells.GetLength(0) - 1 && y == 0)
                {
                    var position = c.wallLeft.transform.position;
                    _spawn4.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);
                    PhotonNetwork.RaiseEvent(4, _spawn4.transform.position, options, sendOptions);
                }

                if (x == 0 && y == _maze.Cells.GetLength(1) - 1)
                {
                    var position = c.wallBottom.transform.position;
                    _spawn3.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);
                    PhotonNetwork.RaiseEvent(3, _spawn3.transform.position, options, sendOptions);
                }

                if (!_maze.Cells[x, y].WallBottom)
                {
                    var data1 = new[] {c.gameObject.GetPhotonView().ViewID, 1};
                    PhotonNetwork.RaiseEvent(5, data1, options, sendOptions);
                    _ids.Push(data1);
                }

                if (!_maze.Cells[x, y].WallLeft)
                {
                    var data2 = new[] {c.gameObject.GetPhotonView().ViewID, 2};
                    PhotonNetwork.RaiseEvent(5, data2, options, sendOptions);
                    _ids.Push(data2);
                }

                if (_maze.Cells[x, y].WallCenter) continue;
                var data3 = new[] {c.gameObject.GetPhotonView().ViewID, 3};
                PhotonNetwork.RaiseEvent(5, data3, options, sendOptions);
                _ids.Push(data3);
            }

            PhotonNetwork.RaiseEvent(13, _maze.Cells.GetLength(0) * 2 - 1, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
            PhotonNetwork.RaiseEvent(10, null, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
            
            var players = new Player[PhotonNetwork.CurrentRoom.PlayerCount];
            PhotonNetwork.CurrentRoom.Players.Values.CopyTo(players, 0);
            
            PhotonNetwork.RaiseEvent(6, players, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
        }

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void ActivateCamera(string nickname)
        {
            tanks[nickname].transform.GetComponentInChildren<AutoCam>(true).gameObject.SetActive(true);
        }
    }
}