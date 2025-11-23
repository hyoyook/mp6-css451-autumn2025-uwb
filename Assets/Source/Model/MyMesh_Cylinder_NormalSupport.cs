/// ---------------------------------------------------------------------------------
/// MyMesh_Cylinder_NormalSupport.cs
/// Author: Alec Situ, Julia Nguyen, Hyobin Yook (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// Compute per-vertex normals for the cylinder mesh
/// ---------------------------------------------------------------------------------

using UnityEngine;
public partial class MyMesh : MonoBehaviour
{
    // Track current cylinder dimensions for normal computation
    private int currentCylinderN;
    private int currentCylinderM;
    private bool isCylinderMode = false;

    // Store cylinder normals for recalculation
    private void ComputeCylinderNormals(Vector3[] v, Vector3[] n, int radialSegments, int heightSegments)
    {
        // Debug.Log($"ComputeCylinderNormals called with radialSegments={radialSegments}, heightSegments={heightSegments}, total vertices={v.Length}");

        // Clear all vertex normals
        for (int i = 0; i < n.Length; i++)
        {
            n[i] = Vector3.zero;
        }

        // calculate normals by averaging triangle normals at each vertex
        int triCount = 0;
        Vector3 firstTriangleNormal = Vector3.zero;
        
        for (int h = 0; h < heightSegments; h++)
        {
            for (int a = 0; a < radialSegments; a++)
            {
                int top_left = h * (radialSegments + 1) + a;
                int top_right = top_left + 1;
                int bottom_left = top_left + (radialSegments + 1);
                int bottom_right = bottom_left + 1;

                // Triangle 1: top_left, bottom_left, bottom_right
                // Vector3 v0 = v[top_left];
                // Vector3 v1 = v[bottom_left];
                // Vector3 v2 = v[bottom_right];

                Vector3 n0 = FaceNormal(v, top_left, bottom_left, bottom_right);
                // Vector3 edge1 = v1 - v0;
                // Vector3 edge2 = v2 - v0;
                // Vector3 triangleNormal = Vector3.Cross(edge1, edge2).normalized;
                n[top_left] += n0;
                n[bottom_left] += n0;
                n[bottom_right] += n0;
                
                if (triCount == 0)
                {
                    firstTriangleNormal = n0;
                    // Debug.Log($"First Triangle Normal: {firstTriangleNormal}");
                }
                
                // n[top_left] += triangleNormal;
                // n[bottom_left] += triangleNormal;
                // n[bottom_right] += triangleNormal;

                // Triangle 2: top_left, bottom_right, top_right
                // v0 = v[top_left];
                // v1 = v[bottom_right];
                // v2 = v[top_right];
                // edge1 = v1 - v0;
                // edge2 = v2 - v0;
                // triangleNormal = Vector3.Cross(edge1, edge2).normalized;

                Vector3 n1 = FaceNormal(v, top_left, bottom_right, top_right);
                n[top_left] += n1;
                n[bottom_right] += n1;
                n[top_right] += n1;

                triCount++;
            }
        }
        
        // Debug.Log($"Total triangles processed: {triCount}");

        // Normalize all vertex normals
        for (int i = 0; i < n.Length; i++)
        {
            if (n[i].sqrMagnitude > 0f)
                n[i] = n[i].normalized;
            else
                n[i] = Vector3.up;

            // Debug: Check if normal is pointing mostly along Y-axis (up/down)
            // this tells me if something's wrong with the normal calculation
            if (Mathf.Abs(n[i].y) > 0.9f && Mathf.Abs(n[i].x) < 0.1f && Mathf.Abs(n[i].z) < 0.1f)
            {
                Debug.LogWarning($"Vertex {i}: Normal points along Y-axis! Normal = {n[i]}, Position = {v[i]}");
            }
        }

        UpdateNormals(v, n);
    }

    // Call this from BuildCylinderMesh to store current dimensions
    public void SetCylinderDimensions(int N, int M)
    {
        currentCylinderN = N;
        currentCylinderM = M;
        isCylinderMode = true;
    }

    // Check if we're in cylinder mode
    public bool IsCylinderMode()
    {
        return isCylinderMode;
    }


    public void CylinderModeOff()
    {
        isCylinderMode = false;

    }

    public void CylinderModeOn()
    {
        isCylinderMode = true;
    }

}

