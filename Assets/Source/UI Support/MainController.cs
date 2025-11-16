using UnityEngine;
using UnityEngine.EventSystems;

public class MainController : MonoBehaviour
{
    public GameObject AxisFramePrefab; // Assign in Inspector

    private GameObject mSelectedSphere = null;
    private GameObject mAxisManipulator = null;
    public MyMesh theMesh;

    public LayerMask sphereLayer; 

    void Update()
    {
        // Check for (Left-Control + LMB)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return; // Don't do anything else, the click was for the UI
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Add the 'out hit', a max distance (like 100f), AND the layer mask
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, sphereLayer))
            {
                // This will now ONLY hit objects on the layer(s) specified in 'sphereLayer'
                SphereController hitSphere = hit.collider.GetComponent<SphereController>();
                if (hitSphere != null)
                {
                    HandleSelection(hitSphere.gameObject);
                }
            }
        }

    }

    void HandleSelection(GameObject newSphere)
    {
        // If we clicked the same sphere, just stop.
        if (mSelectedSphere == newSphere)
        {
            return;
        }

        //Debug.Log("HandleSelection CALLED on: " + newSphere.name);

        // 1. Deselect the old one (if it exists)
        if (mSelectedSphere != null)
        {
            mSelectedSphere.GetComponent<SphereController>().Deselect();
        }

        // 2. Select the new one
        mSelectedSphere = newSphere;
        mSelectedSphere.GetComponent<SphereController>().Select();

        // 3. Spawn or move the manipulator
        if (mAxisManipulator == null)
        {
            mAxisManipulator = Instantiate(AxisFramePrefab);
        }
        mAxisManipulator.transform.position = mSelectedSphere.transform.position;
    }

    /*public void OnShapeDropdownChanged(int index)
    {
        if (index == 0) // 0 = Plane
        {
            theMesh.BuildMesh(theMesh.Resolution);
        }
        else if (index == 1) // 1 = Cylinder
        {
            // Get values from your other sliders
            int cylResolution = ... ;
            int cylSegments = ... ;
            float cylSweep = ... ;
            theMesh.BuildCylinderMesh(cylResolution, cylSegments, cylSweep);
        }
    }*/
}
