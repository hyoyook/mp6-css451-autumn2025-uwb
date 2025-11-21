using UnityEngine;

public partial class MyMesh : MonoBehaviour
{

    public GameObject SphereMarkerPrefab;       // non-selectable (black, no script)

    // init sphere controllers
    // Cylinder-specific controller init
    private void InitCylinderControllers(
    Vector3[] v,
    int heightSegments,
    int radialSegments,
    int selectableColumn)   // 0 == the first column of vertices
    {
        mControllers = new GameObject[v.Length];

        for (int y = 0; y <= heightSegments; y++)
        {
            for (int x = 0; x <= radialSegments; x++)
            {
                int idx = y * (radialSegments + 1) + x;

                // ONE COLUMN selectable: all vertices with this x index
                bool isSelectable = (x == selectableColumn);

                GameObject prefab = isSelectable
                    ? SphereControllerPrefab      // white, has SphereController
                    : SphereMarkerPrefab;         // black, no script / collider

                GameObject s = Instantiate(prefab, this.transform);
                s.name = isSelectable
                    ? $"SphereController_{idx}"
                    : $"SphereMarker_{idx}";

                int layerIdx = LayerMask.NameToLayer("SphereController");
                s.layer = layerIdx;

                s.SetActive(false);
                s.transform.localPosition = v[idx];
                mControllers[idx] = s;
            }
        }

    }
}
