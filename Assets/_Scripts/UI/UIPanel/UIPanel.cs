using System;
using Photon.Pun;
using UnityEngine;

namespace _Scripts.UI.UIPanel
{
    public abstract class UIPanel : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string panelName;

        public string PanelName => panelName;
        
        public abstract void OnOpenPanel();
        public abstract void OnClosePanel();

        public virtual void OpenPanel()
        {
            gameObject.SetActive(true);
            OnOpenPanel();
        }

        public virtual void ClosePanel()
        {
            OnClosePanel();
            gameObject.SetActive(false);
        }
    }
}
