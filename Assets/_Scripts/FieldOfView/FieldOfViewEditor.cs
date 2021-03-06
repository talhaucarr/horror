using _Scripts.FieldOfView;
using UnityEditor;
using UnityEngine;

#if  UNITY_EDITOR


[CustomEditor(typeof(FieldOfViewModule))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfViewModule fov = (FieldOfViewModule)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.CameraTransform.position, Vector3.up, Vector3.forward, 360, fov.radius);

        Vector3 viewAngle01 = DirectionFromAngle(fov.CameraTransform.eulerAngles.y, -fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.CameraTransform.eulerAngles.y, fov.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.CameraTransform.position, fov.CameraTransform.position + viewAngle01 * fov.radius);
        Handles.DrawLine(fov.CameraTransform.position, fov.CameraTransform.position + viewAngle02 * fov.radius);

        if (fov.canSeePlayer)
        {
            Handles.color = Color.green;
            Handles.DrawLine(fov.CameraTransform.position, fov.playerRef.transform.position);
        }
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
#endif