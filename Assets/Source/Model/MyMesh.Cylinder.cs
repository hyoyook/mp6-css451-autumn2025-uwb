/// ---------------------------------------------------------------------------------
/// MyMesh_Cylinder.cs
/// Author: Alec Situ, Julia Nguyen, Hyobin Yook (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// Builds and updates cylinder mesh. 
/// Supports UI-triggered rebuild of rotation and resolution changes.
/// 
/// Written with help of AI
/// ---------------------------------------------------------------------------------

using UnityEngine;

// Builds the cylinder mesh without top/bottom covers.
// Should be called from the mesh controller script
public partial class MyMesh : MonoBehaviour
{

    [SerializeField] private float minRadius = 0.001f;
    public void Build_Cylinder_Mesh(int N, int M, int rotation)
    {
        SetCylinderDimensions(N, M);
        BuildCylinderMesh(N, M, rotation);
        mMeshRenderer.material = cylinderMaterial;
    }
    private void BuildCylinderMesh(int N, int M, int rotation)
    {

        // Make sure we have a mesh and clear out the old mesh
        if (mMesh == null)
        {
            mMesh = GetComponent<MeshFilter>().mesh;
        }
        mMesh.Clear(); // Removes prev vertecies/triangles/normals/uv all the things.

        // Clean up old controllers and normals
        if (mControllers != null)
        {
            for (int i = 0; i < mControllers.Length; i++)
            {
                if (mControllers[i] != null)
                    Destroy(mControllers[i]);
            }
        }

        if (mNormals != null)
        {
            for (int i = 0; i < mNormals.Length; i++)
            {
                if (mNormals[i] != null)
                    Destroy(mNormals[i].gameObject);
            }
        }


        int radialSegments = N; // number of slices/faces around y axis
        int heightSegments = M; // number of faces needed to build the height


        int numVertices = (radialSegments + 1) * (heightSegments + 1);
        int numTriangles = radialSegments * heightSegments * 2;

        Vector3[] v = new Vector3[numVertices]; // Vertex positions
        Vector3[] n = new Vector3[numVertices]; // vertex normals
        int[] t = new int[numTriangles * 3]; // triangle indicies


        // Modify height/radius here
        float height = 2f;
        float radius = 1f;

        float dy = height / heightSegments;
        float yStart = -height * 0.5f;

        // Cylinder rotation here
        // float angleStep = Mathf.PI * 1f / radialSegments;
        float angleStep = (rotation * Mathf.Deg2Rad) / radialSegments;

        // For each height row h: 
        //      calculate y coord
        // For each radial column a: 
        //      calculate angle theta
        //      x = radius * cos(theta)
        //      z = radius * sin(theta)
        //      outward normal = (cos(theta), 0, sin(theta))
        // 
        int idx = 0;
        for (int h = 0; h <= heightSegments; h++)
        {
            float y = yStart + h * dy;

            for (int a = 0; a <= radialSegments; a++)
            {
                float angle = a * angleStep;

                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);

                float x = radius * cosA;
                float z = radius * sinA;

                v[idx] = new Vector3(x, y, z);
                n[idx] = new Vector3(cosA, 0f, sinA);  // outward

                idx++;
            }
        }


        // Create triangle indicies
        // One triangle = TopL -> BotL -> BotR
        // Second Triangle = TopL -> TopR -> BotR
        int tri = 0;
        for (int h = 0; h < heightSegments; h++)
        {
            for (int a = 0; a < radialSegments; a++)
            {
                int top_left = h * (radialSegments + 1) + a;
                int top_right = top_left + 1;
                int bottom_left = top_left + (radialSegments + 1);
                int bottom_right = bottom_left + 1;

                t[tri++] = top_left;
                t[tri++] = bottom_left;
                t[tri++] = bottom_right;

                t[tri++] = top_left;
                t[tri++] = bottom_right;
                t[tri++] = top_right;
            }
        }


        // Reassign to mesh
        mMesh.vertices = v;
        mMesh.triangles = t;
        mMesh.normals = n;

