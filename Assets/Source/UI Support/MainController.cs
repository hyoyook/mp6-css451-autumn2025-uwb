using UnityEngine;
using UnityEngine.EventSystems;

public partial class MainController : MonoBehaviour
{
    public GameObject AxisFramePrefab;
    public MyMesh theMesh;
    public LayerMask sphereLayer;
    public LayerMask manipulatorLayer; //Set to the layer your manipulator axes are on

    private GameObject mSelectedSphere = null;
    private GameObject mAxisManipulator = null;


    // --- Variables for dragging ---
    private AxisController.Axis mSelectedAxis; // X, Y, or Z
    private bool mIsDragging = false;
    private Vector3 mDragStartPosition;
    private Vector3 mDragStartSpherePosition;

    
    private bool mControlWasDown = false; 
    private bool mSphereIsSelected = false;

    void Update()
    {
        bool desiredVisualState = Input.GetKey(KeyCode.LeftControl) || mSphereIsSelected;

        if (desiredVisualState != mControlWasDown)
        {
            // Only call the function if the state has changed
            theMesh.SetVisualizationActive(desiredVisualState);
            mControlWasDown = desiredVisualState;
        }

        // 1. Check for selection
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("MainController: Control + Click detected for selection.");
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            
            // Try to select a SPHERE
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, sphereLayer))
            {
                Debug.Log("MainController: Sphere hit for selection.");
                SphereController hitSphere = hit.collider.GetComponent<SphereController>();
                if (hitSphere != null)
                {
                    HandleSelection(hitSphere.gameObject);
                }
            }
            
        }
        // 2. Check for drag START (on an axis)
        else if (Input.GetMouseButtonDown(0) && mSelectedSphere != null)
        {
            Debug.Log("MainController: Mouse Down detected for dragging.");
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            // Try to hit a MANIPULATOR axis
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, manipulatorLayer)) // Use manipulator layer
            {
                Debug.Log("MainController: Manipulator axis hit for dragging.");
                AxisController axis = hit.collider.GetComponent<AxisController>();
                if (axis != null)
                {
                    // Start the drag!
                    mIsDragging = true;
                    mSelectedAxis = axis.axisDirection;
                    mDragStartPosition = Input.mousePosition; // Store screen position
                    mDragStartSpherePosition = mSelectedSphere.transform.position; // Store world position
                }
            }
        }
        // 3. Check for drag END
        else if (Input.GetMouseButtonUp(0))
        {
            mIsDragging = false;
        }

        // 4. If DRAGGING, process movement
        if (mIsDragging)
        {
            ProcessDrag();
        }

    }

    void ProcessDrag()
    {
        Vector3 mouseDelta = Input.mousePosition - mDragStartPosition;
        
        float dragSpeed = 0.01f; 
        
        Vector3 worldOffset = Vector3.zero;

        switch (mSelectedAxis)
        {
            case AxisController.Axis.X:
                worldOffset.x = mouseDelta.x * dragSpeed;
                break;
            case AxisController.Axis.Y:
                worldOffset.y = mouseDelta.y * dragSpeed;
                break;
            case AxisController.Axis.Z:
                worldOffset.z = mouseDelta.y * dragSpeed; 
                break;
        }

        // Apply the new position
        Vector3 newPos = mDragStartSpherePosition + worldOffset;
        mSelectedSphere.transform.position = newPos; // Move the sphere
        mAxisManipulator.transform.position = newPos; // Move the manipulator with it

        // Must tell MyMesh to update its data
        UpdateMeshVertex();
    }

    void HandleSelection(GameObject newSphere)
    {
        if (mSelectedSphere != newSphere)
        {
            if (mSelectedSphere != null)
            {
                mSelectedSphere.GetComponent<SphereController>().Deselect();
                mSphereIsSelected = false;

            }

            mSelectedSphere = newSphere;
            mSelectedSphere.GetComponent<SphereController>().Select();

            // Make sure the spikes viz know to stay on
            mSphereIsSelected = true;

            if (mAxisManipulator == null)
            {
                mAxisManipulator = Instantiate(AxisFramePrefab);
                mAxisManipulator.name = "AxisManipulator";
                mAxisManipulator.layer = LayerMask.NameToLayer("AxisManipulator");
            }
            mAxisManipulator.transform.position = mSelectedSphere.transform.position;
        }
    }

    void UpdateMeshVertex()
    {
        if (theMesh == null || mSelectedSphere == null) return;
        
        // Get the index of the sphere (we stored it in the name "SphereController_i")
        string[] nameParts = mSelectedSphere.name.Split('_');
        if (nameParts.Length == 2)
        {
            int index = int.Parse(nameParts[1]);
            
            // Get the sphere's LOCAL position relative to the mesh
            Vector3 localPos = theMesh.transform.InverseTransformPoint(mSelectedSphere.transform.position);

            // Tell the mesh to update!
            theMesh.UpdateVertexPosition(index, localPos);
        }
    }

    /*
    get called by the UI drop down
    *** NEED TO UPDATE THIS ***
    public void OnShapeDropdownChanged(int index)
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
    }
    
    */
}
