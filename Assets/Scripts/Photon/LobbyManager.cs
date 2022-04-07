using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 618

namespace Photon
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        private const string GetScoreUrl = "https://shicr.ru/BloxTank/PHP/GetMyScore.php";
        
        private int _num = 1;
        
        private bool _random;
        private bool _ranked;
        
        [SerializeField] private Sprite[] connected;
        
        [SerializeField] private Button privateLobby;
        [SerializeField] private Button randomGame;
        [SerializeField] private Button rankedGame;
        
        [SerializeField] private InputField roomName;
        
        [SerializeField] private TMP_Text rank;
        
        private IEnumerator _getScore()
        {
            var form = new WWWForm();
            form.AddField("Name", PlayerPrefs.GetString("Name", ""));
            form.AddField("Password", PlayerPrefs.GetString("Password", ""));
            var www = UnityWebRequest.Post(GetScoreUrl, form);
            yield return www.SendWebRequest();
            var text = www.downloadHandler.text;
            Debug.Log(text);
            if (www.error != null) yield break;
            var code = int.Parse(text.Split()[0]);
            switch (code)
            {
                case -1:
                    Debug.LogError("Connection error: -01");
                    break;
                case 0:
                    PlayerPrefs.SetInt("Rank", int.Parse(text.Split(";")[1]));
                    rank.text = text.Split(";")[1];
                    Debug.Log("Succes: 00");
                    break;
                case 1:
                    Debug.LogError("User does not exist error: 01");
                    break;
                case 2:
                    Debug.LogError("Wrong password error: 02");
                    break;
                case 3:
                    Debug.LogError("Request error: 03");
                    break;
            }
        }

        private void Start()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
        }

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = Application.version;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = PlayerPrefs.GetString("Name", "");
            StartCoroutine(_getScore());
        }

        public override void OnConnectedToMaster()
        {
            randomGame.image.sprite = connected[0];
            rankedGame.image.sprite = connected[1];
            privateLobby.image.sprite = connected[2];
            randomGame.interactable = true;
            rankedGame.interactable = true;
            privateLobby.interactable = true;
        }

        public void CreateRoom()
        {
            if (roomName.text == "")
                roomName.text = PhotonNetwork.LocalPlayer.NickName;
            PhotonNetwork.CreateRoom(roomName.text, new RoomOptions {MaxPlayers = 4, PublishUserId = true});
        }

        public void JoinRoom()
        {
            if (roomName.text != "")
                PhotonNetwork.JoinRoom(roomName.text);
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.LoadLevel("Game");
        }

        
        public void JoinRandomRoom()
        {
            _random = true;
            PhotonNetwork.JoinOrCreateRoom("Random " + _num, new RoomOptions {MaxPlayers = 4, PublishUserId = false},
                TypedLobby.Default);
        }
        
        
        public void JoinRankedRoom()
        {
            _ranked = true;
            var playerRank = PlayerPrefs.GetInt("Rank", 150) / 100;
            PhotonNetwork.JoinOrCreateRoom("Ranked  " + _num + ";" + playerRank, new RoomOptions {MaxPlayers = 4, PublishUserId = false},
                TypedLobby.Default);
        }
        

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            if (returnCode == 32765 || returnCode == 32764)
            {
                _num++;
                if (_random)
                    JoinRandomRoom();
                else if (_ranked)
                    JoinRankedRoom();
                return;
            }
            Debug.LogErrorFormat("Joining room failed with error code {0}:\n{1}", returnCode, message);
        }


        public void Quit()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}