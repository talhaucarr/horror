using System.Collections.Generic;
using _Scripts.UI.UIPanel;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utilities;

namespace _Scripts.Multiplayer.Room
{
    public class RoomListingMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform content;
        [SerializeField] private RoomListing roomListing;

        private List<RoomListing> _roomListings = new List<RoomListing>();

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            //_mainPanel.CurrentRoomCanvas.Activate();
            UIPanelManager.Instance.OpenPanel("CurrentRoomPanel");
            content.DestroyChildren();
            _roomListings.Clear();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            foreach (RoomInfo roomInfo in roomList)
            {
                if (roomInfo.RemovedFromList)
                {
                    int index = _roomListings.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
                    if (index != -1)
                    {
                        Destroy(_roomListings[index].gameObject);
                        _roomListings.RemoveAt(index);
                    }
                }
                else
                {
                    int index = _roomListings.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
                    if (index == -1)
                    {
                        RoomListing listing = Instantiate(roomListing, content);
                        if (listing != null)
                        {
                            listing.SetupRoomInfo(roomInfo);
                            _roomListings.Add(listing);
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
        }
    }
}
