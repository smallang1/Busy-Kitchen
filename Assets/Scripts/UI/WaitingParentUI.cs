using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WaitingParentUI : MonoBehaviour
{
    public static WaitingParentUI Instance { get; private set; }

    [SerializeField]private GameObject waitingParentUI;
    [SerializeField]private CountDownUI countDownUI;

    private Coroutine _checkPlayerCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }
    public void Show()
    {
        waitingParentUI.SetActive(true);
        // 开始检测房间玩家数量变化
        if (_checkPlayerCoroutine != null) StopCoroutine(_checkPlayerCoroutine);
        _checkPlayerCoroutine = StartCoroutine(CheckPlayerCount());
    }

    public void Hide()
    {
        waitingParentUI.SetActive(false);
        if (_checkPlayerCoroutine != null)
        {
            StopCoroutine(_checkPlayerCoroutine);
            _checkPlayerCoroutine = null;
        }
    }

    /// <summary>
    /// 好友加入后，隐藏等待界面并开始倒计时
    /// </summary>
   public void StartCountDown()
   {
       Hide();
       if (PhotonNetwork.InRoom)
       {
           // Load the game scene for all players in the room
           PhotonNetwork.LoadLevel("2-GameScene");
       }
       else
       {
           if (GameManager.Instance != null)
           {
               GameManager.Instance.StartSinglePlayerMode();
           }
       }
   }

    /// <summary>
    /// 轮询检测房间玩家数量，当有其他玩家加入时开始倒计时
    /// </summary>
    private IEnumerator CheckPlayerCount()
    {
        while (waitingParentUI.activeSelf && PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                Debug.Log("检测到有玩家加入房间，开始倒计时");
                StartCountDown();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
