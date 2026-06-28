using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField]private GameObject uiParent;
    [SerializeField]private TextMeshProUGUI upkeyText;
    [SerializeField]private TextMeshProUGUI downkeyText;
    [SerializeField]private TextMeshProUGUI leftkeyText;
    [SerializeField]private TextMeshProUGUI rightkeyText;
    [SerializeField]private TextMeshProUGUI interactkeyText;
    [SerializeField]private TextMeshProUGUI operatekeyText;
    [SerializeField]private TextMeshProUGUI pausekeyText;

    private bool waitingForConfirm = false;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        // don't auto-show here; only show when player requests tutorial
        // Show();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        // hide tutorial if game is no longer in waiting state
        if (!GameManager.Instance.IsWaitingToStartState())
        {
            Hide();
        }
    }


    void Update()
    {
        if (waitingForConfirm)
        {
            if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
            {
                waitingForConfirm = false;
                Hide();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartSinglePlayerMode();
                }
                else
                {
                    Debug.LogError("尝试从教程开始单人模式时 GameManager.Instance 为 null。");
                }
            }
        }
    }
    public void ShowAndWaitForConfirm()
    {
        Show();
        waitingForConfirm = true;
    }
    private void Show()
    {
        uiParent.SetActive(true);
        UpdateVisual();
    }
    private void Hide()
    {
        uiParent.SetActive(false);
        waitingForConfirm = false;
    }
    private void UpdateVisual()
    {
        upkeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Up);
        downkeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Down);
        leftkeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Left);
        rightkeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Right);
        interactkeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Interact);
        operatekeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Operate);
        pausekeyText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Pause);  //更新按键显示
    }
}
