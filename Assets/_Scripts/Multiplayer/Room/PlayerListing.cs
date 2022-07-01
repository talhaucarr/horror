using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace _Scripts.Multiplayer.Room
{
    public class PlayerListing : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TextMeshProUGUI playerName;

        public Photon.Realtime.Player Player { get; private set; }
        public bool IsReady = false;
        
        public void SetupPlayerInfo(Photon.Realtime.Player player)
        {
            Player = player;
            SetPlayerText(player);
            
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            if (targetPlayer != null && Equals(targetPlayer, Player))
            {
                if(changedProps.ContainsKey("RandomNumber"))
                    SetPlayerText(targetPlayer);
            }
        }

        private void SetPlayerText(Photon.Realtime.Player player)
        {
            int result = -1;
            if(player.CustomProperties.ContainsKey("RandomNumber"))
                result = (int) player.CustomProperties["RandomNumber"];
            playerName.text = result.ToString() + "," +player.NickName;
        }
    }
}
