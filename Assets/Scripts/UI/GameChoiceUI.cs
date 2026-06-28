using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameChoiceUI : MonoBehaviour
{
    [SerializeField]private GameObject uiParent;
    [SerializeField]private Button singlePlayerButton;
    [SerializeField]private Button multiplayerButton;
    [SerializeField]private TutorialUI tutorialUI;
    [SerializeField]private MultiGameUI multiGameUI;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            gameObject.SetActive(true);
            uiParent.SetActive(GameManager.Instance.IsWaitingToStartState());
            GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        }
        else
        {
            gameObject.SetActive(true);
            uiParent.SetActive(true);
        }

        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.AddListener(OnSinglePlayerButton);
        }
        else
        {
            Debug.LogWarning("GameChoiceUI 未在 Inspector 上分配 singlePlayerButton。");
        }

        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.AddListener(OnMultiplayerButton);
        }
        else
        {
            Debug.LogWarning("GameChoiceUI 未在 Inspector 上分配 multiplayerButton。");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= GameManager_OnStateChanged;
        }
        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.RemoveListener(OnSinglePlayerButton);
        }
        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.RemoveListener(OnMultiplayerButton);
        }
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance == null) return;
        bool shouldShow = GameManager.Instance.IsWaitingToStartState();
        uiParent.SetActive(shouldShow);
    }

    public void Show()
    {
        uiParent.SetActive(true);
    }
    public void Hide()
    {
        uiParent.SetActive(false);
    }

    public void OnSinglePlayerButton()
    {
        Hide();

        if (tutorialUI != null)
        {
            tutorialUI.ShowAndWaitForConfirm();
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartSinglePlayerMode();
            }
            else
            {
                Debug.LogError("尝试开始单人模式时 GameManager.Instance 为 null。");
            }
        }
    }

    public void OnMultiplayerButton()
    {
        Hide();
        if (multiGameUI != null)
        {
            multiGameUI.Show();
        }
    }
}
