using System;
using System.Collections.Generic;
using _Scripts.UI.UIPanel;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Utilities;

namespace _Scripts.Multiplayer.Room
{
    public class PlayerListingsMenu : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Transform content;
        [SerializeField] private PlayerListing playerListing;
        [SerializeField] private TextMeshProUGUI readyText;

        private List<PlayerListing> _playerListings = new List<PlayerListing>();

        private bool _isReady;
        
        public override void OnEnable()
        {
            base.OnEnable();
            GetCurrentRoomPlayers();
            SetReadyUp(false);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            for (int i = 0; i < _playerListings.Count; i++)
            {
                Destroy(_playerListings[i].gameObject);
            }
            _playerListings.Clear();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            content.DestroyChildren();
        }

        public void StartGame()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            /*foreach (var t in _playerListings)
            {
                if (t.Player != PhotonNetwork.LocalPlayer)
                {
                    if(!t.IsReady ) return;
                }
            }*/

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(1);
        }

        public void OnClickReadyUp()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                SetReadyUp(!_isReady);
                photonView.RPC(nameof(RPCChangeReadyState), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer,_isReady);
            }
        }
        
        [PunRPC]
        private void RPCChangeReadyState(Photon.Realtime.Player player, bool ready)
        {
            int index = _playerListings.FindIndex(x => Equals(x.Player, player));
            if (index != -1)
            {
                _playerListings[index].IsReady = ready;
            }
        }

        private void SetReadyUp(bool state)
        {
            _isReady = state;
            readyText.text = _isReady ? $"R" : $"N";
        }

        private void GetCurrentRoomPlayers()
        {
            if(!PhotonNetwork.IsConnected) return;
            if(PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null) return;
            foreach (KeyValuePair<int, Photon.Realtime.Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
            {
                AddPlayerListing(playerInfo.Value);
            }
        }

        private void AddPlayerListing(Photon.Realtime.Player player)
        {
            int index = _playerListings.FindIndex(x => x.Player == player);
            if (index != -1)
            {
                _playerListings[index].SetupPlayerInfo(player);
            }
            else
            {
                PlayerListing listing = Instantiate(playerListing, content);
                if (listing != null)
                {
                    listing.SetupPlayerInfo(player);
                    _playerListings.Add(listing);
                }
            }
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            UIPanelManager.Instance.OpenPanel("CreateOrJoinRoomPanel");
            PhotonNetwork.LeaveRoom(true);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            AddPlayerListing(newPlayer);
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            
            int index = _playerListings.FindIndex(x => Equals(x.Player, otherPlayer));
            if (index != -1)
            {
                Destroy(_playerListings[index].gameObject);
                _playerListings.RemoveAt(index);
            }
        }
    }
}
