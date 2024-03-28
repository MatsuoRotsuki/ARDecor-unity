using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FurnituresManager : MonoBehaviour
{
    readonly List<FurnitureObject> m_Furnitures = new();

    /// <summary>
    /// 
    /// </summary>
    public List<FurnitureObject> furnitures => m_Furnitures;

    readonly List<StackableObject> m_Stackables = new();

    /// <summary>
    /// 
    /// </summary>
    public List<StackableObject> stackables => m_Stackables;

    [SerializeField]
    GameObject m_SelectionVisualizationPrefab;

    /// <summary>
    /// 
    /// </summary>
    public GameObject selectionVisualizationPrefab
    {
        get => m_SelectionVisualizationPrefab;
        set => m_SelectionVisualizationPrefab = value;
    }

    [SerializeField]
    GameObject m_AnchorPrefab;

    public GameObject anchorPrefab => m_AnchorPrefab;

    ARAnchor m_Anchor;

    ARAnchorManager m_AnchorManager;

    ARPlaneManager m_PlaneManager;

    ARRaycastManager m_RaycastManager;

    FirebaseManager m_FirebaseManager;

    ScreenManager m_ScreenManager;

    public ARAnchor anchor => m_Anchor;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Awake()
    {
        m_AnchorManager = FindObjectOfType<ARAnchorManager>();
        if (m_AnchorManager == null)
        {
            Debug.LogWarning($"Could not find component {nameof(ARAnchorManager)} in scene.");
        }

        m_PlaneManager = FindObjectOfType<ARPlaneManager>();
        if (m_PlaneManager == null)
        {
            Debug.LogWarning($"Could not find component {nameof(ARPlaneManager)} in scene.");
        }

        m_RaycastManager = FindObjectOfType<ARRaycastManager>();
        if (m_RaycastManager == null)
        {
            Debug.LogWarning($"Could not find component {nameof(ARRaycastManager)} in scene.");
        }

        m_FirebaseManager = FindObjectOfType<FirebaseManager>();
        if (m_FirebaseManager == null)
        {
            Debug.LogWarning($"Could not find component {nameof(FirebaseManager)} in scene.");
        }

        m_ScreenManager = FindObjectOfType<ScreenManager>();
        if (m_ScreenManager == null)
        {
            Debug.LogWarning($"Could not find component {nameof(ScreenManager)} in scene.");
        }
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnEnable()
    {

    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void OnDisable()
    {

    }

    // /// <summary>
    // /// /// See <see cref="MonoBehaviour"/>.
    // /// </summary>
    // protected void Update()
    // {
    // }

    public void RegisterFurniture(FurnitureObject furnitureObj)
    {
        m_Furnitures.Add(furnitureObj);
        //OnRegistered
    }

    public void UnregisterFurniture(FurnitureObject furnitureObj)
    {
        m_Furnitures.Remove(furnitureObj);
        //OnUnregistered
    }

    public void RegisterStackable(StackableObject stackable)
    {
        m_Stackables.Add(stackable);
    }

    public void UnregisterStackable(StackableObject stackable)
    {
        m_Stackables.Remove(stackable);
    }

    void OnRegistered()
    {

    }

    void OnUnregistered()
    {

    }

    public bool RaycastFurniture(Ray ray, out Vector3 hitPosition, out StackableObject hitStackableObj, GameObject selectedObj)
    {
        List<HitStackable> hitStackables = new();
        foreach (StackableObject stackable in m_Stackables)
        {
            if (stackable.gameObject == selectedObj)
            {
                continue;
            }
            if (stackable.RaycastOnTop(ray, out RaycastHit hitInfo))
            {
                var hitStackable = new HitStackable
                {
                    hitInfo = hitInfo,
                    stackble = stackable
                };

                hitStackables.Add(hitStackable);
            }
        }

        if (hitStackables.Count > 0)
        {
            var orderedHitStackables = hitStackables
            .OrderBy(stackable => stackable.hitInfo.distance)
            .ToList();

            var closestStackable = orderedHitStackables[0];
            hitStackableObj = closestStackable.stackble;
            hitPosition = closestStackable.hitInfo.point;
            return true;
        }

        hitStackableObj = default;
        hitPosition = default;
        return false;
    }

    public void OnFurnitureSelected(GameObject gameObject)
    {
        m_ScreenManager.OnSelectObject(gameObject);
    }

    public void OnFurnitureDeselected()
    {
        m_ScreenManager.OnDeselectObject();
    }

    public void OnClickSave()
    {
        if (m_Furnitures.Count == 0)
        {
            return;
        }

        Vector3 result = Vector3.zero;
        List<FurnitureObject> rootObjects = new();
        foreach (FurnitureObject furnitureObject in m_Furnitures)
        {
            bool isParentNotStackable = !furnitureObject.transform.parent.gameObject.TryGetComponent(out StackableObject parentStackable);
            if (isParentNotStackable)
            {
                rootObjects.Add(furnitureObject);
                result += furnitureObject.transform.position;
            }
        }

        Vector3 centerPosition = result / rootObjects.Count;
        var newAnchor = new GameObject("GeneralAnchor");
        newAnchor.transform.position = centerPosition;
        newAnchor.transform.rotation = Quaternion.identity;

        List<Dictionary<string, object>> dataToStore = new();
        foreach (var rootObj in rootObjects)
        {
            rootObj.transform.parent.parent = newAnchor.transform;
        }

        foreach (var furnitureObj in m_Furnitures)
        {
            var node = furnitureObj.CreateNode(m_Furnitures);
            dataToStore.Add(node.ToDictionary());
        }

        m_FirebaseManager.databaseRoofReference.Child("SAVE_SLOT").Child(m_FirebaseManager.firebaseUser.UserId).SetValueAsync(dataToStore)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Writing success");
                }
            });

        foreach (var rootObj in rootObjects)
        {
            rootObj.transform.parent.parent = null;
        }

        Destroy(newAnchor);
    }

    public void OnClickLoad()
    {
        StartCoroutine(LoadSaveSlot());
    }

    IEnumerator LoadSaveSlot()
    {
        var task = m_FirebaseManager
            .databaseRoofReference
            .Child("SAVE_SLOT").Child(m_FirebaseManager.firebaseUser.UserId)
            .GetValueAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        DataSnapshot snapshot = task.Result;
        string jsonData = snapshot.GetRawJsonValue();
        List<GameObject> recreatedGameObjects = new();

        var savedFurnitureObjs = JsonConvert.DeserializeObject<List<Node>>(jsonData);

        //Store created objects in list
        foreach (var savedFurniture in savedFurnitureObjs)
        {
            string modelPath = savedFurniture.modelPath;
            var loadModelTask = m_FirebaseManager.LoadModelAsync(modelPath);

            yield return new WaitUntil(() => loadModelTask.IsCompleted);

            if (loadModelTask.IsCompleted)
            {
                var createdGameObject = loadModelTask.Result;
                recreatedGameObjects.Add(createdGameObject);
            }
        }

        for (int i = 0; i < recreatedGameObjects.Count; i++)
        {
            if (!recreatedGameObjects[i].TryGetComponent(out FurnitureObject furnitureObject))
            {
                furnitureObject = recreatedGameObjects[i].AddComponent<FurnitureObject>();
            }
            furnitureObject.modelPath = savedFurnitureObjs[i].modelPath;
        }

        List<GameObject> rootObjects = new();
        for (int i = 0; i < recreatedGameObjects.Count; i++)
        {
            Vector3 position = savedFurnitureObjs[i].position.Value;
            Quaternion rotation = savedFurnitureObjs[i].rotation.Value;
            int parent = savedFurnitureObjs[i].parent;
            if (parent != -1)
            {
                var parentObject = recreatedGameObjects[i].transform.parent.gameObject;
                recreatedGameObjects[i].transform.parent = recreatedGameObjects[parent].transform;
                Destroy(parentObject);

                recreatedGameObjects[i].transform.localPosition = position;
                recreatedGameObjects[i].transform.localRotation = rotation;

                //Add StackableObject if not included
                if (!recreatedGameObjects[parent].TryGetComponent(out StackableObject stackableParent))
                {
                    recreatedGameObjects[parent].AddComponent<StackableObject>();
                }
            }
            else
            {
                recreatedGameObjects[i].transform.parent.localPosition = position;
                recreatedGameObjects[i].transform.localRotation = rotation;
                rootObjects.Add(recreatedGameObjects[i]);
            }
        }

        //Group root object
        var newAnchor = Instantiate(m_AnchorPrefab, Vector3.zero, Quaternion.identity);
        foreach (var rootObj in rootObjects)
        {
            rootObj.transform.parent.parent = newAnchor.transform;
        }

        if (newAnchor.TryGetComponent<GeneralAnchor>(out var generalAnchor))
        {
            generalAnchor.Reinitialize();
        }
    }

    public struct HitStackable
    {
        public RaycastHit hitInfo { get; set; }
        public StackableObject stackble { get; set; }
    }
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
    public Vector3 Value
    {
        get => new Vector3(x, y, z);
    }
}

[System.Serializable]
public class Rotation
{
    public float x;
    public float y;
    public float z;
    public float w;
    public Quaternion Value
    {
        get => new Quaternion(x, y, z, w);
    }
}

[System.Serializable]
public class Node
{
    public string modelPath;
    public Position position;
    public Rotation rotation;
    public int parent;
}
