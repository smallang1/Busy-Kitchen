using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class PlayerNetwork : MonoBehaviour, IPunObservable
{
    private PhotonView pv;
    private Player player;
    private Animator animator;
    private Rigidbody rb;

    private Vector3 netPos;
    private Quaternion netRot;
    private bool netIsWalking;
    private string netFoodId = "";

    private GameObject currentFoodChild = null;
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    private float lerpSpeed = 15f;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        var vis = transform.Find("PlayerVisual");
        if (vis != null) animator = vis.GetComponent<Animator>();

        if (!pv.IsMine)
        {
            player.enabled = false;
            if (rb != null) rb.isKinematic = true;
        // 自我注册到 PhotonView 的 ObservedComponents
        if (pv != null)
        {
            bool found = false;
            foreach (var comp in pv.ObservedComponents)
            {
                if (comp == this) { found = true; break; }
            }
            if (!found)
                pv.ObservedComponents.Add(this);
        }
        }
    }

    void Update()
    {
        if (!pv.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, netPos, Time.deltaTime * lerpSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, Time.deltaTime * lerpSpeed);
            if (animator != null) animator.SetBool("IsWalking", netIsWalking);
            SyncRemoteHeldFood();
        }
    }

    void SyncRemoteHeldFood()
    {
        string heldId = "";
        if (player.IsHaveKitchenObject())
        {
            var so = player.GetKitchenObjectSO();
            if (so != null && !string.IsNullOrEmpty(so.prefab != null ? so.prefab.name : ""))
                heldId = so.prefab != null ? so.prefab.name : "";
        }
        if (heldId != netFoodId)
        {
            if (currentFoodChild != null) { Destroy(currentFoodChild); currentFoodChild = null; }
            player.ClearKitchenObject();
            if (!string.IsNullOrEmpty(netFoodId) && player.GetHoldPoint() != null)
            {
                if (!prefabCache.ContainsKey(netFoodId))
                    prefabCache[netFoodId] = Resources.Load<GameObject>("KitchenObjects/" + netFoodId);
                GameObject prefab = prefabCache[netFoodId];
                if (prefab != null)
                {
                    currentFoodChild = Instantiate(prefab, player.GetHoldPoint());
                    currentFoodChild.transform.localPosition = Vector3.zero;
                    var ko = currentFoodChild.GetComponent<KitchenObject>();
                    if (ko != null) player.AddKitchenObject(ko);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(player.IsWalking);
            string held = "";
            if (player.IsHaveKitchenObject() && player.GetKitchenObjectSO() != null)
                held = player.GetKitchenObjectSO()?.prefab?.name ?? "";
            stream.SendNext(held);
        }
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            netRot = (Quaternion)stream.ReceiveNext();
            netIsWalking = (bool)stream.ReceiveNext();
            netFoodId = (string)stream.ReceiveNext();
        }
    }

    Vector3 GetCounterKey(Vector3 pos)
    {
        return new Vector3(Mathf.Round(pos.x * 10), Mathf.Round(pos.y * 10), Mathf.Round(pos.z * 10));
    }

    T FindCounterAt<T>(Vector3 pos) where T : BaseCounter
    {
        Vector3 key = GetCounterKey(pos);
        foreach (var c in FindObjectsByType<T>(FindObjectsSortMode.None))
            if (GetCounterKey(c.transform.position) == key) return c;
        return null;
    }

    // 同步：通知所有客户端进行菜刀的切割
    [PunRPC]
    public void RPC_CutProgress(Vector3 counterPos, int cuttingCount, int cuttingCountMax)
    {
        var cc = FindCounterAt<CuttingCounter>(counterPos);
        if (cc != null) cc.NetworkSyncCut(cuttingCount, cuttingCountMax);
    }

    // 同步：切割完成，通知所有客户端重建食物
    [PunRPC]
    public void RPC_CompleteCut(Vector3 counterPos, string outputFoodId)
    {
        var cc = FindCounterAt<CuttingCounter>(counterPos);
        if (cc == null) return;
        if (cc.IsHaveKitchenObject()) cc.DestoryKitchenOject();
        if (!string.IsNullOrEmpty(outputFoodId))
        {
            if (!prefabCache.ContainsKey(outputFoodId))
                prefabCache[outputFoodId] = Resources.Load<GameObject>("KitchenObjects/" + outputFoodId);
            GameObject pf = prefabCache[outputFoodId];
            if (pf != null) cc.CreatKitchenObject(pf);
        }
        cc.CompleteCutVisual();
    }

    // 同步：从柜台拿起食物
    [PunRPC]
    public void RPC_PickupFood(Vector3 counterPos, string foodId)
    {
        var counter = FindCounterAt<BaseCounter>(counterPos);
        if (counter != null && counter.IsHaveKitchenObject())
            counter.DestoryKitchenOject();
    }

    // 同步：放下食物到柜台
    [PunRPC]
    public void RPC_DropFood(Vector3 counterPos, string foodId)
    {
        var counter = FindCounterAt<BaseCounter>(counterPos);
        if (counter == null) return;
        var cc = counter as CuttingCounter;
        if (cc != null) cc.HideProgress();
        if (!string.IsNullOrEmpty(foodId))
        {
            if (!prefabCache.ContainsKey(foodId))
                prefabCache[foodId] = Resources.Load<GameObject>("KitchenObjects/" + foodId);
            GameObject pf = prefabCache[foodId];
            if (pf != null) counter.CreatKitchenObject(pf);
        }
    }

    [PunRPC]
    public void RPC_AddFoodToPlate(Vector3 counterPos, string foodSOName)
    {
        var counter = FindCounterAt<BaseCounter>(counterPos);
        if (counter == null || !counter.IsHaveKitchenObject()) return;
        var plate = counter.GetKitchenObject().GetComponent<PlateKitchenObject>();
        if (plate == null) return;
        KitchenObjectSO so = KitchenObjectSO.FindByName(foodSOName);
        if (so != null) plate.AddKitchenObjectSO(so);
    }
    [PunRPC]
    public void RPC_DeliveryResult(bool success)
    {
        if (OrderManager.Instance == null) return;
        if (success)
            OrderManager.Instance.TriggerRecipeSucceed();
        else
            OrderManager.Instance.TriggerRecipeFailed();
    }
}