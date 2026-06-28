using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCounter : BaseCounter
{
    public static event EventHandler OnObjectTrashed;
    public override void Interact(Player player)
    {
        if (player.IsHaveKitchenObject())
        {
            player.DestoryKitchenOject();
            OnObjectTrashed?.Invoke(this, EventArgs.Empty);
        }
    }

    public static new void ClearStaticData()
    {
        OnObjectTrashed = null; //Çå¿Õ¾²Ì¬ÊÂ¼þ
    }
}
