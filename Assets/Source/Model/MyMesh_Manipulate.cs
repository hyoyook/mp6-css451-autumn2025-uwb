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

            mControllers[i] = s;

            // Debug.Log($"[MyMesh] Created: {s.name}");
        }
    }
}
