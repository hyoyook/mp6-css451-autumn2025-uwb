using UnityEngine;

public class TexturePlacement : MonoBehaviour
{
    // translation
    public float UV_Translate_X = 0f;
    public float UV_Translate_Y = 0f;

    // rotation
    public float UV_Rotation = 0f;   // degrees

    // scale
    public float UV_Scale_X = 1f;
    public float UV_Scale_Y = 1f;

    private MyMesh mMyMesh;
    private Mesh mMesh;

    private void Awake()
    {
        mMyMesh = GetComponent<MyMesh>();
        mMesh   = GetComponent<MeshFilter>().mesh;
    }

    // LateUpdate so that it's called after MyMesh.Update()
    private void LateUpdate()
    {
        if (mMyMesh == null || mMesh == null )
        {
            return;
        }

        if (mMyMesh.IsCylinderMode())
        { 
            return; 
        }

        // base UV
        Vector2[] baseUV = mMyMesh.mInitUV;
        if (baseUV == null)
        {
            return;
        }

        Vector2 translation = new Vector2(UV_Translate_X, UV_Translate_Y);
        Vector2 scale       = new Vector2(UV_Scale_X, UV_Scale_Y);
        
        Matrix3x3 M = Matrix3x3Helpers.CreateTRS(translation, UV_Rotation, scale);

        // apply to all UVs
        Vector2[] newUV = new Vector2[baseUV.Length];

        for (int i = 0; i < baseUV.Length; i++)
        { 
            newUV[i] = M * baseUV[i]; 
        }

        // write back to mesh
        mMesh.uv = newUV;


    }

}
