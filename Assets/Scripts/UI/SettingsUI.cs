using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [SerializeField] private GameObject uiParent;
    [SerializeField] private Button soundButton;
    [SerializeField] private TextMeshProUGUI soundButtonText;
    [SerializeField] private Button musicButton;
    [SerializeField] private TextMeshProUGUI musicButtonText;
    [SerializeField] private Button closeButton;

    [SerializeField] private Button upKeyButton;
    [SerializeField] private Button downKeyButton;
    [SerializeField] private Button leftKeyButton;
    [SerializeField] private Button rightKeyButton;
    [SerializeField] private Button interactKeyButton;
    [SerializeField] private Button operateKeyButton;
    [SerializeField] private Button pauseKeyButton;

    [SerializeField] private TextMeshProUGUI upKeyButtonText;
    [SerializeField] private TextMeshProUGUI downKeyButtonText;
    [SerializeField] private TextMeshProUGUI leftKeyButtonText;
    [SerializeField] private TextMeshProUGUI rightKeyButtonText;
    [SerializeField] private TextMeshProUGUI interactKeyButtonText;
    [SerializeField] private TextMeshProUGUI operateKeyButtonText;
    [SerializeField] private TextMeshProUGUI pauseKeyButtonText;

    [SerializeField] private GameObject rebindingHint;

    private void Awake()
    {
        Instance = this; //单例模式
    }
    private void Start()
    {
        Hide();
        UpdateVisual(); //更新音量显示
        soundButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.ChangeVolum();
            UpdateVisual();
        });
        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        upKeyButton.onClick.AddListener(() =>
        {   ReBinding(GameInput.BindingType.Up);
        });
        downKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Down);
        });
        leftKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Left);
        });
        rightKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Right);
        });
        interactKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Interact);
        });
        operateKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Operate);
        });
        pauseKeyButton.onClick.AddListener(() =>
        {
            ReBinding(GameInput.BindingType.Pause);
        });
    }
    public void Show()
    {
        uiParent.SetActive(true);
    }
    private void Hide()
    {
        uiParent.SetActive(false);
    }
    private void UpdateVisual()
    {
        soundButtonText.text = "音效大小：" + SoundManager.Instance.GetVolume();
        musicButtonText.text = "音乐大小：" + MusicManager.Instance.GetVolume();

        upKeyButtonText.text =GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Up);
        downKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Down);
        leftKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Left);
        rightKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Right);
        interactKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Interact);
        operateKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Operate);
        pauseKeyButtonText.text = GameInput.Instance.GetBindingDisplayString(GameInput.BindingType.Pause);  //更新按键显示
    }
    private void ReBinding(GameInput.BindingType bindingType)
    {
        rebindingHint.SetActive(true);  //显示绑定提示 
        GameInput.Instance.ReBinding(bindingType, () =>
        {
            rebindingHint.SetActive(false);
            UpdateVisual();
        });
    }
}
