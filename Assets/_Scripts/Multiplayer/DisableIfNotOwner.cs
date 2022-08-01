using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotOwner : MonoBehaviour
{
    public bool IsMine = false;

    [SerializeField] private Component[] componentsToDisable;
    private void Awake()
    {
        IsMine = GetComponent<Photon.Pun.PhotonView>().ControllerActorNr == Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
    }

    private void DisableComponents() 
    {
        if (IsMine) return;
        foreach (Component component in componentsToDisable)
        {
            Destroy(component);
        }
    }
}