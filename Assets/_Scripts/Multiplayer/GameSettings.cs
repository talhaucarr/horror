using UnityEngine;

namespace _Scripts.Multiplayer
{
    [CreateAssetMenu(menuName = "ScriptableObjects/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private string gameVersion;
        [SerializeField] private string nickName = "noname";

        public string GameVersion => gameVersion;

        public string NickName
        {
            get
            {
                int value = Random.Range(0, 1031);
                return nickName + value.ToString();
            }
        }
    }
}
