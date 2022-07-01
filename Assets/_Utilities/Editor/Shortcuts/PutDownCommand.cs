using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class PutDownCommand
{
    private static float maxDistance = Mathf.Infinity;
    private static float inGroundDistance = 0.04f;
    
    //[MenuItem("GameObject/Put Down Ctrl Alt Z  %&z")]
    [MenuItem("GameObject/Put Down Ctrl Alt Z  %&z")]
    private static void GroupSelected()
    {
        if (!Selection.activeTransform) return;

        Undo.RegisterCompleteObjectUndo(Selection.transforms, "Back to position");
        
        foreach (var transform in Selection.transforms)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 100, Vector3.down, maxDistance);
            
            foreach (RaycastHit hit  in hits)
            {
                if(hit.collider.gameObject == transform.gameObject)
                    continue;

                
                transform.position = hit.point - new Vector3(0,  inGroundDistance, 0);
                
                break;
            }
        }
    }
}
#endif
