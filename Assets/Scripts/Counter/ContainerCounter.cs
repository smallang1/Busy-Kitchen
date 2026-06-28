using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//≤÷ø‚¿‡πÒÃ®
public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    [SerializeField] private ContainerCounterVisual containerCounterVisual;

    public override void Interact(Player player)
    {
        if(player.IsHaveKitchenObject()) return;

        CreatKitchenObject(kitchenObjectSO.prefab);

        TransferKitchenObject(this, player);

        containerCounterVisual.PlayOpen();
    }
}
