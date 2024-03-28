using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSelectionVisualizer : MonoBehaviour
{
    BoxCollider m_Collider;

    LineRenderer m_LineRenderer;

    FurnitureObject m_FurnitureObject;

    protected void Awake()
    {

        m_LineRenderer = GetComponent<LineRenderer>();
        if (m_LineRenderer == null)
        {
            Debug.LogError($"Could not find component {nameof(BoxCollider)} in current GameObject");
            return;
        }

        // m_FurnitureObject = transform.parent.gameObject.GetComponent<FurnitureObject>();
        // if (m_FurnitureObject == null)
        // {
        //     Debug.LogError($"Could not find component {nameof(FurnitureObject)} in parent GameObject");
        //     return;
        // }
    }

    public void Initialize(BoxCollider boxCollider)
    {
        m_Collider = boxCollider;

        m_LineRenderer.startWidth = 0.02f;
        m_LineRenderer.endWidth = 0.02f;
        m_LineRenderer.positionCount = 4;
        m_LineRenderer.loop = true;

        Vector3[] positions = new Vector3[4];
        Vector3 size = (m_Collider.size / 2f) + Vector3.one * 0.1f;
        Vector3 center = Vector3.zero;

        positions[0] = center + new Vector3(-size.x, 0f, -size.z);
        positions[1] = center + new Vector3(size.x, 0f, -size.z);
        positions[2] = center + new Vector3(size.x, 0f, size.z);
        positions[3] = center + new Vector3(-size.x, 0f, size.z);

        m_LineRenderer.SetPositions(positions);
    }
}
