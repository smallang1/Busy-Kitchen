using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCounter : KitchenObjectHolder
{
    [SerializeField] private GameObject selectCounter;
    public virtual void Interact(Player player)
    {
        Debug.LogWarning("交互方法没有重写");
    }

    public virtual void InteractOperate(Player player)
    {

    }
    public void SelectCounter()
    {
        selectCounter.SetActive(true);
    }


    public void CancelSelect()
    {
        selectCounter.SetActive(false);
    }
    

}
