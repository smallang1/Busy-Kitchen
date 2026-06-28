using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class ClearCounter : BaseCounter
{  
    public override void Interact(Player player)
    {
        if (player.IsHaveKitchenObject())
        {
            //手上有KitchenObject
            if(player.GetKitchenObject()
                .TryGetComponent<PlateKitchenObject>(out PlateKitchenObject plateKitchenObject))
            {//手上有盘子
                if (IsHaveKitchenObject() == false)
                {//当前柜台为空
                    string dropFN = player.GetKitchenObjectSO()?.prefab?.name ?? "";
                    TransferKitchenObject(player, this);
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (pv != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(dropFN))
                        pv.RPC("RPC_DropFood", RpcTarget.Others, transform.position, dropFN);
                }
                else
                {//当前柜台不为空
                    bool isSucess = plateKitchenObject.AddKitchenObjectSO(GetKitchenObjectSO());
                    if (isSucess)
                    {
                        DestoryKitchenOject();
                    }                   
                }
            }
            else
            {//手上是普通食材
                if (IsHaveKitchenObject() == false)
                {//当前柜台为空
                    string dropFN = player.GetKitchenObjectSO()?.prefab?.name ?? "";
                    TransferKitchenObject(player, this);
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (pv != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(dropFN))
                        pv.RPC("RPC_DropFood", RpcTarget.Others, transform.position, dropFN);
                }
                else
                {//当前柜台不为空
                    if(GetKitchenObject().TryGetComponent<PlateKitchenObject>(out plateKitchenObject))
                    {
                        string foodName = player.GetKitchenObjectSO()?.prefab?.name ?? "";
                        if (plateKitchenObject.AddKitchenObjectSO(player.GetKitchenObjectSO()))
                        {
                            player.DestoryKitchenOject();
                            PhotonView pvPlate = player.GetComponent<PhotonView>();
                            if (pvPlate != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(foodName))
                                pvPlate.RPC("RPC_AddFoodToPlate", RpcTarget.Others, transform.position, foodName);
                        }
                        
                    }
                }
            }
        }
        else
        {
            //手上没食材
            if(IsHaveKitchenObject() == false)
            {
                //当前柜台为空
            }
            else
            {
                //当前柜台不为空
                string pickupFN = GetKitchenObjectSO()?.prefab?.name ?? "";
                TransferKitchenObject(this, player);
                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(pickupFN))
                    pv.RPC("RPC_PickupFood", RpcTarget.Others, transform.position, pickupFN);
            }
        }
    }
  
}