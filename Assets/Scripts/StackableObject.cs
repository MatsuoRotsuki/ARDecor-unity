using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FurnitureObject))]
public class StackableObject : MonoBehaviour
{
    private Collider m_Collider;
    public Collider boxCollider => m_Collider;
    private FurnituresManager m_FurnitureManager;

    private FurnitureObject m_FurnitureObject;
    public FurnitureObject furnitureObject => m_FurnitureObject;

    private void Awake()
    {
        m_FurnitureManager = FindObjectOfType<FurnituresManager>();
        if (m_FurnitureManager == null)
        {
            Debug.LogError("Furniture Manager not found.");
        }
    }

    private void Start()
    {
        if (TryGetComponent(out m_FurnitureObject))
        {
            m_Collider = m_FurnitureObject.boxCollider;
        }
        else
        {
            Debug.LogError($"Could not find {nameof(FurnitureObject)} in scene.");
        }
    }

    private void OnEnable()
    {
        m_FurnitureManager.RegisterStackable(this);
    }

    private void OnDisable()
    {
        m_FurnitureManager.UnregisterStackable(this);
    }

    public bool RaycastOnTop(Ray ray, out RaycastHit resultHitInfo)
    {
        if (m_Collider.Raycast(ray, out RaycastHit hitInfo, 100f))
        {
            resultHitInfo = hitInfo;
            if (Vector3.Dot(hitInfo.normal, Vector3.up) > 0.9f)
            {
                return true;
            }
        }

        resultHitInfo = default;
        return false;
    }
}
