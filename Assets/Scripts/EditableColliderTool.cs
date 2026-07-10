using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class EditableColliderTool : MonoBehaviour
{
    public List<Vector3> points = new List<Vector3>();
    public bool isClosedShape = true;
    public float height = 2f;
    public bool reverseTriangles = false;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
    }

    public void InsertPoint(int index, Vector3 point)
    {
        points.Insert(index, point);
    }

    public void RemovePoint(int index)
    {
        if (index >= 0 && index < points.Count)
            points.RemoveAt(index);
    }

    public void ClearShape()
    {
        points.Clear();

        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();

        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;
    }

    public void Bake()
    {
        if (points == null || points.Count < 2)
            return;

        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = GenerateWallMesh();
        mesh.name = "Baked Collider";

        meshFilter.sharedMesh = mesh;

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    private Mesh GenerateWallMesh()
    {
        Mesh mesh = new Mesh();

        int pointsCount = points.Count;
        Vector3 topOffset = new Vector3(0f, height, 0f);

        Vector3[] orderedPoints = points.ToArray();
        if (reverseTriangles)
            System.Array.Reverse(orderedPoints);

        Vector3[] vertices = new Vector3[pointsCount * 2];
        List<int> tris = new List<int>();

        for (int i = 0; i < pointsCount; i++)
        {
            int currentIndex = i * 2;
            int currentIndexTop = currentIndex + 1;

            Vector3 currentPoint = orderedPoints[i];
            Vector3 currentPointTop = currentPoint + topOffset;

            vertices[currentIndex] = currentPoint;
            vertices[currentIndexTop] = currentPointTop;

            if (isClosedShape || i < pointsCount - 1)
            {
                if (!isClosedShape && i == pointsCount - 1)
                    break;

                int nextIndex = ((i + 1) % pointsCount) * 2;
                int nextIndexTop = nextIndex + 1;

                tris.Add(currentIndexTop);
                tris.Add(nextIndex);
                tris.Add(currentIndex);

                tris.Add(currentIndexTop);
                tris.Add(nextIndexTop);
                tris.Add(nextIndex);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}