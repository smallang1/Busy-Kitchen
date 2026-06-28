using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    //����һ���ַ�������
    private const string IS_WALKING = "IsWalking";
    private Animator anim;
    [SerializeField] private Player player;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // 远程玩家由 PlayerNetwork 控制动画
        if (player != null && !player.enabled) return;
        anim.SetBool(IS_WALKING ,player.IsWalking);
    }
}
