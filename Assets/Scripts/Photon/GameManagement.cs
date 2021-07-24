using System;
using System.Threading;
using Joystick_Pack.Scripts.Base;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Photon
{
    public class GameManagement : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startBtn;
        [SerializeField] private Joystick joystick;
        [SerializeField] private Button shootButton;
        [SerializeField] private Text sizeText;
        [SerializeField] private Slider size;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Debug.LogError(Application.targetFrameRate+"fps");
            Debug.LogError(QualitySettings.vSyncCount);
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
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
                startBtn.interactable = true;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
        }

        public void UpdateSize()
        {
            var value = (int)size.value * 2;
            sizeText.text = value.ToString();
        }
    }
}
