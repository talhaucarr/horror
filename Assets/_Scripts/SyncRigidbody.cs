using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SyncRigidbody : MonoBehaviour
{
    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (pv.IsMine) return;
        if(collision.collider.gameObject.TryGetComponent<PhotonView>(out var otherPv))
        {
            if (otherPv.OwnerActorNr != pv.OwnerActorNr)
            {
                pv.TransferOwnership(otherPv.ControllerActorNr);
            }
            
        }
    }
}
