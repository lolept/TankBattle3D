using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Photon
{
    public class PlayerInfo : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private bool _isMasterAccount;
        private bool _isReady;
        private string _nickname = "";
        [SerializeField] private GameObject kick;
        [SerializeField] private GameObject master;
        [SerializeField] private Color masterColor;
        [SerializeField] private Text nickText;
        [SerializeField] private Color notReady;
        [SerializeField] private Color ready;

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 1:
                    if (_nickname == photonEvent.CustomData.ToString() && !_isMasterAccount) SetReady(!_isReady);
                    break;
            }
        }

        public void SetNickname(string value)
        {
            _nickname = value;
            nickText.text = value;
        }

        public string GetNickname()
        {
            return _nickname;
        }

        public void SetReady(bool value)
        {
            _isReady = value;
            var color = value ? ready : notReady;
            color = _isMasterAccount ? masterColor : color;
            nickText.color = color;
        }
        
        public void SetEmpty()
        {
            var color = new Color(236f/255f, 240f/255f, 241f/255f);
            nickText.color = color;
        }

        public void RaiseReady()
        {
            PhotonNetwork.RaiseEvent(1, _nickname,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        public bool GetReady()
        {
            return _isReady;
        }

        public bool GetMasterAccount()
        {
            return _isMasterAccount;
        }

        public void SetMasterAccount(bool value)
        {
            _isMasterAccount = value;
            master.SetActive(value);
            nickText.color = value ? masterColor : notReady;
            kick.SetActive(!value && PhotonNetwork.IsMasterClient);
        }

        public void ClearIcons()
        {
            master.SetActive(false);
            kick.SetActive(false);
        }
    }
}