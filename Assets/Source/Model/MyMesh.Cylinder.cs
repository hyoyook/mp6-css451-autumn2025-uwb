using UnityEngine;

// Builds the cylinder mesh without top/bottom covers.
// Should be called from the mesh controller script
public partial class MyMesh : MonoBehaviour
{
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


        int radialSegments = N; // number of slices around y axis
        int heightSegments = M; // number of segments along the height


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

        // Adjust Cylinder rotation here
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
        // Other Triangle = TopL -> TopR -> BotR
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
        // Debug.Log($"UpdateCylinderVertexPosition called for index {index} to newPos {newPos}");
        Vector3[] vertices = mMesh.vertices;

        // need to caluate the radius by get the new radius from the center.
        Vector3 oldPosition = vertices[index];

        // Debug.Log($"Old position: {oldPosition}, old radius: {new Vector2(oldPosition.x, oldPosition.z).magnitude}");

        // Radius change
        float oldRadius = new Vector2(oldPosition.x, oldPosition.z).magnitude;
        float newRadius = new Vector2(newPos.x, newPos.z).magnitude;

        // Debug.Log($"new radius={newRadius}, radiusScale={newRadius / oldRadius}");

        if (oldRadius < 0.0001f)
        {
            return; // avoid division by zero
        }

        // Scale factor
        float radiusScale = newRadius / oldRadius;

        // find the height (row) of the vertex being moved 
        int radialSegments = currentCylinderN;
        int heightSegments = currentCylinderM;

        int height = index / (radialSegments + 1);
        // Debug.Log($"Vertex index {index} is at height row {height}");

        // Update all vertices in that height row
        for (int a = 0; a <= radialSegments; a++)
        {
            int idx = height * (radialSegments + 1) + a;

            if (idx >= vertices.Length)
            {
                // Debug.LogWarning($"Index {idx} out of bounds for vertices array of length {vertices.Length}");
                continue;
            }

            Vector3 pos = vertices[idx];

            // Scale x and z by radiusScale
            float newX = pos.x * radiusScale;
            float newZ = pos.z * radiusScale;

            vertices[idx] = new Vector3(newX, newPos.y, newZ);
        }

        mMesh.vertices = vertices;
        ComputeCylinderNormals(vertices, mMesh.normals, radialSegments, heightSegments);


        // Update controller sphere positions
        for (int a = 0; a <= radialSegments; a++)
        {
            int idx = height * (radialSegments + 1) + a;

            if (idx >= mControllers.Length)
            {
                // Debug.LogWarning($"Index {idx} out of bounds for controllers array of length {mControllers.Length}");
                continue;
            }

            Vector3 pos = vertices[idx];
            mControllers[idx].transform.localPosition = pos;
        }
    }


    // Added method to update cylinder rotation (because our previous method rebuilt the mesh)
    public void UpdateCylinderRotation(int rotation)
    {
        if (mMesh == null) return;

        Vector3[] vertices = mMesh.vertices;
        Vector3[] normals = mMesh.normals;

        int radialSegments = currentCylinderN;
        int heightSegments = currentCylinderM;

        float angleStep = (rotation * Mathf.Deg2Rad) / radialSegments;

        // Update vertices while preserving radius modifications
        for (int h = 0; h <= heightSegments; h++)
        {
            for (int a = 0; a <= radialSegments; a++)
            {
                int idx = h * (radialSegments + 1) + a;
                if (idx >= vertices.Length) continue;

                Vector3 currentPos = vertices[idx];

                // Calculate current radius (preserve any modifications)
                float currentRadius = new Vector2(currentPos.x, currentPos.z).magnitude;
                float currentY = currentPos.y; // Preserve Y modifications

                // Calculate new angle
                float angle = a * angleStep;
                float cosA = Mathf.Cos(angle);
                float sinA = Mathf.Sin(angle);

                // Apply new angle with preserved radius and Y
                vertices[idx] = new Vector3(currentRadius * cosA, currentY, currentRadius * sinA);
                normals[idx] = new Vector3(cosA, 0f, sinA);
            }
        }

        mMesh.vertices = vertices;
        mMesh.normals = normals;

        // Update controller sphere positions
        if (mControllers != null)
        {
            for (int i = 0; i < mControllers.Length && i < vertices.Length; i++)
            {
                if (mControllers[i] != null)
                {
                    mControllers[i].transform.localPosition = vertices[i];
                }
            }
        }

        // Update normal visualization
        UpdateNormals(vertices, normals);
    }
}
