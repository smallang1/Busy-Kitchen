using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField]private Image progressImage; 

    public void show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void UpdateProgress(float progress)
    {
        show();
        progressImage.fillAmount = progress;

        if(progress == 1)
        {
            Invoke("Hide", 0.5f);
        }
    }
}
