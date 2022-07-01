using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace AnalyticElements
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Singletons/ButtonManager")]
    public class ButtonManager : ScriptableSingleton<ButtonManager>
    {
        [NonSerialized] private List<ModifiedButton> _buttons = new List<ModifiedButton>();
        
        [NonSerialized] private ButtonCallbackProvider _callbackProvider;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void FirstInitialize()
        {
            Instance.InstantiateCallbackProvider();
        }
        
        private void InstantiateCallbackProvider()
        {
            _callbackProvider = new GameObject().AddComponent<ButtonCallbackProvider>();
            _callbackProvider.gameObject.name = "Objective Manager Callback Provider";
            DontDestroyOnLoad(_callbackProvider.gameObject);
        }
        
        public void Subscribe(ModifiedButton button)
        {

            _buttons.Add(button);
        }

        public void UnSubscribe(ModifiedButton button)
        {
            _buttons.Remove(button);
        }

        public void CloseSelf(ModifiedButton analyticButton,ButtonOpenType openType, float openDelay)
        {
            List<ModifiedButton> tempAnalyticButtons = new List<ModifiedButton>();
            DeActivateButton(analyticButton);
            tempAnalyticButtons.Add(analyticButton);
            ExecuteOpenMethod(tempAnalyticButtons, openType, openDelay);
        }

        public void CloseAllButtons(ButtonOpenType openType, float openDelay)
        {
            foreach (var button in _buttons)
            {
                DeActivateButton(button);
            }
            ExecuteOpenMethod(_buttons, openType, openDelay);
        }

        public void CloseButtonByGroup(ButtonTag groupType, ButtonOpenType openType, float openDelay)
        {
            List<ModifiedButton> tempAnalyticButtons = new List<ModifiedButton>();
            foreach (var button in _buttons)
            {
                if (groupType == button.GroupType)
                {
                    
                    DeActivateButton(button);
                    tempAnalyticButtons.Add(button);
                }
            }
            ExecuteOpenMethod(tempAnalyticButtons, openType, openDelay);
        }

        private void ExecuteOpenMethod(List<ModifiedButton> analyticButtons, ButtonOpenType openType, float openDelay)
        {
            switch (openType)
            {
                case ButtonOpenType.Timed:
                    _callbackProvider.StartCoroutine(ActivateButtonGroupWithDelay(analyticButtons, openDelay));
                    break;
                case ButtonOpenType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(openType), openType, null);
            }
        }

        public void ResetButtons()
        {
            foreach (var button in _buttons)
            {
                ActivateButton(button);
            }
        }

        public void ResetButtonsByGroup(ButtonTag type)
        {
            foreach (var button in _buttons)
            {
                if(button.GroupType == type) ActivateButton(button);
            }
        }

        private IEnumerator ActivateButtonGroupWithDelay(List<ModifiedButton> analyticButtons, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            foreach (var button in analyticButtons)
            {
                if(button) ActivateButton(button);
            }
        }
        
        private static void DeActivateButton(ModifiedButton button)
        {
            button.interactable = false;
            button.NotClickable = true;
        }

        private static void ActivateButton(ModifiedButton button)
        {
            button.interactable = true;
            button.NotClickable = false;
        }
    }
    
    public class ButtonCallbackProvider : MonoBehaviour
    {
        private ButtonManager cachedInstance;

        private void Awake()
        {
            cachedInstance = ButtonManager.Instance;
        }
    }
}