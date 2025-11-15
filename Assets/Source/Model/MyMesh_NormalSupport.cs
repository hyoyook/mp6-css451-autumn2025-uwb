using UnityEngine;

public partial class MyMesh : MonoBehaviour
{
    private LineSegment[] mNormals;     // one per vertex
    public LineSegment NormalPrefab;
    private void InitNormals(Vector3[] v, Vector3[] n)
    {
        mNormals = new LineSegment[v.Length];
        for (int i = 0; i < v.Length; i++)
        {
            LineSegment o = Instantiate(NormalPrefab);
            o.name = $"Normal_{i}";

            mNormals[i] = o;
            mNormals[i].transform.SetParent(this.transform);
            
            Debug.Log($"[MyMesh] Created: {o.name}");
        }
        UpdateNormals(v, n);
    }

    void UpdateNormals(Vector3[] v, Vector3[] n)
    {
        for (int i = 0; i < v.Length; i++)
        {
            mNormals[i].SetEndPoints(v[i], v[i] + 1.0f * n[i]);
        }
    }
    Vector3 FaceNormal(Vector3[] v, int i0, int i1, int i2)
    {
        Vector3 a = v[i1] - v[i0];
        Vector3 b = v[i2] - v[i0];
        return Vector3.Cross(a, b).normalized;
    }

    void ComputeNormals(Vector3[] v, Vector3[] n)
    {
        // N-by-N quads → (N+1)-by-(N+1) vertices
        int N = Mathf.Max(1, Resolution);

        // loop over all quads, compute normals of both triangles
        for (int z = 0; z < N; z++)
        {
            for (int x = 0; x < N; x++)
            {
                int i0 = z * (N + 1) + x;        // top-left
                int i1 = i0 + 1;                 // top-right
                int i2 = i0 + (N + 1);           // bottom-left
                int i3 = i2 + 1;                 // bottom-right

                // Triangle 1: (i0, i2, i3)
                Vector3 n0 = FaceNormal(v, i0, i2, i3);
                n[i0] += n0;
                n[i2] += n0;
                n[i3] += n0;

                // Triangle 2: (i0, i3, i1)
                Vector3 n1 = FaceNormal(v, i0, i3, i1);
                n[i0] += n1;
                n[i3] += n1;
                n[i1] += n1;
            }
        }

        // normalize all
        for (int i = 0; i < n.Length; i++)
        {
            if (n[i].sqrMagnitude > 0f)
                n[i] = n[i].normalized;
            else
                n[i] = Vector3.up;
        }

        UpdateNormals(v, n);

    }

}
