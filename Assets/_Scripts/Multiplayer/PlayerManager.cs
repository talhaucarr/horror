using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class PlayerManager : MonoBehaviour
{

    [SerializeField] Transform defaultSpawnPosition;
    
    PhotonView PV;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        CreatePlayerController();
    }

    void CreatePlayerController()
    {
        Debug.Log("Player Id:" + PV.Controller.ActorNumber);
        Vector3 spawnPosition = defaultSpawnPosition.position + new Vector3(PV.Controller.ActorNumber * 10, 0, 0);
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerFirstPerson"), spawnPosition, Quaternion.identity);
    }
}
