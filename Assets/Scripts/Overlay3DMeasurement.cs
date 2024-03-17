using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overlay3DMeasurement : MonoBehaviour
{
    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    private void Awake()
    {

    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    private void Start()
    {
        CombineMeshes();
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    private void Update()
    {

    }

    private void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Mesh finalMesh = new Mesh();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combine, true);

        MeshFilter newMeshFilter = gameObject.AddComponent<MeshFilter>();
        newMeshFilter.mesh = finalMesh;

        BoxCollider collider = gameObject.AddComponent<BoxCollider>();

        Bounds combinedBounds = finalMesh.bounds;
        Debug.Log("Final Mesh Bounding Box:");
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        Debug.Log($"Center: {localCenter.x} {localCenter.y} {localCenter.z}");
        Debug.Log($"Size: {combinedBounds.size.x} {combinedBounds.size.y} {combinedBounds.size.z}");

        collider.center = localCenter;
        collider.size = combinedBounds.size;
    }
}
