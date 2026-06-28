using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    private List<KitchenObjectSO> kitchenObjectSOList = new List<KitchenObjectSO>();

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    [SerializeField] private PlateCompleteVisual plateCompleteVisual;

    [SerializeField] private KitchenObjectGridUI kitchenObjectGridUI;
    public bool AddKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false; //已经存在
        }
        if (validKitchenObjectSOList.Contains(kitchenObjectSO) == false)
        {
            return false; //不是有效的食材
        }
        plateCompleteVisual.ShowKitchenObject(kitchenObjectSO); //显示对应的模型
        kitchenObjectGridUI.ShowKitchenObjectUI(kitchenObjectSO); //显示对应的UI
        kitchenObjectSOList.Add(kitchenObjectSO);
        return true; //添加成功
    }
    public List<KitchenObjectSO>  GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
}
