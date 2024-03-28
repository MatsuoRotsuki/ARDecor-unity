using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    FurnituresManager m_FurnituresManager;
    public FurnituresManager furnituresManager => m_FurnituresManager;

    public UnityEvent<BoxCollider> boxColliderCreated = new UnityEvent<BoxCollider>();

    [SerializeField]
    string m_ModelPath;
    public string modelPath
    {
        get => m_ModelPath;
        set => m_ModelPath = value;
    }

    [SerializeField]
    string m_modelID;
    public string modelID
    {
        get => m_modelID;
        set => m_modelID = value;
    }

    public TransformationNode node { get; set; }

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

        // Edit SelectionInteractable component to show selection indications
        GameObject newSelectionVisualizer = Instantiate(m_FurnituresManager.selectionVisualizationPrefab);
        newSelectionVisualizer.transform.parent = transform;
        newSelectionVisualizer.SetActive(false);
        newSelectionVisualizer.transform.localPosition = Vector3.zero;
        newSelectionVisualizer.transform.rotation = Quaternion.identity;
        var selectionVisualizer = newSelectionVisualizer.GetComponent<ARSelectionVisualizer>();
        selectionVisualizer.Initialize(m_Collider);
        m_SelectionInteractable.selectionVisualization = newSelectionVisualizer;
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

    protected void Start()
    {
        StartCoroutine(LerpObjectScale(Vector3.zero, Vector3.one, 0.3f));
    }

    IEnumerator LerpObjectScale(Vector3 a, Vector3 b, float time)
    {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i <= 1.0f)
        {
            i += Time.deltaTime * rate;
            transform.localScale = Vector3.Lerp(a, b, i);
            yield return null;
        }
        m_ScaleInteractable = gameObject.AddComponent<ARScaleInteractable>();
        // ARScaleInteractable Configuration
        m_ScaleInteractable.minScale = 0.5f;
        m_ScaleInteractable.maxScale = 2f;
        m_ScaleInteractable.sensitivity = 0.5f;
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
        m_FurnituresManager.OnFurnitureSelected(gameObject);
    }

    void OnFurnitureDeselected(SelectExitEventArgs args)
    {
        // m_ScreenManager.selectedObject = null;
        m_FurnituresManager.OnFurnitureDeselected();
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

        boxColliderCreated.Invoke(m_Collider);
    }

    public TransformationNode CreateNode(List<FurnitureObject> furnitureObjects)
    {
        var currentNode = new TransformationNode();
        currentNode.modelPath = modelPath;
        currentNode.SetRotation(transform.localRotation);
        if (transform.parent.gameObject.TryGetComponent<StackableObject>(out var parentStackable))
        {
            currentNode.SetPosition(transform.localPosition);
        }
        else
        {
            currentNode.SetPosition(transform.parent.localPosition);
        }

        if (transform.parent.gameObject.TryGetComponent<FurnitureObject>(out var parentFurnitureObject))
        {
            currentNode.parent = furnitureObjects.IndexOf(parentFurnitureObject);
        }
        else
        {
            currentNode.parent = -1;
        }

        node = currentNode;
        return currentNode;
    }
}

[Serializable]
public class TransformationNode
{
    public string modelPath;

    public Dictionary<string, float> position = new();

    public Dictionary<string, float> rotation = new();

    public void SetPosition(Vector3 position)
    {
        this.position = new Dictionary<string, float>() {
            {"X", position.x},
            {"y", position.y},
            {"z", position.z}
        };
    }

    public Vector3 GetPosition()
    {
        return new Vector3(this.position["x"], this.position["y"], this.position["z"]);
    }

    public void SetRotation(Quaternion rotation)
    {
        this.rotation = new Dictionary<string, float>() {
            {"X", rotation.x},
            {"y", rotation.y},
            {"z", rotation.z},
            {"w", rotation.w},
        };
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(this.rotation["x"], this.rotation["y"], this.rotation["z"], this.rotation["w"]);
    }

    public int? parent;

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new();
        result["modelPath"] = this.modelPath;
        result["position"] = this.position;
        result["rotation"] = this.rotation;
        result["parent"] = this.parent;
        return result;
    }

}
