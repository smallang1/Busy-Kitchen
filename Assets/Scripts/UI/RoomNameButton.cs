using TMPro;
using UnityEngine;

public class RoomNameButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    private string roomName;

    public void SetRoomName(string name)
    {
        roomName = name;
        if (roomNameText != null)
        {
            roomNameText.text = " 房间号："+ roomName;
        }
    }

    public string GetRoomName()
    {
        return roomName;
    }
}
