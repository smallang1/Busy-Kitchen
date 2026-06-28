using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class CuttingCounter : BaseCounter
{
    public static event EventHandler OnCut;

    [SerializeField] private CuttingRecipeListSO cuttingRecipeList;

    [SerializeField] private ProgressBarUI progressBarUI;

    [SerializeField] private CuttingCounterVisual cuttingCounterVisual;

    private int cuttingCount = 0;

    private void Start()
    {
        if (cuttingRecipeList == null)
        {
            Debug.LogError($"CuttingCounter（{gameObject.name}）未在 Inspector 中分配 CuttingRecipeList。尝试从 Resources 中查找。");
            CuttingRecipeListSO[] found = Resources.LoadAll<CuttingRecipeListSO>("");
            if (found != null && found.Length > 0)
            {
                cuttingRecipeList = found[0];
                Debug.Log($"已从 Resources 恢复 CuttingRecipeListSO '{cuttingRecipeList.name}' 并分配给 CuttingCounter（{gameObject.name}）。");
            }
            else
            {
                Debug.LogWarning("在 Resources 中未找到 CuttingRecipeListSO。若已在 Inspector 中分配，请确保资产不在 Editor 目录并且 prefab/场景上有该引用。");
            }
        }
    }

    public override void Interact(Player player)
    {
        if (player.IsHaveKitchenObject())
        {
            if (IsHaveKitchenObject() == false)
            {
                string dropFN = player.GetKitchenObjectSO()?.prefab?.name ?? "";
                cuttingCount = 0;
                TransferKitchenObject(player, this);
                PhotonView pv2 = player.GetComponent<PhotonView>();
                if (pv2 != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(dropFN))
                    pv2.RPC("RPC_DropFood", RpcTarget.Others, transform.position, dropFN);
            }
        }
        else
        {
            if (IsHaveKitchenObject() == false) { }
            else
            {
                string pickupFN = GetKitchenObjectSO()?.prefab?.name ?? "";
                TransferKitchenObject(this, player);
                progressBarUI.Hide();
                PhotonView pv3 = player.GetComponent<PhotonView>();
                if (pv3 != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(pickupFN))
                    pv3.RPC("RPC_PickupFood", RpcTarget.Others, transform.position, pickupFN);
            }
        }
    }

    public override void InteractOperate(Player player)
    {
        if (IsHaveKitchenObject())
        {
            var kitchenObject = GetKitchenObject();
            if (kitchenObject == null) return;
            var kitchenObjectSO = kitchenObject.GetKitchenObjectSO();
            if (kitchenObjectSO == null) return;
            if (cuttingRecipeList == null)
            {
                Debug.LogError($"CuttingCounter（{gameObject.name}）无法执行切割：cuttingRecipeList 为 null。");
                return;
            }
            if (cuttingRecipeList.TryGetCuttingRecipe(kitchenObjectSO, out CuttingRecipe cuttingRecipe))
            {
                Cut();
                progressBarUI.UpdateProgress((float)cuttingCount / cuttingRecipe.cuttingCountMax);

                PhotonView pv = player.GetComponent<PhotonView>();
                if (pv != null && PhotonNetwork.InRoom)
                {
                    pv.RPC("RPC_CutProgress", RpcTarget.Others, transform.position, cuttingCount, cuttingRecipe.cuttingCountMax);
                }

                if (cuttingCount == cuttingRecipe.cuttingCountMax)
                {
                    string outName = cuttingRecipe.output?.prefab?.name ?? "";
                    DestoryKitchenOject();
                    CreatKitchenObject(cuttingRecipe.output.prefab);
                    if (pv != null && PhotonNetwork.InRoom && !string.IsNullOrEmpty(outName))
                        pv.RPC("RPC_CompleteCut", RpcTarget.Others, transform.position, outName);
                }
            }
        }
    }

    private void Cut()
    {
        OnCut?.Invoke(this, EventArgs.Empty);
        cuttingCount++;
        cuttingCounterVisual.PlayCut();
    }

    public void NetworkSyncCut(int newCount, int maxCount)
    {
        if (progressBarUI != null) progressBarUI.UpdateProgress((float)newCount / maxCount);
        cuttingCounterVisual.PlayCut();
    }
    public void HideProgress() { if (progressBarUI != null) progressBarUI.Hide(); }
    public void CompleteCutVisual() { cuttingCounterVisual.PlayCut(); HideProgress(); }

    public static new void ClearStaticData()
    {
        OnCut = null;
    }
}