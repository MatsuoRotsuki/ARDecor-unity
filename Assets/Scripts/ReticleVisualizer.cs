using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ReticleVisualizer : MonoBehaviour
{

    private XROrigin m_XROrigin;

    private ARPlaneManager m_PlaneManager;

    private ARRaycastManager m_RaycastManager;

    private static List<ARRaycastHit> s_Hits = new();

    private readonly Vector2 kScreenCenterPosition = new(Screen.width / 2, Screen.height / 2);

    [SerializeField]
    private GameObject sprite;

    public Vector3 placementPosition;

    private bool planeDetected = false;
    public UnityEvent onPlaneDetected = new UnityEvent();

    private void Awake()
    {
        m_XROrigin = FindObjectOfType<XROrigin>();
        if (m_XROrigin == null)
        {
            Debug.LogError("XROrigin not exist.");
        }

        if (!m_XROrigin.TryGetComponent(out m_PlaneManager))
        {
            Debug.LogError("ARPlaneManager not exist.");
        }

        if (!m_XROrigin.TryGetComponent(out m_RaycastManager))
        {
            Debug.LogError("ARRaycastManager not exist.");
        }

        if (sprite == null)
        {
            Debug.LogError("Sprite need to be initialized.");
        }
        else
        {
            sprite.SetActive(false);
        }
    }

    private void Update()
    {
        if (m_RaycastManager == null)
            return;

        if (sprite == null)
            return;

        if (m_RaycastManager.Raycast(kScreenCenterPosition, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = s_Hits[0].pose;
            gameObject.transform.position = hitPose.position;
            placementPosition = hitPose.position;
            sprite.SetActive(true);

            if (!planeDetected)
            {
                onPlaneDetected.Invoke();
            }
            planeDetected = true;
        }
        else
        {
            sprite.SetActive(false);
        }
    }
}
