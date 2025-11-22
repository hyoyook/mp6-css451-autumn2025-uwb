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

    private AxisController mCurrentAxisController = null; // Tracks which axis cylinder is being dragged

    // --- Variables for dragging ---
    private AxisController.Axis mSelectedAxis; // X, Y, or Z
    private bool mIsDragging = false;
    private Vector3 mDragStartPosition;
    private Vector3 mDragStartSpherePosition;

    // --- State Variables ---
    private bool mControlWasDown = false;
    private bool mVisualsAreActive = false; // Tracks the current state of normals/spheres visualization

    // --- variables for Texture Control (kept for structure)
    public TexturePlacement TexturePlacement;
    public XfromControl UV_XformControl;

    // Initialize state
    void Start()
    {
        // Ensure the visuals are off at the start
        if (theMesh != null)
        {
            theMesh.SetVisualizationActive(false);
            mVisualsAreActive = false;
        }
    }

    void Update()
    {
        // 0. Toggle Visualization (Spikes/Spheres)
        // Visuals are ON if Control is held OR if a sphere is selected.
        bool desiredVisualState = Input.GetKey(KeyCode.LeftControl) || (mSelectedSphere != null);

        if (desiredVisualState != mVisualsAreActive)
        {
            // Only call the function if the visibility state has changed
            theMesh.SetVisualizationActive(desiredVisualState);
            mVisualsAreActive = desiredVisualState;
        }

        // 1. Check for Selection (Ctrl + LMB Down)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Try to select a SPHERE
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, sphereLayer))
            {
                SphereController hitSphere = hit.collider.GetComponent<SphereController>();
                if (hitSphere != null)
                {
                    HandleSelection(hitSphere.gameObject);
                }
            }

        }

        // 2. Check for drag START (on an axis)
        // Note: This must be separate IF, not ELSE IF, to allow drag right after selection.
        if (Input.GetMouseButtonDown(0) && mSelectedSphere != null)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // Try to hit a MANIPULATOR axis
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hitAxis = Physics.Raycast(ray, out RaycastHit hit, 1000f, manipulatorLayer);
            
            if (hitAxis)
            {
                AxisController axis = hit.collider.GetComponent<AxisController>();
                if (axis != null)
                {
                    // --- START DRAG LOGIC ---
                    
                    // NEW: Highlight the selected axis
                    mCurrentAxisController = axis;
                    mCurrentAxisController.Highlight(true); 

                    // Initialize drag state
                    mIsDragging = true;
                    mSelectedAxis = axis.axisDirection;
                    mDragStartPosition = Input.mousePosition; 
                    mDragStartSpherePosition = mSelectedSphere.transform.position; 
                }
            }
        }

        // 3. Check for drag END
        if (Input.GetMouseButtonUp(0))
        {
            // NEW: Restore the color of the previously selected axis
            if (mCurrentAxisController != null)
            {
                mCurrentAxisController.Highlight(false);
                mCurrentAxisController = null;
            }
            
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
        if (mSelectedSphere == null) return;

        // Calculate the total mouse movement from the moment the drag started
        Vector3 mouseTotalDelta = Input.mousePosition - mDragStartPosition;

        // Use fixed speeds for better responsiveness
        float xyDragSpeed = 0.01f;
        float zDragSpeed = 0.05f;

        Vector3 manipulatorRight = mAxisManipulator.transform.right; 
        Vector3 manipulatorUp = mAxisManipulator.transform.up;      
        Vector3 manipulatorForward = mAxisManipulator.transform.forward; 

        float totalDisplacement = 0f;
        Vector3 movementVector = Vector3.zero;

        switch (mSelectedAxis)
        {
            case AxisController.Axis.X:
                movementVector = manipulatorRight;
                totalDisplacement = mouseTotalDelta.x * xyDragSpeed;
                break;

            case AxisController.Axis.Y:
                movementVector = manipulatorUp;
                totalDisplacement = mouseTotalDelta.y * xyDragSpeed;
                break;

            case AxisController.Axis.Z:
                movementVector = manipulatorForward;
                totalDisplacement = mouseTotalDelta.y * zDragSpeed;
                break;
        }

        // 1. Calculate the New Position
        Vector3 newWorldPosition = mDragStartSpherePosition + movementVector * totalDisplacement;

        // 2. Apply the position to the View components
        mSelectedSphere.transform.position = newWorldPosition;
        mAxisManipulator.transform.position = newWorldPosition;

        // 3. Update the Mesh Model
        UpdateMeshVertex();
    }

    void HandleSelection(GameObject newSphere)
    {
        // Uses guard clause for readability
        if (mSelectedSphere == newSphere)
        {
            // If the user Ctrl+Clicked the currently selected sphere, deselect it.
            // This allows the user to turn off the manipulator and sphere color.
            mSelectedSphere.GetComponent<SphereController>().Deselect();
            mSelectedSphere = null;
            
            // Clean up manipulator when deselected
            if (mAxisManipulator != null)
            {
                Destroy(mAxisManipulator);
            }
            return;
        }
        
        // Deselect the old sphere if it exists
        if (mSelectedSphere != null)
        {
            mSelectedSphere.GetComponent<SphereController>().Deselect();
        }

        // Select the new one
        mSelectedSphere = newSphere;
        mSelectedSphere.GetComponent<SphereController>().Select();

        // Spawn/Move the manipulator
        if (mAxisManipulator == null)
        {
            mAxisManipulator = Instantiate(AxisFramePrefab);
            mAxisManipulator.name = "AxisManipulator";
            applyAxisControllerAndLayer(mAxisManipulator);
        }
        mAxisManipulator.transform.position = mSelectedSphere.transform.position;
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
            if (!theMesh.IsCylinderMode())
            {
                theMesh.UpdateVertexPosition(index, localPos);
            }
            else
            {
                theMesh.UpdateCylinderVertexPosition(index, localPos);
            }
        }
    }

    // Helper function to dynamically add scripts and set layers to the manipulator
    void applyAxisControllerAndLayer(GameObject axisController)
    {
        // Assumes axis names are exactly "X-Axis", "Y-Axis", "Z-Axis"
        Transform xAxisT = axisController.transform.Find("X-Axis");
        Transform yAxisT = axisController.transform.Find("Y-Axis");
        Transform zAxisT = axisController.transform.Find("Z-Axis");

        if (xAxisT != null)
        {
            xAxisT.gameObject.AddComponent<AxisController>().axisDirection = AxisController.Axis.X;
            // Only set layer if it exists in Unity
            if (LayerMask.NameToLayer("AxisManipulator") != -1)
            {
                 xAxisT.gameObject.layer = LayerMask.NameToLayer("AxisManipulator");
            }
        }
        if (yAxisT != null)
        {
            yAxisT.gameObject.AddComponent<AxisController>().axisDirection = AxisController.Axis.Y;
            if (LayerMask.NameToLayer("AxisManipulator") != -1)
            {
                yAxisT.gameObject.layer = LayerMask.NameToLayer("AxisManipulator");
            }
        }
        if (zAxisT != null)
        {
            zAxisT.gameObject.AddComponent<AxisController>().axisDirection = AxisController.Axis.Z;
            if (LayerMask.NameToLayer("AxisManipulator") != -1)
            {
                zAxisT.gameObject.layer = LayerMask.NameToLayer("AxisManipulator");
            }
        }
    }
}