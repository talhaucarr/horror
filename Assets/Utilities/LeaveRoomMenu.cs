using System.Collections;
using System.Collections.Generic;
using _Scripts.UI.UIPanel;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.Multiplayer.Room
{
    public class LeaveRoomMenu : MonoBehaviour
    {
        public void OnClick_LeaveRoom()
        {
            UIPanelManager.Instance.OpenPanel("CreateOrJoinRoomPanel");
            PhotonNetwork.LeaveRoom(true);
        }
    }
}

