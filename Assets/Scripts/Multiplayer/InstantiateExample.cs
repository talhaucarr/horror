using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace _Scripts.Multiplayer
{
    public class InstantiateExample : MonoBehaviourPun
    {
        [SerializeField] private GameObject prefab;
        
       private void Awake()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                MasterManager.NetworkInstantiate(prefab, transform.position, Quaternion.identity);
            }
            else
            {
                photonView.RPC(nameof(SpwanPlayer), RpcTarget.MasterClient);
            }
            
        }
        

        [PunRPC] 
        private void SpwanPlayer()
        {
            MasterManager.NetworkInstantiate(prefab, transform.position, Quaternion.identity);
        }
    }
}
