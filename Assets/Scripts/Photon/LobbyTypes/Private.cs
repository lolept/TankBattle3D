using ExitGames.Client.Photon;
using Game;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Photon.LobbyTypes
{
    public class Private : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private Player[] _player;
        private PlayerInfo[] _players;
        private PlayerStats[] _hps;
        private const int MinPlayers = 2;

        private Button _readyBtn;
        private Text _roomName;
        private Slider _size;
        private Text _sizeText;
        private Button _startBtn;
        private GameObject _timer;
        
        private Sprite[] _startBtnVariants;
        private Sprite[] _readyBtnVariants;
        
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
                    break;
                case 9:
                    _startBtn.interactable = (bool) photonEvent.CustomData;
                    _startBtn.image.sprite = _startBtn.interactable ? _startBtnVariants[0] : _startBtnVariants[1];
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
            _timer = gameManagement.timer;
            _startBtnVariants = gameManagement.startBtnVariants;
            _readyBtnVariants = gameManagement.readyBtnVariants;
            
            _timer.SetActive(false);
            
            Application.targetFrameRate = 360;
            QualitySettings.vSyncCount = 0;
        }

        public void StartRoom()
        {
            _roomName.text = PhotonNetwork.CurrentRoom.Name;
            _size.interactable = PhotonNetwork.IsMasterClient;
            _startBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            _readyBtn.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            
            _player = new Player[PhotonNetwork.CurrentRoom.MaxPlayers];
            PhotonNetwork.CurrentRoom.Players.Values.CopyTo(_player, 0);
            for (var i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                _players[i].SetNickname(_player[i].NickName);
                gameManagement.healthPoints.Add(_player[i].NickName, 100);
                Debug.Log(gameManagement.healthPoints.ToStringFull());
                _hps[i].SetNickname(_player[i].NickName);
                _players[i].SetMasterAccount(_player[i].IsMasterClient);
            }

            _startBtn.interactable = CheckPlayers();
            _startBtn.image.sprite = _startBtn.interactable ? _startBtnVariants[0] : _startBtnVariants[1];
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
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetReady(false);
            _players[PhotonNetwork.CurrentRoom.PlayerCount - 1].gameObject.GetComponentInChildren<TextAnimation>().StopAllCoroutines();
            _hps[PhotonNetwork.CurrentRoom.PlayerCount - 1].SetNickname(newPlayer.NickName);
            
            var ind = GetIndexOf(PhotonNetwork.NickName);
            _players[ind].SetReady(false);
            
            _startBtn.interactable = CheckPlayers();
            _startBtn.image.sprite = _startBtn.interactable ? _startBtnVariants[0] : _startBtnVariants[1];
            
            if (PhotonNetwork.IsMasterClient) UpdateSize();
            
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
                _players[i].SetReady(false);
                _players[i].SetMasterAccount(_player[i].IsMasterClient);
            }

            _startBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            _size.interactable = PhotonNetwork.IsMasterClient;
            _readyBtn.gameObject.SetActive(!PhotonNetwork.IsMasterClient);
            
            _players[PhotonNetwork.CurrentRoom.PlayerCount].SetNickname("");
            _players[PhotonNetwork.CurrentRoom.PlayerCount].SetReady(false);
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

            _startBtn.interactable = CheckPlayers();
            _startBtn.image.sprite = _startBtn.interactable ? _startBtnVariants[0] : _startBtnVariants[1];
        }

        private int GetIndexOf(string value)
        {
            for (var i = 0; i < _players.Length; i++)
                if (_players[i].GetNickname() == value)
                    return i;
            return -1;
        }

        public void UpdateSize()
        {
            var value = (int) _size.value * 2;
            _sizeText.text = value.ToString();
            PhotonNetwork.RaiseEvent(7, value,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        public void KickPlayer(int ind)
        {
            Debug.LogFormat("Kick {0} with id {1}", _players[ind].GetNickname(), ind);
            PhotonNetwork.RaiseEvent(0, _players[ind].GetNickname(),
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        public void SetReady()
        {
            var ind = GetIndexOf(PhotonNetwork.NickName);
            _players[ind].SetReady(!_players[ind].GetReady());
            _players[ind].RaiseReady();
            
            _startBtn.interactable = CheckPlayers();
            
            _readyBtn.image.sprite = _players[ind].GetReady() ? _readyBtnVariants[0] : _readyBtnVariants[1];
            _startBtn.image.sprite = _startBtn.interactable ? _startBtnVariants[0] : _startBtnVariants[1];
            
            PhotonNetwork.RaiseEvent(9, _startBtn.interactable,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        private bool CheckPlayers()
        {
            return (_players[0].GetReady() || _players[0].GetMasterAccount() ||
                    _players[0].GetNickname().Equals("")) &&
                   (_players[1].GetReady() || _players[1].GetMasterAccount() ||
                    _players[1].GetNickname().Equals("")) &&
                   (_players[2].GetReady() || _players[2].GetMasterAccount() ||
                    _players[2].GetNickname().Equals("")) &&
                   (_players[3].GetReady() || _players[3].GetMasterAccount() ||
                    _players[3].GetNickname().Equals("")) &&
                   PhotonNetwork.CurrentRoom.PlayerCount >= MinPlayers;
        }
    }
}
