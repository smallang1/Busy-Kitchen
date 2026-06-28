using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT = "Cut";

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayCut()
    {
        anim.SetTrigger(CUT);
    }
}
