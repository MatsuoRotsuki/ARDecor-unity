using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private GameObject m_SelectionVisualizationPrefab;

    /// <summary>
    /// 
    /// </summary>
    public GameObject selectionVisualizationPrefab
    {
        get => m_SelectionVisualizationPrefab;
        set => m_SelectionVisualizationPrefab = value;
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    protected void Awake()
    {
        //TODO
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

    public struct HitStackable
    {
        public RaycastHit hitInfo { get; set; }
        public StackableObject stackble { get; set; }
    }
}
