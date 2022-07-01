using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public class SelectAllHiddenObjects : EditorWindow
{

    private Mesh meshToReplace;
    private Mesh newMesh;

    [MenuItem("Tools/Select Hidden Objects")]
    static void CreateReplaceWindow()
    {
        EditorWindow.GetWindow<SelectAllHiddenObjects>();
    }

    private void OnGUI()
    {
        List<Object> hiddenObjects = new List<Object>();
        if (GUILayout.Button("Select Hidden Objects"))
        {
            int selectionCount = 0;
            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!renderer.isVisible)
                {
                    hiddenObjects.Add(renderer.gameObject);
                    selectionCount++;
                }
            }
            Selection.objects = hiddenObjects.ToArray();
            Debug.Log(string.Format("Selected {0} invisible objects", selectionCount));
        }
    }
}
#endif