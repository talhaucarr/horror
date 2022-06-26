using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace _Scripts.Multiplayer.Room
{
    public class RandomCustomPropertyGenerator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        private Hashtable _myCustomProperties = new Hashtable();

        private void SetCustomNumber()
        {
            System.Random rnd = new System.Random();
            int result = rnd.Next(0, 99);

            text.text = result.ToString();
            _myCustomProperties["RandomNumber"] = result;
            PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        }
        
        public void GetNumber()
        {
            SetCustomNumber();
        }
    }
}
