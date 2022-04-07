using System;
using System.Collections;
using System.Linq;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon;
using Photon.LobbyTypes;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Tank
{
    public class Tank : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject deadMenu;
        [SerializeField] private GameObject deathCam;
        [SerializeField] private GameObject minimapCircle;
        [SerializeField] private Image healthBar;
        public int healthPoints;
        public GameManagement gameManagement;
        [SerializeField] private GameObject inGameUi;
        [SerializeField] private GameObject minimapCamera;
        [SerializeField] private GameObject players;
        [SerializeField] private GameObject ready;
        [SerializeField] private GameObject start;
        private const string SetScoreUrl = "https://shicr.ru/BloxTank/PHP/SetMyScore.php";

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case 8:
                    var data8 = photonEvent.CustomData.Unwrap<string[]>();
                    var id = int.Parse(data8[0]);
                    if (id != -1)
                        if (gameObject.GetPhotonView().ViewID == id)
                            Destroy(gameObject);
                    break;

                case 11:
                    var a = photonEvent.CustomData.Unwrap<string[]>();
                    if (int.Parse(a[0]) == gameObject.GetPhotonView().ViewID)
                    {
                        healthPoints = int.Parse(a[1]);
                        gameManagement.healthPoints[gameObject.GetPhotonView().Controller.NickName] = healthPoints;
                        Debug.Log(gameManagement.healthPoints.ToStringFull());
                        var healths = new int[gameManagement.healthPoints.Count];
                        var names = new string[gameManagement.healthPoints.Count];
                        gameManagement.healthPoints.Values.CopyTo(healths, 0);
                        gameManagement.healthPoints.Keys.CopyTo(names, 0);
                        var alive = 0;
                        var aliveInd = 0;
                        for (var j = 0; j < healths.Length; j++)
                        {
                            if (healths[j] <= 0) continue;
                            alive++;
                            aliveInd = j;
                        }

                        if (alive <= 1)
                        {
                            PhotonNetwork.RaiseEvent(16, names[aliveInd],
                                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                                new SendOptions {Reliability = true});
                        }
                    }
                    break;
                case 12:
                    var data = new[] {gameObject.GetPhotonView().Owner.NickName, healthPoints.ToString()};
                    Send(data);
                    break;
                case 16:
                    if (!gameObject.GetPhotonView().IsMine) return;
                    background.SetActive(true);
                    players.SetActive(false);
                    deadMenu.SetActive(true);
                    start.SetActive(false);
                    ready.SetActive(false);
                    inGameUi.SetActive(false);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;
                    var winner = photonEvent.CustomData.ToString();
                    if (gameManagement.roomType == GameManagement.RoomType.Ranked)
                    {
                        var score = PlayerPrefs.GetInt("Rank", 0);
                        if (winner.Equals(PhotonNetwork.LocalPlayer.NickName))
                            score += 15;
                        else
                            score -= 5;
                        StartCoroutine(SetScore(score));
                    }

                    gameManagement.started = false;
                    var playersEnd = deadMenu.GetComponentsInChildren<PlayerStats>();
                    foreach (var pl in playersEnd)
                    {
                        pl.SetWinner(pl.GetNickname().Equals(winner));
                    }

                    var hr = gameObject.GetComponent<HeadRotation>();
                    hr.mouseLook.SetCursorLock(false);
                    
                    break;
            }
        }

        private void Send(IEnumerable data)
        {
            PhotonNetwork.RaiseEvent(15, data,
                new RaiseEventOptions {Receivers = ReceiverGroup.Others},
                new SendOptions {Reliability = true});
        }

        private void Awake()
        {
            gameManagement = FindObjectOfType<GameManagement>();
            minimapCamera = FindObjectOfType<minimapCamera>(true).gameObject;
            if (!gameObject.GetPhotonView().IsMine)
            {
                Destroy(minimapCamera);
                minimapCircle.SetActive(false);
                return;
            }

            healthBar = FindObjectOfType<healthBar>(true).GetComponent<Image>();
            deathCam = FindObjectOfType<DeathCamera>(true).gameObject;
            players = FindObjectOfType<PlayersList>(true).gameObject;
            background = FindObjectOfType<Background>(true).gameObject;
            deadMenu = FindObjectOfType<DeadList>(true).gameObject;
            ready = FindObjectOfType<ReadyBtn>(true).gameObject;
            start = FindObjectOfType<StartBtn>(true).gameObject;
            inGameUi = FindObjectOfType<InGameUI>(true).gameObject;
        }

        private void OnHit()
        {
            if (!gameObject.GetPhotonView().IsMine) return;
            StartCoroutine(SmoothAnim());
            Debug.Log(healthPoints);
            var data = new[]
                {gameObject.GetPhotonView().ViewID.ToString(), healthPoints.ToString(), PhotonNetwork.NickName};
            PhotonNetwork.RaiseEvent(11, data,
                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
            if (healthPoints <= 0)
                Death();
        }

        private void Death()
        {
            if (!gameObject.GetPhotonView().IsMine) return;
            deathCam.SetActive(true);
            deathCam.transform.position = gameObject.transform.position;
            background.SetActive(true);
            players.SetActive(false);
            deadMenu.SetActive(true);
            start.SetActive(false);
            ready.SetActive(false);
            inGameUi.SetActive(false);
            PhotonNetwork.RaiseEvent(8, gameObject.GetPhotonView().ViewID,
                new RaiseEventOptions {Receivers = ReceiverGroup.All},
                new SendOptions {Reliability = true});
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!gameObject.GetPhotonView().IsMine) return;
            switch (other.tag)
            {
                case "Bullet":
                    var obj = other.gameObject;
                    var id = obj.GetPhotonView().ViewID;
                    healthPoints -= obj.GetComponent<BulletTank>().damage;
                
                    var data = new[] {id.ToString(), obj.name};
                    PhotonNetwork.RaiseEvent(8, data, new RaiseEventOptions {Receivers = ReceiverGroup.All},
                        new SendOptions {Reliability = true});
                    
                    OnHit();
                    break;
            }
        }
        
        private IEnumerator SmoothAnim()
        {
            while (healthBar.fillAmount > healthPoints / 100f)
            {
                healthBar.fillAmount -= 0.05f;
                yield return new WaitForSeconds(0.05f);
            }
        }

        private IEnumerator SetScore(int score)
        {
            var form = new WWWForm();
            form.AddField("Name", PlayerPrefs.GetString("Name", ""));
            form.AddField("Password", PlayerPrefs.GetString("Password", ""));
            form.AddField("Rank", score);
            var www = UnityWebRequest.Post(SetScoreUrl, form);
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
                    Debug.Log("Succes: 00");
                    Debug.Log(score);
                    PlayerPrefs.SetInt("Rank", score);
                    break;
                case 1:
                    Debug.LogError("Player does nor exist error: 01");
                    break;
                case 2:
                    Debug.LogError("Wrong password error: 02");
                    break;
                case 3:
                    Debug.LogError("Request error: 03");
                    break;
            }
        }
    }
}