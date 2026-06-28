using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    [SerializeField]private KitchenObjectSO plateSO;
    [SerializeField]private float spawnRate = 3;
    [SerializeField]private int plateCountMax = 5;

    private List<KitchenObject> platesList = new List<KitchenObject>(); //存储所有 plates

    private float timer = 0;
    private void Update()
    {
        if(platesList.Count < plateCountMax)
        {
            timer += Time.deltaTime;
        }
        if(timer >= spawnRate)
        {
            timer = 0; //重置计时器
            SpawnPlate();
        }

    }
    public override void Interact(Player player)
    {
        if(player.IsHaveKitchenObject() == false)
        {
            if(platesList.Count > 0)
            {
                //当前柜台上有食材
                player.AddKitchenObject(platesList[platesList.Count - 1]);
                //将食材移除
                platesList.RemoveAt(platesList.Count - 1);
            }
        }
    }
    public void SpawnPlate()
    {
        if(platesList.Count >= plateCountMax)
        {
            timer = 0;
            return; 
        }
        KitchenObject kitchenObject = GameObject.Instantiate(plateSO.prefab, GetHoldPoint()).GetComponent<KitchenObject>();

        kitchenObject.transform.localPosition = Vector3.zero + Vector3.up * 0.1f * platesList.Count;

        platesList.Add(kitchenObject);
    }
}
