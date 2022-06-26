using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Multiplayer
{
    public class RaiseEventExample : MonoBehaviourPun
    {
        [SerializeField] private Material dummyMat;

        private const byte COLOR_CHANGE_EVENT = 0;

        private void Start()
        {
            ChangeColor();
        }

        private void OnEnable()
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClientOnEventReceived;
        }

        private void OnDisable()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClientOnEventReceived;
        }

        private void NetworkingClientOnEventReceived(EventData obj)
        {
            if (obj.Code == COLOR_CHANGE_EVENT)
            {
                object[] datas = (object[]) obj.CustomData;
                float r = (float) datas[0];
                float g = (float) datas[1];
                float b = (float) datas[2];
                dummyMat.color = new Color(r, g, b, 1f);
            }
        }

        private void ChangeColor()
        {
            var r = Random.Range(0f, 1f);
            var g = Random.Range(0f, 1f);
            var b = Random.Range(0f, 1f);
            
            dummyMat.color = new Color(r, g, b, 1f);

            object[] datas = new object[] {r, g, b,};

            PhotonNetwork.RaiseEvent(COLOR_CHANGE_EVENT, datas, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        }
    }
}
