using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Singletons/Editor/ButtonManager")]
public class EditorResources : EditorScriptableSingleton<EditorResources>
{
    public RuntimeAnimatorController UIPanelAnimationController;

    public ScriptableObject HierarchyIconSettings;

    public AudioClip DefaultButtonSound;
}
