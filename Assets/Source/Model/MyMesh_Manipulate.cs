/// ---------------------------------------------------------------------------------
/// MyMesh_Manipulate.cs
/// Author: Julia Nguyen, Alec Situ, Hyobin Yook (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Edited for MP6, CSS451, UWB. 
/// 
/// Manage a sphere controller for each vertex
/// ---------------------------------------------------------------------------------

using UnityEngine;

public partial class MyMesh : MonoBehaviour
{

    private GameObject[] mControllers;  // one per vertex
    public GameObject SphereControllerPrefab;
    
    // init sphere controllers
    private void InitControllers(Vector3[] v)
    {
        mControllers = new GameObject[v.Length];

        for (int i = 0; i < v.Length; i++)
        {
            GameObject s = Instantiate(SphereControllerPrefab);
            s.name = $"SphereController_{i}";
            s.transform.SetParent(this.transform);          // Parent under this mesh
            s.transform.localPosition = v[i];               // Position = vertex local position
            
            int layerIdx = LayerMask.NameToLayer("SphereController");
            s.layer = layerIdx;
            mControllers[i] = s;

            // Debug.Log($"[MyMesh] Created: {s.name}");
            s.SetActive(false);
        }
    }
}
