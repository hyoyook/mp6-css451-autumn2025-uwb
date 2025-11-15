using UnityEngine;

public partial class MyMesh : MonoBehaviour
{
    Mesh mMesh;

    // Resolution Range (2, 20)
    public int Resolution = 4; // random default -> changed by user (slider)

    // Prefabs
    public GameObject AxisFramePrefab;
    
    private void Start()
    {
        Mesh theMesh = GetComponent<MeshFilter>().mesh; // get the mesh component
        BuildMesh(Resolution);
    }
    void Update()
    {

        Vector3[] v = mMesh.vertices;          // get curr vertices
        Vector3[] n = new Vector3[v.Length];   // for recomputed normals

        // each vertex follows its controller
        for (int i = 0; i < mControllers.Length; i++)
        {
            if (mControllers[i] == null)
            {
                continue;
            }

            // pos of vertex i = localPosition of its sphere controller
            v[i] = mControllers[i].transform.localPosition;
        }

        // recompute normals
        ComputeNormals(v, n);

        // write back to the mesh
        mMesh.vertices = v;
        mMesh.normals = n;
    }

    // build an N x N grid mesh, init controller and normals
    private void BuildMesh(int N) 
    {
        if (mMesh == null) 
        {
            mMesh = GetComponent<MeshFilter>().mesh;
            
        }
        mMesh.Clear();  // delete whatever is there!!


        // clean up old controlelrs and normals
        if (mControllers != null)
        {
            for (int i = 0; i < mControllers.Length; i++)
            {
                if (mControllers[i] != null)
                {
                    Destroy(mControllers[i]);
                }
            }
        }

        if (mNormals != null)
        {
            for (int i = 0; i < mNormals.Length; i++)
            {
                if (mNormals[i] != null)
                {
                    Destroy(mNormals[i].gameObject);
                }
            }
        }

        int numVertices = (N + 1) * (N + 1);
        int numTriangles = N * N * 2;

        // vertices and normals
        Vector3[] v = new Vector3[numVertices];
        Vector3[] n = new Vector3[numVertices];
        int[] t = new int[numTriangles * 3];

        // 2x2 plane on XZ plane at (0, 0)
        float size = 2f;
        float gridSpace = size / N;     // if N ==4, gridSpace = 2/4 = 0.5f, N == 20, gridSpace = 0.1f
        /* every vertex in the mesh's pos 
         *  x = start + (xIndex * gridSpace)
            z = start + (zIndex * gridSpace)
        -> the plane always stays the same size, only the resolution changes
         */
        float start = -size * 0.5f; // -1 : -1 <= x, y <= 1

        // vertices
        int idx = 0;
        for (int z = 0; z <= N; z++) 
        {
            for (int x = 0; x <= N; x++) 
            {
                v[idx] = new Vector3(start + x * gridSpace, 0f, start + z * gridSpace);
                n[idx] = Vector3.up; // always (0, 1, 0)
                idx++;
            }
        }
        
        // triangles (each quad -> 2 triangles)
        int tri = 0;

        for (int z = 0; z < N; z++) 
        {
            for (int x = 0; x < N; x++)
            {
                int top_left  = z * (N + 1) + x;
                int top_right = top_left + 1;
                int btm_left  = top_left + (N + 1);
                int btm_right = btm_left + 1;
                
                // triangle 1 ( top_left, btm_left, btm_right)
                t[tri++] = top_left; 
                t[tri++] = btm_left;
                t[tri++] = btm_right;

                // triangle 2 ( top_left, btm_right, top_right)
                t[tri++] = top_left;
                t[tri++] = btm_right;
                t[tri++] = top_right;

            }
        }

        // Assign to mesh
        mMesh.vertices = v;
        mMesh.triangles = t;
        mMesh.normals = n;

        // Initialize controllers and normals
        InitControllers(v);
        InitNormals(v, n);

    }

}