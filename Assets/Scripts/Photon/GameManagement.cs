using System;
using System.Collections.Generic;
using Photon.LobbyTypes;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = Photon.LobbyTypes.Random;

namespace Photon
{
    public class GameManagement : MonoBehaviour
    {
        [SerializeField] public Dictionary<string, int> healthPoints = new Dictionary<string, int>();
        public PlayerInfo[] players;
        public PlayerStats[] hps;
        
        [Header("Ui")]
        public Button readyBtn;
        public Text roomName;
        public Slider size;
        public Text sizeText;
        public Button startBtn;
        public TMP_Text startTimer;
        public GameObject timer;
        
        [Header("Button variants")]
        public Sprite[] startBtnVariants;
        public Sprite[] readyBtnVariants;
        
        public Private privateRoom;
        public Random randomRoom;
        public Ranked rankedRoom;

        public enum RoomType
        {
            Random,
            Ranked,
            Private
        };

        public RoomType roomType;

        public bool started;

        private void Awake()
        {
            var roomNameStr = PhotonNetwork.CurrentRoom.Name;
            if (roomNameStr.Contains("Random"))
            {
                roomType = RoomType.Random;
                randomRoom = gameObject.AddComponent<Random>();
                randomRoom.StartRoom();
            }
            else if (roomNameStr.Contains("Ranked"))
            {
                roomType = RoomType.Ranked;
                rankedRoom = gameObject.AddComponent<Ranked>();
                rankedRoom.StartRoom();
            }
            else
            {
                roomType = RoomType.Private;
                privateRoom = gameObject.AddComponent<Private>();
                privateRoom.StartRoom();
            }
        }


        public void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void KickPlayer(int ind)
        {
            privateRoom.KickPlayer(ind);
        }

        public void SetReady()
        {
            switch (roomType)
            {
                case RoomType.Private:
                    privateRoom.SetReady();
                    break;
                case RoomType.Random:
                    break;
                case RoomType.Ranked:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateSize()
        {
            switch (roomType)
            {
                case RoomType.Private:
                    privateRoom.UpdateSize();
                    break;
                case RoomType.Random:
                    break;
                case RoomType.Ranked:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}