using System;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Game;
using Game.Tank;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private readonly Color _low = new Color(0.91f, 0.3f, 0.24f);
    private readonly Color _lowBackground = new Color(0.75f, 0.22f, 0.17f);
    private readonly Color _normal = new Color(0.18f, 0.8f, 0.44f);
    private readonly Color _normalBackground = new Color(0.15f, 0.68f, 0.38f);
    public int health;
    private bool _isWinner;
    [SerializeField] private string nickname;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image winnerImage;
    [SerializeField] private Text nickName;

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case 11:
                var a = photonEvent.CustomData.Unwrap<string[]>();
                if (nickname == a[2])
                {
                    health = int.Parse(a[1]);
                    healthBar.fillAmount = health / 100f;
                    healthBar.color = health <= 20 ? _low : _normal;
                    healthBar.transform.parent.GetComponent<Image>().color =
                        health <= 20 ? _lowBackground : _normalBackground;
                }
                break;
            case 15:
                var data = photonEvent.CustomData.Unwrap<string[]>();
                if (data[0].Equals(nickname))
                {
                    health = int.Parse(data[1]);
                    healthBar.fillAmount = health / 100f;
                    healthBar.color = health <= 20 ? _low : _normal;
                    healthBar.transform.parent.GetComponent<Image>().color =
                        health <= 20 ? _lowBackground : _normalBackground;
                }
                break;
        }
    }
    
    public void SetNickname(string value)
    {
        nickname = value;
        nickName.text = nickname;
        SetEmpty(value.Length == 0);
    }

    public string GetNickname()
    {
        return nickname;
    }

    public void SetWinner(bool a)
    {
        _isWinner = a;
        winnerImage.gameObject.SetActive(a);
    }
    
    public bool GetWinner()
    {
        return _isWinner;
    }

    public void SetEmpty(bool value)
    {
        gameObject.SetActive(!value);
    }

    public void GoToCamera()
    {
        var localSpawner = FindObjectOfType<MazeSpawner>();
        localSpawner.ActivateCamera(nickname);
    }

    public override void OnEnable()
    {
        //base.OnEnable();
        var tanks = FindObjectsOfType<Tank>(true);
        foreach (var tank in tanks)
        {
            if (tank.gameObject.GetPhotonView().Owner.NickName == nickname)
            {
                health = tank.healthPoints;
                healthBar.fillAmount = health / 100f;
                healthBar.color = health <= 20 ? _low : _normal;
                healthBar.transform.parent.GetComponent<Image>().color =
                    health <= 20 ? _lowBackground : _normalBackground;
            }
        }
        PhotonNetwork.RaiseEvent(12, null,
            new RaiseEventOptions {Receivers = ReceiverGroup.Others},
            new SendOptions {Reliability = true});
    }
}