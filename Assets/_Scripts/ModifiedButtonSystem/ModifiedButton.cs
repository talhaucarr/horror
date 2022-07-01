using System;
using System.Collections;
using AnalyticElements;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModifiedButton : Button
{
    [BHeader("Group Settings")]
    [SerializeField] private ButtonCloseType closeType = ButtonCloseType.CloseAll;
    [SerializeField] private ButtonTag buttonGroup = null;
    [SerializeField] private ButtonOpenType openType = ButtonOpenType.Timed;
    [Space(10)] [SerializeField] private float openDelay = 0.2f;

    [BHeader("Animation")] 
    [SerializeField] private bool playsAnimation;
    [SerializeField] private ButtonAnimType buttonAnimType;
    [SerializeField] private float duration = 0.5f;

    [BHeader("Analytics")]
    [SerializeField] private bool sendsAnalytics = true;
    [SerializeField] private string eventName;
    [SerializeField] private string parameterName;
    [SerializeField] private string parameterValue;
    
    [BHeader("Sound")] 
    [SerializeField] private AudioClip buttonSound;

    #region EVENTS
    public UnityEvent OnButtonDown = new UnityEvent();
    public UnityEvent OnButtonUp = new UnityEvent();
    #endregion

    #region FIELDS

    private bool notClickable = false;

    #endregion


    #region PROPERTY

    public ButtonTag GroupType
    {
        get => buttonGroup;
        set => buttonGroup = value;
    }
    
    public ButtonCloseType CloseType
    {
        get => closeType;
        set => closeType = value;
    }
    
    public bool NotClickable
    {
        get => notClickable;
        set => notClickable = value;
    }
    
    public bool IsAnimBool
    {
        get => playsAnimation;
        set => playsAnimation = value;
    }

    #endregion
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (notClickable) return;
        if (playsAnimation) PlayAnim();

        base.OnPointerClick(eventData);

        //VibrationManager.TriggerHaptic(VibrationType.Selection);

        //serialized analytic event parameters
        CallAnalytics();

        switch (closeType)
        {
            case ButtonCloseType.CloseByGroup:
                ButtonManager.Instance.CloseButtonByGroup(buttonGroup, openType, openDelay);
                break;
            case ButtonCloseType.CloseAll:
                ButtonManager.Instance.CloseAllButtons(openType, openDelay);
                break;
            case ButtonCloseType.CloseSelf:
                ButtonManager.Instance.CloseSelf(this, openType, openDelay);
                break;
            default:
                break;
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnButtonDown.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnButtonUp.Invoke();
    }

    private void CallAnalytics()
    {
        string evName = String.IsNullOrEmpty(eventName) ? "button_click" : eventName;
        string pName = String.IsNullOrEmpty(parameterName) ? "button_name" : parameterName;
        string pValue = parameterValue;
        if (String.IsNullOrEmpty(parameterValue))
        {
            if (gameObject.transform.parent != null) pValue = gameObject.transform.parent.name + "_" + gameObject.name;
            else pValue = gameObject.name;
        } 
        Debug.Log($"FirebaseLog: {evName}, {pName}, {pValue}");
    }

    protected override void Awake()
    {
        base.Awake();
        if(ButtonManager.Instance)
            ButtonManager.Instance.Subscribe(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(ButtonManager.Instance)
            ButtonManager.Instance.UnSubscribe(this);
    }

    private void PlayAnim()
    {
        switch (buttonAnimType)
        {
            case ButtonAnimType.None:
                break;
            case ButtonAnimType.ScaleAnim:
                gameObject.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), duration);
                Invoke(nameof(ReScaleInvoke), 0.3f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ReScaleInvoke()
    {
        gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), duration);
    }
}