using UnityEditor;
using UnityEngine;
[CanEditMultipleObjects]
[CustomEditor(typeof(ModifiedButton))]
public class ModifiedButtonEditor : UnityEditor.UI.ButtonEditor
{
    private SerializedProperty eventName, parameterName, parameterValue, playsAnimation, buttonSound, buttonGroup, closeType, openType, openDelay, sendsAnalytics, fadeDuration, buttonAnimType;

    protected override void OnEnable()
    {
        base.OnEnable();
        eventName = serializedObject.FindProperty("eventName");
        parameterName = serializedObject.FindProperty("parameterName");
        parameterValue = serializedObject.FindProperty("parameterValue");
        buttonSound = serializedObject.FindProperty("buttonSound");
        playsAnimation = serializedObject.FindProperty("playsAnimation");
        buttonGroup = serializedObject.FindProperty("buttonGroup");
        closeType = serializedObject.FindProperty("closeType");
        openType = serializedObject.FindProperty("openType");
        openDelay = serializedObject.FindProperty("openDelay");
        sendsAnalytics = serializedObject.FindProperty("sendsAnalytics");
        fadeDuration = serializedObject.FindProperty("duration");
        buttonAnimType = serializedObject.FindProperty("buttonAnimType");
    }

    public override void OnInspectorGUI()
    {
        //serialize additional parameters
        serializedObject.Update();
        EditorGUILayout.PropertyField(closeType);
        if ((ButtonCloseType) closeType.enumValueIndex == ButtonCloseType.CloseByGroup)
        {
            EditorGUILayout.PropertyField(buttonGroup);
        }
        EditorGUILayout.PropertyField(openType);
        if ((ButtonOpenType) openType.enumValueIndex == ButtonOpenType.Timed) EditorGUILayout.PropertyField(openDelay);
        if((ButtonCloseType) closeType.enumValueIndex == ButtonCloseType.CloseByGroup && (ButtonTag)buttonGroup.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Group Button Tag is Missing !", MessageType.Error);
        }
        EditorGUILayout.PropertyField(sendsAnalytics);
        if (sendsAnalytics.boolValue)
        {
            EditorGUILayout.PropertyField(eventName);
            EditorGUILayout.PropertyField(parameterName);
            EditorGUILayout.PropertyField(parameterValue);
        }
        
        EditorGUILayout.PropertyField(playsAnimation);
        if (playsAnimation.boolValue)
        {
            EditorGUILayout.PropertyField(buttonAnimType);
            if ((ButtonAnimType) buttonAnimType.enumValueIndex != ButtonAnimType.None)
            {
                EditorGUILayout.PropertyField(fadeDuration);
            }
        }

        EditorGUILayout.PropertyField(buttonSound);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(20);

        base.OnInspectorGUI();
    }
}