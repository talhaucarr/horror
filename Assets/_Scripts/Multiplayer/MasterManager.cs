using System.Collections.Generic;
using Photon.Pun;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace _Scripts.Multiplayer
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Singletons/MasterManager")]
    public class MasterManager : Utilities.ScriptableSingleton<MasterManager>
    {
        [SerializeField] private GameSettings gameSettings;

        public GameSettings GameSettings => Instance.gameSettings;

        private List<NetworkPrefab> _networkPrefabs = new List<NetworkPrefab>();

        public static GameObject NetworkInstantiate(GameObject obj, Vector3 pos, Quaternion rotation)
        {
            foreach (NetworkPrefab networkPrefab in Instance._networkPrefabs)
            {
                if (networkPrefab.Prefab == obj)
                {
                    if (networkPrefab.PrefabPath != string.Empty)
                    {
                        var result = PhotonNetwork.Instantiate(networkPrefab.PrefabPath, pos, rotation);
                        return result;
                    }
                    else
                    {
                        Debug.LogError("Path is empty for gameobject name");
                    }
                }
            }
            return null;
        }
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PopulateNetworkedPrefabs()
        {
#if UNITY_EDITOR
            Instance._networkPrefabs.Clear();
            GameObject[] results = Resources.LoadAll<GameObject>("");

            for (int i = 0; i < results.Length; i++)
            {
                 var path = AssetDatabase.GetAssetPath(results[i]);
                Instance._networkPrefabs.Add(new NetworkPrefab(results[i], path));
                
            }
#endif
        }
    }
}
