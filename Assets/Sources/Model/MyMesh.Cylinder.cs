using UnityEngine;

public partial class MyMesh : MonoBehaviour
{
    private void BuildCylinderMesh(int N)
    {
        if (mMesh == null)
        {
            mMesh = GetComponent<MeshFilter>().mesh;
        }
        mMesh.Clear();

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

        int radialSegments = N;
        int heightSegments = N;

        int numVertices = (radialSegments + 1) * (heightSegments + 1);
        int numTriangles = radialSegments * heightSegments * 2;

        Vector3[] v = new Vector3[numVertices];
        Vector3[] n = new Vector3[numVertices];
        int[] t = new int[numTriangles * 3];

        float height = 2f;
        float radius = 1f;

        float dy = height / heightSegments;
        float yStart = -height * 0.5f;

        float angleStep = Mathf.PI * 2f / radialSegments;

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

        int tri = 0;
        for (int h = 0; h < heightSegments; h++)
        {
            for (int a = 0; a < radialSegments; a++)
            {
                int top_left     = h * (radialSegments + 1) + a;
                int top_right    = top_left + 1;
                int bottom_left  = top_left + (radialSegments + 1);
                int bottom_right = bottom_left + 1;

                t[tri++] = top_left;
                t[tri++] = bottom_left;
                t[tri++] = bottom_right;

                t[tri++] = top_left;
                t[tri++] = bottom_right;
                t[tri++] = top_right;
            }
        }

        mMesh.vertices = v;
        mMesh.triangles = t;
        mMesh.normals = n;

        InitControllers(v);
        InitNormals(v, n);
    }
}
