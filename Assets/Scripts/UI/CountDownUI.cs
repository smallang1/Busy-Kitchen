using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CountDownUI : MonoBehaviour
{
    private const string IS_SHAKE = "IsShake";

    [SerializeField]private TextMeshProUGUI numberText;
    private Animator anim;

    private int preNumber = -1;
    private void Start()
    {
        anim = GetComponent<Animator>();
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }
    private void Update()
    {
        if (GameManager.Instance.IsCountDownState())
        {
            int nowNumber = Mathf.CeilToInt(GameManager.Instance.GetCountDownTime());
            numberText.text = nowNumber.ToString();
            if (nowNumber != preNumber)
            {
                preNumber = nowNumber;  //¸üÐÂÊý×Ö
                anim.SetTrigger(IS_SHAKE);
                float volume = 1f;
                SoundManager.Instance.PlayCountDownSound(volume);
            }
        }
    }
    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsCountDownState())
        {
            numberText.gameObject.SetActive(true);
        }
        else
        {
            numberText.gameObject.SetActive(false);
        }
    }
}
