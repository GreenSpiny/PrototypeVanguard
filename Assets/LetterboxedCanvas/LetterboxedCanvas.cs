using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class LetterboxedCanvas : MonoBehaviour
{
    // User assigned fields
    [SerializeField] protected List<CameraInfo> assignedCameras;
    [SerializeField] protected float maxAspectRatio = 1.33f;
    [SerializeField] protected float minAspectRatio = 0.667f;
    [SerializeField] protected bool showVisibleArea;

    // Canvas object references
    [NonSerialized] protected CanvasScaler canvasScaler;
    [SerializeField] protected LayoutElement verticalArea;
    [SerializeField] protected LayoutElement visibleArea;
    [NonSerialized] protected Image visibleAreaImage;

    [SerializeField] float temp;

    Vector2 baseResolution;

    // Internal variables
    protected Vector2Int lastStoredResolution;
    protected bool canvasNeedsRefresh = false;

    // Testing objects
    [SerializeField] protected GameObject testObj;

    // === PUBLIC FUNCTIONS === //

    /// <summary>
    /// Returns the currently assigned maximum aspect ratio.
    /// </summary>
    /// <returns></returns>
    public float GetMaxAspect() { return maxAspectRatio; }

    /// <summary>
    /// Returns the currently assigned minimum aspect ratio.
    /// </summary>
    /// <returns></returns>
    public float GetMinAspect() { return minAspectRatio; }

    /// <summary>
    /// Sets a new maximum and minimum aspect ratio.
    /// </summary>
    /// <param name="max">The new maximum aspect.</param>
    /// <param name="min">The new minimum aspect.</param>
    public void SetAspectRatio(float max, float min)
    {
        if (max != maxAspectRatio || min != minAspectRatio)
        {
            maxAspectRatio = max;
            minAspectRatio = min;
            canvasNeedsRefresh = true;
        }
    }

    /// <summary>
    /// Returns the metadata of a camera assigned the letterbox, or null if the camera is not assigned.
    /// </summary>
    /// <param name="camera"></param>
    /// <returns></returns>
    public CameraInfo GetCamera(Camera camera)
    {
        foreach (CameraInfo cameraInfo in assignedCameras)
        {
            if (cameraInfo.camera == camera)
            {
                return cameraInfo;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the list of cameras scaled by the letterbox.
    /// </summary>
    /// <returns></returns>
    public List<CameraInfo> GetCameras()
    {
        return assignedCameras;
    }

    /// <summary>
    /// Mark the letterbox for refresh. This should be called after code you write that edits camera data.
    /// </summary>
    public void Refresh()
    {
        canvasNeedsRefresh = true;
    }

    /// <summary>
    /// Turns the checkered overlay on the visible area on or off.
    /// </summary>
    public void ToggleVisibleArea(bool toggle)
    {
        showVisibleArea = toggle;
        visibleAreaImage.enabled = showVisibleArea;
    }

    // === INTERNAL FUNCTIONS === //

    protected void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        baseResolution = canvasScaler.referenceResolution;
        verticalArea.preferredWidth = baseResolution.x;

        visibleAreaImage = visibleArea.GetComponent<Image>();
        visibleAreaImage.enabled = showVisibleArea;

        // Remove null and duplicate cameras
        List<int> duplicateIndicies = new List<int>();
        HashSet<Camera> usedCameras = new HashSet<Camera>();
        for (int i = 0; i < assignedCameras.Count; i++)
        {
            if (assignedCameras[i].camera == null)
            {
                duplicateIndicies.Add(i);
            }
            else if (!usedCameras.Add(assignedCameras[i].camera))
            {
                duplicateIndicies.Add(i);
            }
        }
        for (int j = duplicateIndicies.Count - 1; j >= 0; j--)
        {
            assignedCameras.RemoveAt(j);
            Debug.LogWarning("Warning: Removed null or duplicate camera from Letterboxed Canvas at index: " + j.ToString(), this);
        }
    }

    protected void OnEnable()
    {
        UpdateResolution();
    }

    protected void LateUpdate()
    {
        if (canvasNeedsRefresh || lastStoredResolution.x != Screen.width || lastStoredResolution.y != Screen.height)
        {
            UpdateResolution();
        }
    }

    protected void UpdateResolution()
    {
        lastStoredResolution = new Vector2Int(Screen.width, Screen.height);
        canvasNeedsRefresh = false;

        float aspect = (float) lastStoredResolution.x / lastStoredResolution.y;
        
        // If the aspect is too wide, we must reduce the width of each camera and realign them horizontally.
        if (aspect > maxAspectRatio)
        {
            float widthReduction = maxAspectRatio / aspect;
            foreach (CameraInfo info in assignedCameras)
            {
                info.camera.rect = new Rect(
                    info.viewportRect.x + (1f - 2 * info.viewportRect.x) * (1f - widthReduction) / 2f,
                    info.viewportRect.y,
                    info.viewportRect.width * widthReduction,
                    info.viewportRect.height);
            }
            visibleArea.preferredHeight = baseResolution.y;
            canvasScaler.referenceResolution = new Vector2(baseResolution.x / widthReduction, baseResolution.y);
        }
        // If the aspect is too tall, we must reduce the height of each camera and realign them vertically.
        else if (aspect < minAspectRatio)
        {
            float heightReduction = aspect / minAspectRatio;
            foreach (CameraInfo info in assignedCameras)
            {
                info.camera.rect = new Rect(
                    info.viewportRect.x,
                    info.viewportRect.y + (1f - 2 * info.viewportRect.y) * (1f - heightReduction) / 2f,
                    info.viewportRect.width,
                    info.viewportRect.height * heightReduction);
            }
            visibleArea.preferredHeight = baseResolution.y * heightReduction / aspect;
            if (aspect < 1f)
            {
                canvasScaler.referenceResolution = new Vector2(baseResolution.x, baseResolution.y / heightReduction);
            }
            else
            {
                canvasScaler.referenceResolution = baseResolution;
            }
        }
        // If the aspect is within parameters, the original camera rects can be preserved.
        else
        {
            foreach (CameraInfo info in assignedCameras)
            {
                info.camera.rect = info.viewportRect;
            }
            visibleArea.preferredHeight = baseResolution.y;
            canvasScaler.referenceResolution = baseResolution;
        }
        Canvas.ForceUpdateCanvases();
    }

    [System.Serializable]
    public class CameraInfo
    {
        [SerializeField] public Camera camera;
        [SerializeField] public Rect viewportRect = new Rect(0f, 0f, 1f, 1f);

        public CameraInfo(Camera camera, Rect viewportRect)
        {
            this.camera = camera;
            this.viewportRect = viewportRect;
        }
    }
}
