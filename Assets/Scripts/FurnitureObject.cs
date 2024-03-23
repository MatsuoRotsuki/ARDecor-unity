using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class FurnitureObject : MonoBehaviour
{
    ARSelectionInteractable m_SelectionInteractable;

    ARTranslationExtensionInteractable m_TranslationInteractable;

    ARGizmoPositioningInteractable m_GizmoInteractable;

    ARRotationInteractable m_RotationInteractable;

    ARScaleInteractable m_ScaleInteractable;

    ARGestureInteractor m_GestureInteractor;

    [SerializeField]
    BoxCollider m_Collider;
    public BoxCollider boxCollider => m_Collider;

    private FurnituresManager m_FurnituresManager;
    public FurnituresManager furnituresManager => m_FurnituresManager;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Awake()
    {
        // Register
        m_FurnituresManager = FindObjectOfType<FurnituresManager>();
        if (m_FurnituresManager == null)
        {
            Debug.LogWarning($"Could not find {nameof(FurnituresManager)} in scene.");
            return;
        }
        m_FurnituresManager.RegisterFurniture(this);

        // Combine meshes and create Box Collider
        CombineMeshes();

        m_SelectionInteractable = gameObject.AddComponent<ARSelectionInteractable>();
        m_TranslationInteractable = gameObject.AddComponent<ARTranslationExtensionInteractable>();
        m_GizmoInteractable = gameObject.AddComponent<ARGizmoPositioningInteractable>();
        m_RotationInteractable = gameObject.AddComponent<ARRotationInteractable>();
        m_ScaleInteractable = gameObject.AddComponent<ARScaleInteractable>();

        // Edit SelectionInteractable component to show selection indications
        GameObject newSelectionVisualizer = Instantiate(m_FurnituresManager.selectionVisualizationPrefab);
        newSelectionVisualizer.transform.parent = transform;
        newSelectionVisualizer.SetActive(false);
        newSelectionVisualizer.transform.localPosition = Vector3.zero;
        newSelectionVisualizer.transform.rotation = Quaternion.identity;
        m_SelectionInteractable.selectionVisualization = newSelectionVisualizer;

        // ARScaleInteractable Configuration
        m_ScaleInteractable.minScale = 0.5f;
        m_ScaleInteractable.maxScale = 2f;
        m_ScaleInteractable.sensitivity = 0.5f;

    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnEnable()
    {
        m_SelectionInteractable.selectEntered.AddListener(OnFurnitureSelected);
        m_SelectionInteractable.selectExited.AddListener(OnFurnitureDeselected);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDisable()
    {
        m_SelectionInteractable.selectEntered.RemoveListener(OnFurnitureSelected);
        m_SelectionInteractable.selectExited.RemoveListener(OnFurnitureDeselected);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDestroy()
    {
        // Don't need to do anything; method kept for backwards compatibility
    }

    void OnFurnitureSelected(SelectEnterEventArgs args)
    {
        // m_ScreenManager.selectedObject = this.gameObject;
        // m_ScreenManager.OnSelectObject();
    }

    void OnFurnitureDeselected(SelectExitEventArgs args)
    {
        // m_ScreenManager.selectedObject = null;
        // m_ScreenManager.OnDeselectObject();
    }

    void CombineMeshes()
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

        // Add box collider to root gameObject
        m_Collider = gameObject.AddComponent<BoxCollider>();

        Bounds combineBounds = finalMesh.bounds;
        Vector3 localCenter = transform.InverseTransformPoint(combineBounds.center);

        m_Collider.center = localCenter;
        m_Collider.size = combineBounds.size;
    }
}
