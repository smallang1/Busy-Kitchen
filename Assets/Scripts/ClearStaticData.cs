using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearStaticData : MonoBehaviour
{
    private void Start()
    {
        TrashCounter.ClearStaticData();
        KitchenObjectHolder.ClearStaticData();
        CuttingCounter.ClearStaticData();
    }
}
