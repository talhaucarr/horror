using _Scripts.UI.UIPanel;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Multiplayer.Room
{
    public class CreateRoomMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Text roomName;
        
        public void OnClickCreateRoom()
        {
            if(!PhotonNetwork.IsConnected) return;
            RoomOptions roomOptions = new RoomOptions
            {
                BroadcastPropsChangeToAll = true,
                PublishUserId = true,
                MaxPlayers = 4
            };
            PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            Debug.Log($"Created room Successfully");
           // _mainPanel.CurrentRoomCanvas.Activate();
           UIPanelManager.Instance.OpenPanel("CurrentRoomPanel");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log($"Room creation failed: {message}");
        }
    }
}
