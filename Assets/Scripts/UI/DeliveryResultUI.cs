using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryResultUI : MonoBehaviour
{
    private const string IS_SHOW = "IsShow";

    [SerializeField]private Animator deliverySuccessUIAnimator;
    [SerializeField]private Animator deliveryFailUIAnimator;
    void Start()
    {
        OrderManager.Instance.OnRecipeSucceed += OrderManager_OnRecipeSucceed;
        OrderManager.Instance.OnRecipeFailed += OrderManager_OnRecipeFailed;
    }

    private void OrderManager_OnRecipeFailed(object sender, EventArgs e)
    {
        deliveryFailUIAnimator.gameObject.SetActive(true);
        deliveryFailUIAnimator.SetTrigger(IS_SHOW);
    }

    private void OrderManager_OnRecipeSucceed(object sender, EventArgs e)
    {
        deliverySuccessUIAnimator.gameObject.SetActive(true);
        deliverySuccessUIAnimator.SetTrigger(IS_SHOW);
    }
}
