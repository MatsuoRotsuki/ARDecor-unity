using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class GeneralAnchor : MonoBehaviour
{
    [SerializeField]
    Vector3 m_Center;

    public Vector3 center
    {
        get => m_Center;
        set => m_Center = value;
    }

    [SerializeField]
    Vector3 m_Size;

    public Vector3 size
    {
        get => m_Size;
        set => m_Size = value;
    }

    BoxCollider m_Collider;

    ARSelectionInteractable m_SelectionInteractable;

    ARTranslationExtensionInteractable m_TranslationInteractable;

    ARGizmoPositioningInteractable m_GizmoInteractable;

    ARRotationInteractable m_RotationInteractable;

    LineRenderer m_LineRenderer;

    protected void Awake()
    {
        m_LineRenderer = GetComponent<LineRenderer>();
        if (m_LineRenderer == null)
        {
            Debug.LogError($"Could not find component {nameof(BoxCollider)} in current GameObject");
            return;
        }
    }

    public void Reinitialize()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Mesh finalMesh = new();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combine, true);

        m_Collider = gameObject.AddComponent<BoxCollider>();

        Bounds combineBounds = finalMesh.bounds;
        Vector3 localCenter = transform.InverseTransformPoint(combineBounds.center);

        m_Center = localCenter;
        m_Size = combineBounds.size;

        m_Collider.center = localCenter;
        m_Collider.size = combineBounds.size + Vector3.one * 0.1f;

        MakeBoundaryLines();

        PushInteractables();
    }

    void MakeBoundaryLines()
    {
        m_LineRenderer.startWidth = 0.015f;
        m_LineRenderer.endWidth = 0.015f;
        m_LineRenderer.loop = true;

        Vector3[] positions = new Vector3[5];
        Vector3 size = (m_Size / 2f);

        Vector3[] vertices = new Vector3[8]
        {
            center + new Vector3(-size.x, -size.y, -size.z),
            center + new Vector3(size.x, -size.y, -size.z),
            center + new Vector3(-size.x, -size.y, size.z),
            center + new Vector3(size.x, -size.y, size.z),
            center + new Vector3(-size.x, size.y, -size.z),
            center + new Vector3(size.x, size.y, -size.z),
            center + new Vector3(-size.x, size.y, size.z),
            center + new Vector3(size.x, size.y, size.z),
        };

        int[] edgeIndices = new int[]
        {
            0, 1, 3, 2, 0, 4, 5, 1, 5, 7, 3, 7, 6, 2, 6, 4
        };
        m_LineRenderer.positionCount = edgeIndices.Length;

        for (int i = 0; i < edgeIndices.Length; i++)
        {
            m_LineRenderer.SetPosition(i, vertices[edgeIndices[i]]);
        }
        m_LineRenderer.enabled = false;
    }

    void PushInteractables()
    {
        if (TryGetComponent(out m_SelectionInteractable))
        {
            Destroy(m_SelectionInteractable);
        }

        if (TryGetComponent(out m_TranslationInteractable))
        {
            Destroy(m_TranslationInteractable);
        }

        if (TryGetComponent(out m_GizmoInteractable))
        {
            Destroy(m_GizmoInteractable);
        }

        if (TryGetComponent(out m_RotationInteractable))
        {
            Destroy(m_RotationInteractable);
        }

        m_SelectionInteractable = gameObject.AddComponent<ARSelectionInteractable>();
        m_TranslationInteractable = gameObject.AddComponent<ARTranslationExtensionInteractable>();
        m_GizmoInteractable = gameObject.AddComponent<ARGizmoPositioningInteractable>();
        m_RotationInteractable = gameObject.AddComponent<ARRotationInteractable>();

        m_SelectionInteractable.selectEntered.AddListener(OnAnchorSelected);
        m_SelectionInteractable.selectExited.AddListener(OnAnchorDeselected);
    }

    public void OnAnchorSelected(SelectEnterEventArgs args)
    {
        m_LineRenderer.enabled = true;
        var m_ScreenManager = FindObjectOfType<ScreenManager>();
        if (m_ScreenManager != null)
        {
            m_ScreenManager.OnSelectAnchor(gameObject);
        }
    }

    public void OnAnchorDeselected(SelectExitEventArgs args)
    {
        m_LineRenderer.enabled = false;
        var m_ScreenManager = FindObjectOfType<ScreenManager>();
        if (m_ScreenManager != null)
        {
            m_ScreenManager.OnDeselectAnchor();
        }
    }

    public void OnComplete()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).parent = null;
        }

        Destroy(gameObject);
    }
}