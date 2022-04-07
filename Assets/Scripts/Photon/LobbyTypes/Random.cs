using ExitGames.Client.Photon;
using Game;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Photon.LobbyTypes
{
    public class Random : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private Player[] _player;
        private PlayerInfo[] _players;
        private PlayerStats[] _hps;

        private Text _roomName;
        private Text _sizeText;
        private TMP_Text _startTimer;
        private Button _readyBtn;
        private Button _startBtn;
        private Slider _size;

        private bool _readyToStart;
        private bool _started;
        private const int MinPlayers = 2;
        private float _startTimerCounter = 5f;
        private int _playersAlive;

        public GameManagement gameManagement;

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 0:
                    if (PhotonNetwork.NickName.Equals(photonEvent.CustomData.ToString())) PhotonNetwork.LeaveRoom();
                    break;
                case 7:
                    if (PhotonNetwork.IsMasterClient) return;
                    _sizeText.text = photonEvent.CustomData.ToString();
                    _size.value = float.Parse(photonEvent.CustomData.ToString()) / 2;
                    PlayerPrefs.SetInt("MapSize", (int)photonEvent.CustomData);
                    break;
                case 14:
                    _startTimerCounter = (float)photonEvent.CustomData;
                    _readyToStart = true;
                    break;
            }
        }

        private void Awake()
        {
            gameManagement = gameObject.GetComponent<GameManagement>();
            _players = gameManagement.players;
            _hps = gameManagement.hps;
            _readyBtn = gameManagement.readyBtn;
            _roomName = gameManagement.roomName;
            _size = gameManagement.size;
            _sizeText = gameManagement.sizeText;
            _startBtn = gameManagement.startBtn;
            _startTimer = gameManagement.startTimer;
            
            Application.targetFrameRate = 360;
            QualitySettings.vSyncCount = 0;
        }

        public void StartRoom()
        {
            _roomName.text = PhotonNetwork.CurrentRoom.Name;
            _size.interactable = false;
            if (PhotonNetwork.IsMasterClient)
            {
                _size.value = UnityEngine.Random.Range(10, 16);
                UpdateSize();
            }
            _startBtn.gameObject.SetActive(false);
            _readyBtn.gameObject.SetActive(false);
            
            _player = new Player[PhotonNetwork.CurrentRoom.MaxPlayers];
            PhotonNetwork.CurrentRoom.Players.Values.CopyTo(_player, 0);
            for (var i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                _players[i].SetNickname(_player[i].NickName);
                gameManagement.healthPoints.Add(_player[i].NickName, 100);
                Debug.Log(gameManagement.healthPoints.ToStringFull());
                _players[i].SetMasterAccount(false);
                _players[i].SetReady(true);
                _players[i].gameObject.GetComponentInChildren<Button>(true).gameObject.SetActive(false);
                _hps[i].SetNickname(_player[i].NickName);
            }
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
            
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetNickname(newPlayer.NickName);
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetMasterAccount(false);
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetReady(true);
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject.GetComponentInChildren<Button>().gameObject.SetActive(false);
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject.GetComponentInChildren<TextAnimation>().StopAllCoroutines();
            _hps[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetNickname(newPlayer.NickName);
            
            var ind = GetIndexOf(PhotonNetwork.NickName);
            _players[ind].SetReady(true);

            if (PhotonNetwork.IsMasterClient) UpdateSize();

            if (!CheckPlayers()) return;
            _startTimerCounter = 20f;
            _readyToStart = true;
            
            PhotonNetwork.RaiseEvent(14, _startTimerCounter,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
            
            gameManagement.healthPoints.Add(newPlayer.NickName, 100);
            Debug.Log(gameManagement.healthPoints.ToStringFull());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            gameManagement.healthPoints.Remove(otherPlayer.NickName);
            Debug.Log(gameManagement.healthPoints.ToStringFull());
            
            if (gameManagement.healthPoints.Count <= 1 && gameManagement.started)
            {
                PhotonNetwork.RaiseEvent(16, PhotonNetwork.LocalPlayer.NickName,
                    new RaiseEventOptions {Receivers = ReceiverGroup.All},
                    new SendOptions {Reliability = true});
            }
            
            Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
            
            for (var i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                _players[i].SetNickname(_player[i].NickName);
                _hps[i].SetNickname(_player[i].NickName);
                _hps[i].SetWinner(_hps[i+1].GetWinner());
                _players[i].SetReady(true);
                _players[i].gameObject.GetComponentInChildren<Button>(true).gameObject.SetActive(false);
            }
            
            _players[PhotonNetwork.CurrentRoom.PlayerCount].SetNickname("");
            _players[PhotonNetwork.CurrentRoom.PlayerCount].SetEmpty();
            _players[PhotonNetwork.CurrentRoom.PlayerCount].ClearIcons();
            _hps[PhotonNetwork.CurrentRoom.PlayerCount].SetEmpty(true);
            
            var data = new[] {otherPlayer.NickName, "0"};
            PhotonNetwork.RaiseEvent(15, data,
                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
            
            for (int i = PhotonNetwork.CurrentRoom.PlayerCount; i < 4; i++)
            {
                _players[i].gameObject.GetComponentInChildren<TextAnimation>().StopAllCoroutines();
                _players[i].gameObject.GetComponentInChildren<Text>().text = "Waiting";
                _players[i].gameObject.GetComponentInChildren<TextAnimation>().Start();
            }

            if (CheckPlayers()) return;
            _startTimerCounter = 30;
            _readyToStart = false;
            _startTimer.text = "30";
        }

        private int GetIndexOf(string value)
        {
            for (var i = 0; i < _players.Length; i++)
                if (_players[i].GetNickname() == value)
                    return i;
            return -1;
        }

        private void UpdateSize()
        {
            var value = (int) _size.value * 2;
            _sizeText.text = value.ToString();
            PlayerPrefs.SetInt("MapSize", value);
            PhotonNetwork.RaiseEvent(7, value,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        private static bool CheckPlayers()
        {
            return PhotonNetwork.CurrentRoom.PlayerCount >= MinPlayers;
        }


        private void Update()
        {
            if(_started) return;
            if (_startTimerCounter <= 0)
            {
                _started = true;
                gameManagement.started = true;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                var spawner = gameObject.GetComponent<MazeSpawner>();
                spawner.Spawn();
            }
            if (!_readyToStart) return;
            _startTimerCounter -= Time.deltaTime;
            _startTimer.text = ((int)_startTimerCounter).ToString();
        }
    }
}
