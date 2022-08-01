using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class ServerSetupManager : MonoBehaviour
{
    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SetupServer();
    }

    private void SetupServer()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Cube"), new Vector3(0,0,10), Quaternion.identity);
    }
}
