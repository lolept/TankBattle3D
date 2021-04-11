using System;
using System.Threading;
using Game;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Photon
{
    public class GameManagement : MonoBehaviourPunCallbacks
    {
        public Button StartBtn;
        public Joystick joystick;
        private void Start()
        {
            if(Application.platform != RuntimePlatform.Android)
                joystick.gameObject.SetActive(false);
        }

        public void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
            Thread.Sleep(1000);
        }

        private void Update()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
            {
                StartBtn.interactable = true;
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
        }
    }
}
