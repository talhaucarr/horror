using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Scripts.Multiplayer
{
    public class TestConnect : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.NickName = MasterManager.Instance.GameSettings.NickName;
            PhotonNetwork.GameVersion = MasterManager.Instance.GameSettings.GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log($"Connected to server");
            Debug.Log($"{PhotonNetwork.LocalPlayer.NickName}");
            if(!PhotonNetwork.InLobby)
                PhotonNetwork.JoinLobby();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            base.OnDisconnected(cause);
            Debug.Log($"Disconnected from server for: {cause.ToString()}");
        }
    }
}