        // recreate controller balls and normal lines
        // int heightSegments = mHeightRes;      // whatever you call it
        // int radialSegments = mCylinderRes;    // your “Cylinder Res.” slider
        int selectableRow = heightSegments / 2;
        InitCylinderControllers(v, heightSegments, radialSegments, 0);
        InitNormals(v, n);
    }


    public void UpdateCylinderVertexPosition(int index, Vector3 newPos)
    {
        if (mMesh == null) return;                     // Mesh not initialized
        Vector3[] vertices = mMesh.vertices;
        Vector3[] normals = mMesh.normals;
        if (index < 0 || index >= vertices.Length) return;  // Invalid index

        // Existing vertex state
        Vector3 oldPos = vertices[index];
        float oldRadius = new Vector2(oldPos.x, oldPos.z).magnitude;
        float newRadius = new Vector2(newPos.x, newPos.z).magnitude;

        // Decide ring scaling:
        // 1. oldRadius must be non-zero (avoid divide by zero)
        // 2. newRadius must be >= minRadius (prevent collapse)
        // 3. Change magnitude must exceed small threshold (fixes weird jitter)
        const float radiusChangeThreshold = 0.001f;
        bool scaleRing =
            oldRadius > 1e-4f &&
            newRadius >= minRadius &&
            Mathf.Abs(newRadius - oldRadius) > radiusChangeThreshold;

        int radialSegments = currentCylinderN;
        int heightSegments = currentCylinderM;

        if (scaleRing)
        {
            Debug.Log("Ring scale move");
            // Uniform ring scaling: compute row index (height level)
            int height = index / (radialSegments + 1);
            float radiusScale = newRadius / oldRadius; // Scale factor for XZ

            for (int a = 0; a <= radialSegments; a++)
            {
                int idx = height * (radialSegments + 1) + a;
                if (idx >= vertices.Length) continue;

                // Scale existing vertex direction; set Y to newPos.y
                Vector3 p = vertices[idx];
                float newX = p.x * radiusScale;
                float newZ = p.z * radiusScale;
                vertices[idx] = new Vector3(newX, newPos.y, newZ);

                // Normal: outward along XZ; fallback if near axis
                Vector2 dir = new Vector2(newX, newZ);
                normals[idx] = dir.sqrMagnitude < 1e-8f
                    ? Vector3.up
                    : new Vector3(dir.normalized.x, 0f, dir.normalized.y);
            }

            // apply mesh changes; recompute normals
            mMesh.vertices = vertices;
            ComputeCylinderNormals(vertices, normals, radialSegments, heightSegments);
            mMesh.normals = normals;

            // Sync controller spheres for entire ring
            if (mControllers != null)
            {
                for (int a = 0; a <= radialSegments; a++)
                {
                    int idx = height * (radialSegments + 1) + a;
                    if (idx < mControllers.Length && mControllers[idx] != null)
                        mControllers[idx].transform.localPosition = vertices[idx];
                }
            }
        }
        else
        {
            Debug.Log("Single vertex move");
            // Local vertex move only (allows crossing center). Does not alter neighbors.
            vertices[index] = newPos;

            // Normal: outward from axis; if at axis choose arbitrary up to avoid zero normal
            Vector2 r = new Vector2(newPos.x, newPos.z);
            normals[index] = r.sqrMagnitude < 1e-8f
                ? Vector3.up
                : new Vector3(r.normalized.x, 0f, r.normalized.y);

            // apply single-vertex change
            mMesh.vertices = vertices;
            mMesh.normals = normals;

            // Update only this controller sphere
            if (mControllers != null && index < mControllers.Length && mControllers[index] != null)
                mControllers[index].transform.localPosition = newPos;
        }
    }

    public void UpdateCylinderRotation(int rotation)
    {
        if (mMesh == null) return;

        Vector3[] vertices = mMesh.vertices;
        Vector3[] normals = mMesh.normals;

        int radialSegments = currentCylinderN;
        int heightSegments = currentCylinderM;

        float angleStep = (rotation * Mathf.Deg2Rad) / radialSegments;

        for (int h = 0; h <= heightSegments; h++)
        {
            for (int a = 0; a <= radialSegments; a++)
            {
                int idx = h * (radialSegments + 1) + a;
                if (idx >= vertices.Length) continue;

                Vector3 currentPos = vertices[idx];
                float currentY = currentPos.y; // Preserve Y

                // Calculate new "expected" direction at this angle
                float angle = a * angleStep;
                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);
                Vector2 expectedDir = new Vector2(cosA, sinA);

                // Current XZ position
                Vector2 currentXZ = new Vector2(currentPos.x, currentPos.z);
                float currentRadius = currentXZ.magnitude;

                // Determine if vertex is flipped: dot product with expected direction
                float dot = Vector2.Dot(currentXZ.normalized, expectedDir);
                float sign = (currentRadius < 1e-5f) ? 1f : Mathf.Sign(dot); // if at center, default to positive

                // Apply signed radius in new direction
                float signedRadius = currentRadius * sign;
                vertices[idx] = new Vector3(signedRadius * cosA, currentY, signedRadius * sinA);

                // Normal: always outward from actual position (recompute after to handle flips correctly)
                Vector2 finalXZ = new Vector2(vertices[idx].x, vertices[idx].z);
                normals[idx] = finalXZ.sqrMagnitude < 1e-8f
                    ? Vector3.up
                    : new Vector3(finalXZ.normalized.x, 0f, finalXZ.normalized.y);
            }
        }

        mMesh.vertices = vertices;
        mMesh.normals = normals;

        // Update controller spheres
        if (mControllers != null)
        {
            for (int i = 0; i < mControllers.Length && i < vertices.Length; i++)
            {
                if (mControllers[i] != null)
                    mControllers[i].transform.localPosition = vertices[i];
            }
        }

        UpdateNormals(vertices, normals);
    }
}
