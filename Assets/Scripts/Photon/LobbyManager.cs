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
        public InputField piska;

        private void Start()
        {
            PhotonNetwork.NickName = "Player" + Random.Range(1, 99999);
            Log("Players name is " + PhotonNetwork.NickName);

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
            if(piska.text.Equals("")) return;
            PhotonNetwork.CreateRoom(piska.text, new RoomOptions {MaxPlayers = 2});
        }

        public void JoinRoom()
        {
            PhotonNetwork.JoinRoom(roomName.text);
        }

        public override void OnJoinedRoom()
        {
            Log("Joined the room");
            
            PhotonNetwork.LoadLevel("Game");
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
