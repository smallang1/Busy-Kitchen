using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DeliveryCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if(player.IsHaveKitchenObject() 
            && player.GetKitchenObject()
            .TryGetComponent<PlateKitchenObject>(out PlateKitchenObject plateKitchenObject))
        {
            bool result = OrderManager.Instance.DelivertRecipe(plateKitchenObject);
            player.DestoryKitchenOject();
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && PhotonNetwork.InRoom)
                pv.RPC("RPC_DeliveryResult", RpcTarget.Others, result);
        }
    }
}