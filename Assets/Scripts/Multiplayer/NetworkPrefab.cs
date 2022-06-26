using System;
using UnityEngine;

namespace _Scripts.Multiplayer
{
    [Serializable]
    public class NetworkPrefab
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private string prefabPath;

        public GameObject Prefab => prefab;
        public string PrefabPath => prefabPath;

        public NetworkPrefab(GameObject obj, string path)
        {
            prefab = obj;
            prefabPath = PrefabPathModifed(path);
        }

        private string PrefabPathModifed(string path)
        {
            var extensionLength = System.IO.Path.GetExtension(path).Length;
            int additionalLength = 10;
            var startIndex = path.ToLower().IndexOf("resources");

            if (startIndex == -1)
            {
                return string.Empty;
            }
            else
            {
                return path.Substring(startIndex + additionalLength, path.Length - (additionalLength + startIndex + extensionLength));
            }
        }
    }
}
