using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public Text logText;
        public InputField roomName;
        private bool _isFree = true;
        private bool _isCreated = true;
        private int _num = 1;
        private GameObject a;

        private void Start()
        {
            Log("Players name is " + PlayerPrefs.GetString("Name", "a"));

            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Log("Connected to master");
        }

        public void CreateRoom()
        {
            PhotonNetwork.CreateRoom(PlayerPrefs.GetString("Name", "a"), new RoomOptions {MaxPlayers = 2});
        }

        public void JoinRoom()
        {
            PhotonNetwork.JoinRoom(roomName.text);
        }

        public override void OnJoinedRoom()
        {
            Log("Joined the room");
            _isCreated = true;
            _isFree = true;
            PhotonNetwork.LoadLevel("Game");
        }

        public void JoinRandomRoom()
        {
            if (!_isCreated)
            {
                PhotonNetwork.CreateRoom("Random_" + _num, new RoomOptions {MaxPlayers = 4});
                return;
            }
            if (!_isFree)
            {
                _num++;
                //JoinRandomRoom();
            }
            PhotonNetwork.JoinRoom("Random_" + _num);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log(message);
            if (message.Equals("Game does not exist"))
            {
                _isCreated = false;
                JoinRandomRoom();
            }
            if (message.Equals("Game full"))
            {
                _isFree = false;
                JoinRandomRoom();
            }
        }

        private void Log(string message)
        {
            Debug.Log(message);
            logText.text += "\n";
            // ReSharper disable once Unity.InefficientPropertyAccess
            logText.text += message;
        }
    }
}
