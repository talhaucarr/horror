using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnalyticToggle : Toggle
{
    public enum InteractableTypes { button, toggle}

    [Header("Analytics")]
    public InteractableTypes type = InteractableTypes.button;
    public string eventName;
    public string parameterName;
    public string parameterValue;
    public bool isPlayAnim;
    public Animator Animator;
    public UnityEvent OnClick;
    
    /// <summary>
    /// ///////////////////////
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerClick(PointerEventData eventData)
    {
        //UIManager.Instance.ButtonSound();
        
        base.OnPointerClick(eventData);
        OnClick.Invoke();
        
        if (isPlayAnim) PlayAnim();
        //serialized analytic event parameters
        string evName = String.IsNullOrEmpty(eventName) ? type.ToString() + "_click" : eventName;
        string pName = String.IsNullOrEmpty(parameterName) ?  type.ToString() + "_name" : parameterName;
        string pValue = String.IsNullOrEmpty(parameterValue) ? gameObject.name : parameterValue;

        //Firebase.Analytics.FirebaseAnalytics.LogEvent(evName, pName, pValue);
        Debug.Log($"Analytic Sent: {evName}, {pName}, {pValue}");
        
    }

    public void PlayAnim()
    {
        animator.SetBool("IsOn", isOn);
    }

}
