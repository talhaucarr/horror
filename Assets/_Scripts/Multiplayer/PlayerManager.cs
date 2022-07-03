using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            CreatePlayerController();
        }
    }

    void CreatePlayerController()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerFirstPerson"), Vector3.zero, Quaternion.identity);
    }
}
