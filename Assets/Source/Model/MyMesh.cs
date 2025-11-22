/*
 * TODO: REFERENCES
 * Author:  Dr. Kelvin Sung
 * Editors: 
 * Created for CSS451, edited for MP6
 */
using UnityEngine;
public partial class MyMesh : MonoBehaviour
{
    Mesh mMesh;

    // Resolution Range (2, 20)
    // Support N by M resolution control
    public int columns = 4;
    public int rows    = 4;
    
    // Track current grid dimensions for normal computation
    private int currentColumns;
    private int currentRows;

    // UV array for texture
    public Vector2[] mInitUV = null;
    public Material planeMaterial;
    public Material cylinderMaterial;
    private MeshRenderer mMeshRenderer;

    private void Start()
    {
        Mesh theMesh = GetComponent<MeshFilter>().mesh; // get the mesh component
        mMeshRenderer = GetComponent<MeshRenderer>();
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

        // recompute normals based on mesh type
        if (IsCylinderMode())
        {
            // Debug.Log($"Update(): Recomputing cylinder normals with N={currentCylinderN}, M={currentCylinderM}");
            ComputeCylinderNormals(v, n, currentCylinderN, currentCylinderM);
        }
        else
        {
            // Debug.Log($"Update(): Recomputing plane normals with Columns={currentColumns}, Rows={currentRows}");
            // Debug.Log("IsCylinderMode? " + IsCylinderMode());
            ComputeNormals(v, n, currentColumns, currentRows);
        }

        // write back to the mesh
        mMesh.vertices = v;
        mMesh.normals  = n;
    }

    public void UpdateVertexPosition(int index, Vector3 newLocalPos)
    {
        Vector3[] v = mMesh.vertices;
        v[index] = newLocalPos;
        mMesh.vertices = v;

        // We also need to re-compute normals now
        Vector3[] n = new Vector3[v.Length];
        ComputeNormals(v, n, currentColumns, currentRows);
        mMesh.normals = n;
    }

    public void SetVisualizationActive(bool active)
    {
        
        // Safety check: only run if they exist
        if (mNormals == null || mControllers == null) return;
        
        // Toggle the spikes (Normals)
        for (int i = 0; i < mNormals.Length; i++)
        {
            if (mNormals[i] != null)
            {
                mNormals[i].gameObject.SetActive(active);
            }
        }

        // Toggle the spheres (Controllers)
        for (int i = 0; i < mControllers.Length; i++)
        {
            if (mControllers[i] != null)
            {
                mControllers[i].SetActive(active);
            }
        }

        if (!active){
            GameObject axisManipulator = GameObject.Find("AxisManipulator");
            GameObject.Destroy(axisManipulator);
        }
    }

#region build plane mesh
    public void Build_Plane_Mesh(int N, int M)
    {
        BuildMesh(N, M);
        mMeshRenderer.material = planeMaterial;
    }

    // build an N x N grid mesh, init controller and normals
    private void BuildMesh(int col, int row) 
    {
        // resolution range (2, 20)
        col = Mathf.Clamp(col, 2, 20);
        row = Mathf.Clamp(row, 2, 20);
        
        // Store current dimensions for normal computation
        currentColumns  = col;
        currentRows     = row;

        isCylinderMode = false;

        if (mMesh == null) 
        {
            mMesh = GetComponent<MeshFilter>().mesh;  
        }

        mMesh.Clear();  // delete whatever is there

        // clean up old controllers and normals
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

        int numVertices  = (col + 1) * (row + 1);
        int numTriangles = col * row * 2;

        // vertices, normals, uvs, triangles
        Vector3[] v  = new Vector3[numVertices];
        Vector3[] n  = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[]     t  = new int[numTriangles * 3];

        // 2x2 plane on XZ plane at (0, 0)
        float sizeX = 2f;
        float sizeZ = 2f;

        float gridSpaceX = sizeX / col;
        float gridSpaceZ = sizeZ / row;

        /* every vertex in the mesh's pos 
         *  x = start + (xIndex * gridSpace)
         *  z = start + (zIndex * gridSpace)
         *  -> the plane always stays the same size, only the resolution changes
         */

        float startX = -sizeX * 0.5f; // -1 : -1 <= x, z <= 1
        float startZ = -sizeZ * 0.5f;        

        // vertices and UVs
        int idx = 0;
        for (int r = 0; r <= row; r++) 
        {
            float v_coord = r / (float) row;            // 0 <= v_coord <= 1 (vertical)
            for (int c = 0; c <= col; c++) 
            {
                float x = startX + c * gridSpaceX;
                float z = startZ + r * gridSpaceZ;

                v[idx] = new Vector3(x, 0f, z);
                n[idx] = Vector3.up;            // always (0, 1, 0)

                float u_coord = c / (float) col;        // 0 <= u_coord <= 1 (horizontal)
                uv[idx] = new Vector2(u_coord, v_coord);
                
                idx++;
            }
        }
        
        // triangles (each quad -> 2 triangles)
        int tri = 0;
        for (int r = 0; r < row; r++) 
        {
            for (int c = 0; c < col; c++)
            {
                int i0 = r * (col + 1) + c; // top-left
                int i1 = i0 + 1;            // top-right
                int i2 = i0 + (col + 1);    // bottom-left
                int i3 = i2 + 1;            // bottom-right

                // triangle 1 (top_left, btm_left, btm_right)
                t[tri++] = i0; 
                t[tri++] = i2;
                t[tri++] = i3;

                // triangle 2 (top_left, btm_right, top_right)
                t[tri++] = i0;
                t[tri++] = i3;
                t[tri++] = i1;
            }
        }

        // assign to mesh
        mMesh.vertices  = v;
        mMesh.triangles = t;
        mMesh.normals   = n;
        mMesh.uv = uv;

        //// store original plannar uv for TRS <-------------------- (Hyob) note to myself: do i need this?
        mInitUV = new Vector2[uv.Length];
        System.Array.Copy(uv, mInitUV, uv.Length);

        // initialize controllers and normals
        InitControllers(v);
        InitNormals(v, n);
    }
    #endregion
}