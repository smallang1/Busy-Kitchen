using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField]private GameObject stoveOnVisual;
    [SerializeField]private GameObject sizzingpParticles;

    public void ShowStoveEffect()
    {
        stoveOnVisual.SetActive(true);
        sizzingpParticles.SetActive(true);
    }
    public void HideStoveEffect()
    {
        stoveOnVisual.SetActive(false);
        sizzingpParticles.SetActive(false);
    }
}
