using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using static LetterboxedCanvas;
using Unity.VisualScripting;

[CanEditMultipleObjects]
[CustomEditor(typeof(LetterboxedCanvas))]
public class LetterboxedCanvasInspector : Editor
{
    SerializedProperty assignedCameras;
    SerializedProperty maxAspectRatio;
    SerializedProperty minAspectRatio;
    SerializedProperty showVisibleArea;

    SerializedProperty verticalArea;
    SerializedProperty visibleArea;

    Image visibleAreaImage;

    void OnEnable()
    {
        assignedCameras = serializedObject.FindProperty("assignedCameras");
        maxAspectRatio = serializedObject.FindProperty("maxAspectRatio");
        minAspectRatio = serializedObject.FindProperty("minAspectRatio");
        showVisibleArea = serializedObject.FindProperty("showVisibleArea");

        verticalArea = serializedObject.FindProperty("verticalArea");
        visibleArea = serializedObject.FindProperty("visibleArea");

        visibleAreaImage = visibleArea.objectReferenceValue.GetComponent<Image>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = true;
        EditorGUILayout.PropertyField(maxAspectRatio);
        EditorGUILayout.PropertyField(minAspectRatio);
        EditorGUILayout.PropertyField(showVisibleArea);
        visibleAreaImage.enabled = showVisibleArea.boolValue;

        EditorGUILayout.PropertyField(assignedCameras);

        var cameraInfos = assignedCameras.GetUnderlyingValue() as List<CameraInfo>;
        foreach (var cameraInfo in cameraInfos)
        {
            cameraInfo.viewportRect.width = cameraInfo.camera == null && cameraInfo.viewportRect.width == 0 ? 1 : cameraInfo.viewportRect.width;
            cameraInfo.viewportRect.height = cameraInfo.camera == null && cameraInfo.viewportRect.height == 0 ? 1 : cameraInfo.viewportRect.height;
        }

        EditorGUILayout.PropertyField(verticalArea);
        EditorGUILayout.PropertyField(visibleArea);

        serializedObject.ApplyModifiedProperties();
    }
}