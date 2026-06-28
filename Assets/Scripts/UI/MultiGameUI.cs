using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using Photon.Realtime;

public class MultiGameUI : MonoBehaviour
{
    public static MultiGameUI Instance { get; private set; }

    [SerializeField] private GameChoiceUI gameChoiceUI;
    [SerializeField] private GameObject uiParent;
    [SerializeField] private GameObject psdUiParent;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField roomPasswordInput;
    //验证密码输入
    [SerializeField] private TMP_InputField PasswordInput;

    [SerializeField] private Button createButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform roomListContent;
    [SerializeField] private GameObject roomNameButtonPrefab;
    [SerializeField] private Button closePsdUIParentButton;
    [SerializeField] private Button LoginButton;

    private string selectedRoomName;
    private string selectedRoomPassword;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        Hide();
        psdUiParent.SetActive(false);
        createButton.onClick.AddListener(OnCreateRoomButtonDown);
        closeButton.onClick.AddListener(OnCloseButtonDown);
        closePsdUIParentButton.onClick.AddListener(OnClosePsdUIParentButton);
        LoginButton.onClick.AddListener(OnLoginButtonDown);
    }

    private void OnCreateRoomButtonDown()
    {
        string roomName = roomNameInput.text;
        string roomPassword = roomPasswordInput.text;

        if (string.IsNullOrWhiteSpace(roomName)) { Debug.LogWarning("房间名不能为空！"); return; }
        if (string.IsNullOrWhiteSpace(roomPassword)) { Debug.LogWarning("房间密码不能为空！"); return; }

        // 密码只能包含数字
        bool allDigits = true;
        foreach (char c in roomPassword) { if (!char.IsDigit(c)) { allDigits = false; break; } }
       if (!allDigits) { Debug.LogWarning("密码只能包含数字！"); return; }

        Debug.Log("创建房间时，当前大厅房间数: " + (NetworkManager.Instance != null ? NetworkManager.Instance.GetCachedRoomList().Count.ToString() : "null"));

       NetworkManager.Instance.CreateRoom(roomName, roomPassword);
        Hide(); psdUiParent.SetActive(false);
        WaitingParentUI.Instance?.Show();
        roomNameInput.text = ""; roomPasswordInput.text = "";
    }

    private void OnCloseButtonDown() { Hide(); if (gameChoiceUI != null) gameChoiceUI.Show(); }
    private void OnClosePsdUIParentButton() { Show(); psdUiParent.SetActive(false); }

    public void Show()
    {
        uiParent.SetActive(true);
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnRoomListUpdated -= OnPhotonRoomListUpdated;
            NetworkManager.Instance.OnRoomListUpdated += OnPhotonRoomListUpdated;
            
            // 第一步: 确保在大厅中（触发 OnRoomListUpdate）
            NetworkManager.Instance.JoinLobbyWhenReady();
            
            // 显示当前缓存的房间列表
            var cachedList = NetworkManager.Instance.GetCachedRoomList();
            Debug.Log("MultiGameUI.Show() - 缓存房间列表数量: " + (cachedList != null ? cachedList.Count.ToString() : "null"));
            OnPhotonRoomListUpdated(cachedList != null ? cachedList : new List<RoomInfo>());

            // 立即尝试刷新一次以减少延迟
            NetworkManager.Instance.RefreshRoomList();
            // 第二步: 延迟 0.5 秒后再次强制刷新房间列表（防止初始更新为空）
            StartCoroutine(QuickDelayedRefresh());
            // 保持原来的 1.5s 备用刷新
            StartCoroutine(DelayedRefresh());
        }
    }

    private IEnumerator QuickDelayedRefresh()
    {
        yield return new WaitForSeconds(0.5f);
        if (NetworkManager.Instance != null && uiParent.activeSelf)
        {
            Debug.Log("MultiGameUI: 执行快速延迟刷新房间列表");
            NetworkManager.Instance.RefreshRoomList();
        }
    }

    private IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(1.5f);
        if (NetworkManager.Instance != null && uiParent.activeSelf)
        {
            Debug.Log("MultiGameUI: 执行延迟刷新房间列表");
            NetworkManager.Instance.RefreshRoomList();
        }
    }

    private void Hide()
    {
        uiParent.SetActive(false);
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnRoomListUpdated -= OnPhotonRoomListUpdated;
    }

    private void OnLoginButtonDown()
    {
        if (string.IsNullOrEmpty(selectedRoomName)) { Debug.Log("房间名无效"); return; }

        // Read from PasswordInput first (the psd UI input), fall back to roomPasswordInput
        selectedRoomPassword = "";
        if (PasswordInput != null && !string.IsNullOrEmpty(PasswordInput.text))
            selectedRoomPassword = PasswordInput.text;
        else if (roomPasswordInput != null && !string.IsNullOrEmpty(roomPasswordInput.text))
            selectedRoomPassword = roomPasswordInput.text;

        Debug.Log("加入房间: " + selectedRoomName + " 密码: " + selectedRoomPassword);

        // If password is empty, prompt user to enter it and focus the input field
        if (string.IsNullOrEmpty(selectedRoomPassword))
        {
            Debug.LogWarning("请输入房间密码");
            // keep psd UI open and focus input
            if (psdUiParent != null) psdUiParent.SetActive(true);
            if (roomPasswordInput != null) StartCoroutine(FocusPasswordInput());
            return;
        }

        // 验证密码 - 错误则停留在密码输入界面
        if (NetworkManager.Instance != null && !NetworkManager.Instance.ValidateRoomPassword(selectedRoomName, selectedRoomPassword))
        {
            Debug.LogWarning("密码错误！请重新输入");
            roomPasswordInput.text = "";
            // keep psd UI open
            if (psdUiParent != null) psdUiParent.SetActive(true);
            if (roomPasswordInput != null) StartCoroutine(FocusPasswordInput());
            return;
        }

        // 密码正确，跳转到等待大厅
        Hide(); psdUiParent.SetActive(false);
        WaitingParentUI.Instance?.Show();
        NetworkManager.Instance.JoinRoom(selectedRoomName, selectedRoomPassword);
        roomPasswordInput.text = "";
    }

    void OnPhotonRoomListUpdated(List<RoomInfo> roomList)
    {
        ClearRoomList();
        Debug.Log("OnPhotonRoomListUpdated - 收到的房间数量: " + roomList.Count);
        foreach (var room in roomList)
        {
            if (room.RemovedFromList) continue;
            GameObject newButton = Instantiate(roomNameButtonPrefab, roomListContent);
            RoomNameButton bc = newButton.GetComponent<RoomNameButton>();
            if (bc != null) bc.SetRoomName(room.Name);
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                string roomName = room.Name;
                btn.onClick.AddListener(() =>
                {
                    uiParent.SetActive(false); psdUiParent.SetActive(true);
                    selectedRoomName = roomName; selectedRoomPassword = "";
                    // ensure password input is cleared and focused
                    if (roomPasswordInput != null)
                    {
                        roomPasswordInput.text = "";
                        StartCoroutine(FocusPasswordInput());
                    }
                    Debug.Log("选中房间: " + roomName);
                });
            }
        }
    }

    private IEnumerator FocusPasswordInput()
    {
        // wait a frame to allow UI to activate
        yield return null;
        if (roomPasswordInput != null)
        {
            roomPasswordInput.ActivateInputField();
            roomPasswordInput.Select();
        }
    }

    void ClearRoomList() { if (roomListContent == null) return; foreach (Transform c in roomListContent) Destroy(c.gameObject); }
    private void OnDestroy() { if (NetworkManager.Instance != null) NetworkManager.Instance.OnRoomListUpdated -= OnPhotonRoomListUpdated; }
}
