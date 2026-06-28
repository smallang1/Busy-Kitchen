using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static NetworkManager Instance { get; private set; }
    public event Action<List<RoomInfo>> OnRoomListUpdated;

    [SerializeField] private string playerPrefabName = "Player"; // Name of player prefab in Resources

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    private bool _waitingToJoinRoom = false;
    private bool _waitingToCreateRoom = false;
    private string _pendingRoomName = null;
    private string _pendingRoomPassword = null;
    private Coroutine _waitCoroutine = null;
    private const float WAIT_TIMEOUT = 10f;
    private Coroutine _joinLobbyCoroutine = null;
    private Coroutine _rejoinLobbyCoroutine = null;

   // Event code for notifying other clients about newly created room
   private const byte EV_NEW_ROOM = 1;

   private bool _playerSpawned = false;

   private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private new void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private new void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

   public void Start()
   {
       Debug.Log("NetworkManager: ConnectUsingSettings");
       PhotonNetwork.ConnectUsingSettings();
       SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDestroy()
   {
       SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
       if (scene.buildIndex == 2) // GameScene
       {
           _playerSpawned = false;
           SpawnLocalPlayerIfNeeded();

           if (PhotonNetwork.InRoom && GameManager.Instance != null)
           {
               GameManager.Instance.StartMultiplayerMode();
           }
       }
   }

   public override void OnConnectedToMaster()
   {
       Debug.Log("OnConnectedToMaster - InLobby=" + PhotonNetwork.InLobby + " State=" + PhotonNetwork.NetworkClientState);
       if (!PhotonNetwork.InLobby && PhotonNetwork.NetworkClientState != ClientState.JoiningLobby)
       {
           PhotonNetwork.JoinLobby();
       }
   }

   public override void OnJoinedLobby()
   {
       Debug.Log("OnJoinedLobby - clearing cachedRoomList");
       cachedRoomList.Clear();
       // Do NOT invoke OnRoomListUpdated here — OnRoomListUpdate will deliver rooms shortly
   }
   public override void OnLeftLobby()
   {
       Debug.Log("OnLeftLobby");
   }

   public override void OnRoomListUpdate(List<RoomInfo> roomList)
   {
       Debug.Log("OnRoomListUpdate received: " + (roomList == null ? "null" : roomList.Count.ToString()));
       if (roomList == null || roomList.Count == 0)
       {
           // Photon may send null/empty batches — notify UI with current cache
           OnRoomListUpdated?.Invoke(new List<RoomInfo>(cachedRoomList));
           return;
       }

       foreach (var room in roomList)
       {
           try
           {
               Debug.Log($"OnRoomListUpdate - item: {room.Name} | players={room.PlayerCount} | max={room.MaxPlayers} | visible={room.IsVisible} | open={room.IsOpen} | removed={room.RemovedFromList}");
           }
           catch (Exception ex)
           {
               Debug.LogWarning("Failed to log RoomInfo: " + ex);
           }

           if (room.RemovedFromList)
           {
               int ri = cachedRoomList.FindIndex(r => r.Name == room.Name);
               if (ri >= 0) { cachedRoomList.RemoveAt(ri); Debug.Log("Removed room from cache: " + room.Name); }
               continue;
           }
           int idx = cachedRoomList.FindIndex(r => r.Name == room.Name);
           if (idx >= 0)
           {
               cachedRoomList[idx] = room;
               Debug.Log("Updated cached room: " + room.Name + FormatRoomPwd(room));
           }
           else
           {
               cachedRoomList.Add(room);
               Debug.Log("Added cached room: " + room.Name + FormatRoomPwd(room));
           }
       }
       Debug.Log("OnRoomListUpdate - cached count=" + cachedRoomList.Count);
       OnRoomListUpdated?.Invoke(new List<RoomInfo>(cachedRoomList));
   }

   private string FormatRoomPwd(RoomInfo room)
   {
       try
       {
           if (room.CustomProperties != null && room.CustomProperties.ContainsKey("pwd"))
           {
               object pwdObj = room.CustomProperties["pwd"];
               string p = pwdObj != null ? pwdObj.ToString() : "(null)";
               return " [pwd=" + p + "]";
           }
       }
       catch { }
       return "";
   }

   public List<RoomInfo> GetCachedRoomList() { return new List<RoomInfo>(cachedRoomList); }


   public bool ValidateRoomPassword(string roomName, string roomPassword)
    {
        if (string.IsNullOrEmpty(roomName)) return false;
        string provided = roomPassword != null ? roomPassword.Trim() : "";
        var roomInfo = cachedRoomList.Find(r => r.Name == roomName);
        if (roomInfo == null)
        {
            Debug.LogWarning($"ValidateRoomPassword: room not found in cache: {roomName}. Allowing join (no local validation). Provided='{provided}'");
            // If we don't have the room in cache, allow the join attempt — avoid false negatives.
            return true;
        }
        if (roomInfo.CustomProperties != null && roomInfo.CustomProperties.ContainsKey("pwd"))
        {
            object pwdObj = roomInfo.CustomProperties["pwd"];
            string expectedPwd = pwdObj != null ? pwdObj.ToString().Trim() : "";
            Debug.Log($"ValidateRoomPassword: provided='{provided}' expected='{expectedPwd}'");
            return string.Equals(provided, expectedPwd, StringComparison.Ordinal);
        }
        // no password set on room
        Debug.Log("ValidateRoomPassword: no pwd on room");
        return true;
    }

   public override void OnDisconnected(DisconnectCause cause)
   {
       Debug.Log("OnDisconnected: " + cause);
       cachedRoomList.Clear();
       ClearPending();
   }


   public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed: " + returnCode + " msg=" + message);
        const short GameDoesNotExist = 32758;
        if (returnCode == GameDoesNotExist && _waitingToJoinRoom && !string.IsNullOrEmpty(_pendingRoomName))
        {
            _waitingToCreateRoom = true; _waitingToJoinRoom = false;
            if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);
            _waitCoroutine = StartCoroutine(WaitUntilReadyThenCreate(_pendingRoomName, _pendingRoomPassword));
            return;
        }
        _waitingToJoinRoom = false; _pendingRoomName = null; _pendingRoomPassword = null;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("创建房间失败! returnCode=" + returnCode + " message=" + message);
        _waitingToCreateRoom = false; _pendingRoomName = null; _pendingRoomPassword = null;
    }

    public void JoinLobbyWhenReady()
    {
        Debug.Log("JoinLobbyWhenReady called. InLobby=" + PhotonNetwork.InLobby + " State=" + PhotonNetwork.NetworkClientState);
        if (PhotonNetwork.InLobby) return;
        if (PhotonNetwork.NetworkClientState == ClientState.JoiningLobby) return;
        if (PhotonNetwork.InRoom) { PhotonNetwork.LeaveRoom(); return; }
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        { PhotonNetwork.JoinLobby(); return; }
        if (_joinLobbyCoroutine != null) StopCoroutine(_joinLobbyCoroutine);
        _joinLobbyCoroutine = StartCoroutine(WaitUntilConnectedThenJoinLobby());
    }

    private IEnumerator WaitUntilConnectedThenJoinLobby()
    {
        float timer = 0f;
        while (timer < WAIT_TIMEOUT)
        {
            if (PhotonNetwork.InLobby || PhotonNetwork.NetworkClientState == ClientState.JoiningLobby) { _joinLobbyCoroutine = null; yield break; }
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
            { PhotonNetwork.JoinLobby(); _joinLobbyCoroutine = null; yield break; }
            timer += Time.deltaTime; yield return null;
        }
        if (PhotonNetwork.InLobby || PhotonNetwork.NetworkClientState == ClientState.JoiningLobby) { _joinLobbyCoroutine = null; yield break; }
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        { PhotonNetwork.JoinLobby(); _joinLobbyCoroutine = null; yield break; }
        _joinLobbyCoroutine = null;
    }

   public void JoinRoom(string roomName, string roomPassword = "")
   {
       if (string.IsNullOrEmpty(roomName)) return;
       // use centralized validation
       if (!ValidateRoomPassword(roomName, roomPassword))
       {
           Debug.LogWarning("密码错误！无法加入房间: " + roomName);
           return;
       }

       if (PhotonNetwork.InRoom)
       {
           if (PhotonNetwork.CurrentRoom.Name == roomName) { ClearPending(); return; }
           _waitingToJoinRoom = true; _waitingToCreateRoom = false;
           _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
           PhotonNetwork.LeaveRoom(); return;
       }
       if (PhotonNetwork.IsConnected && (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer || PhotonNetwork.NetworkClientState == ClientState.JoinedLobby))
       {
           _waitingToJoinRoom = true; _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
           PhotonNetwork.JoinRoom(roomName); return;
       }
       _waitingToJoinRoom = true; _waitingToCreateRoom = false;
       _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
       if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);
       _waitCoroutine = StartCoroutine(WaitUntilReadyThenJoin(roomName, roomPassword));
   }

   public void CreateRoom(string roomName, string roomPassword = "")
   {
       if (string.IsNullOrEmpty(roomName)) return;
       if (PhotonNetwork.InRoom)
       {
           if (PhotonNetwork.CurrentRoom.Name == roomName) { ClearPending(); return; }
           _waitingToCreateRoom = true; _waitingToJoinRoom = false;
           _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
           PhotonNetwork.LeaveRoom(); return;
       }
       if (PhotonNetwork.IsConnected && (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer || PhotonNetwork.NetworkClientState == ClientState.JoinedLobby))
       {
            RoomOptions options = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true, EmptyRoomTtl = 60000 };
            if (!string.IsNullOrEmpty(roomPassword))
            {
                options.CustomRoomProperties = new Hashtable { { "pwd", roomPassword.Trim() } };
                options.CustomRoomPropertiesForLobby = new string[] { "pwd" };
            }
           Debug.Log("CreateRoom: " + roomName + " (InLobby=" + PhotonNetwork.InLobby + ")");
           PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
           return;
       }
       _waitingToCreateRoom = true; _waitingToJoinRoom = false;
       _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
       if (_waitCoroutine != null) StopCoroutine(_waitCoroutine);
       _waitCoroutine = StartCoroutine(WaitUntilReadyThenCreate(roomName, roomPassword));
   }

   private IEnumerator WaitUntilReadyThenJoin(string roomName, string roomPassword)
   {
       while (_waitingToJoinRoom)
       {
           if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
           {
               _waitingToJoinRoom = true; _pendingRoomName = roomName; _pendingRoomPassword = roomPassword;
               yield return new WaitForSeconds(0.05f);
               PhotonNetwork.JoinRoom(roomName); yield break;
           }
           yield return new WaitForSeconds(0.25f);
       }
   }

   private IEnumerator WaitUntilReadyThenCreate(string roomName, string roomPassword)
   {
       while (_waitingToCreateRoom)
       {
           if (PhotonNetwork.IsConnected && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
           {
               RoomOptions options = new RoomOptions { MaxPlayers = 4, IsVisible = true, IsOpen = true, EmptyRoomTtl = 60000 };
               if (!string.IsNullOrEmpty(roomPassword))
               {
                   options.CustomRoomProperties = new Hashtable { { "pwd", roomPassword.Trim() } };
                   options.CustomRoomPropertiesForLobby = new string[] { "pwd" };
               }
               yield return new WaitForSeconds(0.02f);
               Debug.Log("WaitUntilReadyThenCreate: creating room " + roomName + " (InLobby=" + PhotonNetwork.InLobby + ")");
               PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);

               yield break;
           }
           yield return new WaitForSeconds(0.25f);
       }
   }

    private IEnumerator RejoinLobbyRoutine()
    {
        if (!PhotonNetwork.IsConnected) yield break;
        Debug.Log("RejoinLobbyRoutine: leaving lobby to force refresh");
        PhotonNetwork.LeaveLobby();
        // wait until left or short timeout
        float timer = 0f; float timeout = 0.6f;
        while (timer < timeout && PhotonNetwork.InLobby)
        {
            timer += Time.deltaTime; yield return null;
        }
        // increased delay before rejoin to allow master server to surface new room to lobby (set to 1.5s)
        yield return new WaitForSeconds(1.5f);
        if (PhotonNetwork.IsConnected && PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            Debug.Log("RejoinLobbyRoutine: rejoining lobby after 1.5s delay");
            PhotonNetwork.JoinLobby();
        }
        _rejoinLobbyCoroutine = null;
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom: " + PhotonNetwork.CurrentRoom.Name);
        ClearPending();
        if (WaitingParentUI.Instance != null) WaitingParentUI.Instance.Show();
        // Room is now visible in lobby - other players will see it via OnRoomListUpdate
        // No need to broadcast - Photon master server handles room list updates automatically
    }
   public override void OnJoinedRoom()
   {
       Debug.Log("OnJoinedRoom: " + PhotonNetwork.CurrentRoom.Name);
       ClearPending();
       if (WaitingParentUI.Instance != null) WaitingParentUI.Instance.Show();
   }

    public override void OnLeftRoom()
    {
        _playerSpawned = false;
    }

    private void SpawnLocalPlayerIfNeeded()
    {
        if (!PhotonNetwork.InRoom) return;
        if (_playerSpawned) return;
        if (Player.Instance != null)
        {
            _playerSpawned = true; // already exists and likely local
            return;
        }

        // Choose spawn position based on ActorNumber to avoid overlapping
        Vector3 spawnPos = Vector3.zero;
        try
        {
            int idx = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            spawnPos = new Vector3(idx * 1.5f, 0f, 0f);
        }
        catch { }

        Debug.Log("Spawning local player prefab via PhotonNetwork.Instantiate: " + playerPrefabName + " at " + spawnPos);
        try
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, Quaternion.identity);
            if (go != null) _playerSpawned = true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to PhotonNetwork.Instantiate player prefab. Ensure prefab named '" + playerPrefabName + "' exists in Resources folder and has a PhotonView. Exception: " + ex);
        }
    }

    private void ClearPending()
    {
        _waitingToJoinRoom = false; _waitingToCreateRoom = false;
        _pendingRoomName = null; _pendingRoomPassword = null;
        if (_waitCoroutine != null) { StopCoroutine(_waitCoroutine); _waitCoroutine = null; }
        if (_joinLobbyCoroutine != null) { StopCoroutine(_joinLobbyCoroutine); _joinLobbyCoroutine = null; }
        if (_rejoinLobbyCoroutine != null) { StopCoroutine(_rejoinLobbyCoroutine); _rejoinLobbyCoroutine = null; }
    }

   public void RefreshRoomList()
   {
       Debug.Log("RefreshRoomList called. InLobby=" + PhotonNetwork.InLobby + " State=" + PhotonNetwork.NetworkClientState);
       // If already in lobby, force a leave+join to trigger OnRoomListUpdate faster
       if (PhotonNetwork.InLobby)
       {
           if (_rejoinLobbyCoroutine != null) StopCoroutine(_rejoinLobbyCoroutine);
           _rejoinLobbyCoroutine = StartCoroutine(RejoinLobbyRoutine());
           return;
       }
       // Otherwise ensure we join lobby
       JoinLobbyWhenReady();
   }

    // IOnEventCallback implementation - handle EV_NEW_ROOM
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent == null) return;
        if (photonEvent.Code == EV_NEW_ROOM)
        {
            try
            {
                object[] payload = photonEvent.CustomData as object[];
                if (payload != null && payload.Length >= 1)
                {
                    string roomName = payload[0] as string;
                    string pwd = "";
                    if (payload.Length >= 2 && payload[1] != null) pwd = payload[1].ToString();
                    Debug.Log("EV_NEW_ROOM received for: " + roomName + " pwd='" + pwd + "'");
                    // Trigger a refresh to get the new room into local lobby cache
                    RefreshRoomList();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("OnEvent processing failed: " + ex);
            }
        }
    }
}
