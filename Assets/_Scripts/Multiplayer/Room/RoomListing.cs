using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace _Scripts.Multiplayer.Room
{
    public class RoomListing : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roomName;
        
        public RoomInfo RoomInfo { get; private set; }
        
        public void SetupRoomInfo(RoomInfo roomInfo)
        {
            RoomInfo = roomInfo;
            roomName.text = $"Room Name: {roomInfo.Name} - Max Players: {roomInfo.MaxPlayers}";
        }

        public void OnClick_Button()
        {
            PhotonNetwork.JoinRoom(RoomInfo.Name);
        }
    }
}
