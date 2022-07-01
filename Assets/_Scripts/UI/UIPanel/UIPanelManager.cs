using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.UI.UIPanel
{
    public class UIPanelManager : PersistentAutoSingleton<UIPanelManager>
    {
        [SerializeField] private UIPanel mainPanel;
        [SerializeField] private List<UIPanel> _uiPanels = new List<UIPanel>();

        private void Start()
        {
            OpenPanel(mainPanel.PanelName);
        }

        public void OpenPanel(string panelName)
        {
            foreach (UIPanel panel in _uiPanels)
            {
                if (panel.PanelName == panelName)
                {
                    panel.OpenPanel();
                }
                else
                {
                    panel.ClosePanel();
                }
            }
        }
    }
}
