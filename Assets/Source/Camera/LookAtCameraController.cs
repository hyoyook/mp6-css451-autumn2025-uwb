using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCameraController : MonoBehaviour
{

    [Header("Refs")]
    public Transform LookAtCamera;
    public Transform LookAtTarget;

    [Header("Speeds")]
    public float tumbleSpeed; // Rotation speed
    public float trackSpeed; // pan speed
    public float dollySpeed; // Zoom speed

    // Fields to help with mouse input
    Vector2 prevMousePos;
    bool altHeld, leftMouseHeld, rightMouseHeld;
    const float kMaxPitch = 85f; // Used to stop pole singularity


    // lookat camera should align with lookat target on start.
    void Start()
    {

        Debug.Assert(LookAtCamera == null, "LookAtCameraController: LookAtCamera is not assigned!");
        Debug.Assert(LookAtTarget == null, "LookAtCameraController: LookAtTarget is not assigned!");


        // Set the initial orientation of the camera to look at the target
        Vector3[] orientation = createNewOrientationNormals(LookAtCamera.position, LookAtTarget.position);
        Matrix4x4 camTRS = createMatrix(orientation[0], orientation[1], orientation[2], LookAtCamera.position);
        LookAtCamera.position = camTRS.GetColumn(3);
        LookAtCamera.rotation = camTRS.rotation;
        Debug.Log("LookAtCameraController: Start completed, camera oriented to target.");
    }

    void LateUpdate()
    {
        FaceTarget();
    }

    // update in charge of checking for user input
    void Update()
    {
        altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        leftMouseHeld = Input.GetMouseButton(0);
        rightMouseHeld = Input.GetMouseButton(1);

        /*
            Zoom (Dolly) = Alt + Scroll wheel
                eyePos = eyePos + (scroll delta * forward vector)

            Tumble (Orbit) = Alt + Left mouse drag
                move to, rotate based on mouse delta, move back

            Track (Pan) = Alt + Right mouse drag combining left/right and up/down
                eyePos = eyePos + (right vector * deltaX) + (up vector * deltaY)
        */

        // Zoom (Dolly)
        if (altHeld)
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {   // zoom in/out without passing through the LookAtTarget
                Vector3 forward = (LookAtCamera.position - LookAtTarget.position).normalized;
                float currentDistance = Vector3.Distance(LookAtCamera.position, LookAtTarget.position);
                float newDistance = currentDistance + scrollDelta * dollySpeed;
                if (newDistance < 0.5f) // Stop it from zooming past object
                {
                    newDistance = 0.5f;
                }

                LookAtCamera.position = LookAtTarget.position + forward * newDistance;
            }
        }

        // Tumble (Orbit)
        if (altHeld && leftMouseHeld)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseDelta = mousePos - prevMousePos;
            prevMousePos = mousePos;

            float deltaX = mouseDelta.x / Screen.width * 180f;  // degrees
            float deltaY = -(mouseDelta.y / Screen.height) * 180f;  // invert Y movement
            if (mouseDelta.magnitude > 0.001f)
            {
                Debug.Log("LookAt cam Position before Tumble: " + LookAtCamera.position);
                TumbleSteps(LookAtCamera, LookAtTarget.position, deltaX, deltaY);
                Debug.Log("LookAt cam Position after Tumble: " + LookAtCamera.position);
            }

        }
        // Track/Pan (also moves the target)
        else if (altHeld && rightMouseHeld)
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 mouseDelta = mousePos - prevMousePos;
            prevMousePos = mousePos;

            Vector3 right = LookAtCamera.right;
            Vector3 up = LookAtCamera.up;

            LookAtCamera.position += (-right * mouseDelta.x + -up * mouseDelta.y) * (trackSpeed * Time.deltaTime);
            LookAtTarget.position += (-right * mouseDelta.x + -up * mouseDelta.y) * (trackSpeed * Time.deltaTime);
        }
        // reset previous mouse position when no drag is happening
        else
        {
            prevMousePos = Input.mousePosition;
        }
    }


    // Rotate the camera around the target based on mouse changes
    private void TumbleSteps(Transform cam, Vector3 lookAtObjPos, float deltaX, float deltaY)
    {
        // Calculate forward, right, and up vectors
        Vector3 forward = (lookAtObjPos - cam.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 up = Vector3.Cross(forward, right).normalized;

        // Limit how far we can tumble up/down
        deltaY = ClampPitchAngle(forward, -deltaY);

        // Move - rotations - move backwards to new position
        Matrix4x4 steps =
            Matrix4x4.Translate(lookAtObjPos) *
            Matrix4x4.Rotate(Quaternion.AngleAxis(deltaX * tumbleSpeed, Vector3.up)) *
            Matrix4x4.Rotate(Quaternion.AngleAxis(-deltaY * tumbleSpeed, right)) *
            Matrix4x4.Translate(-lookAtObjPos);

        // store new camera pos and remake the axes
        Vector3 newCamPos = steps.MultiplyPoint(cam.position);
        Vector3[] newOrientation = createNewOrientationNormals(newCamPos, lookAtObjPos);

        // build new TRS
        Matrix4x4 newCamTRS = createMatrix(newOrientation[0], newOrientation[1], newOrientation[2], newCamPos);

        // set camera position and rotation
        cam.position = newCamTRS.GetColumn(3);
        cam.rotation = newCamTRS.rotation;
        Debug.Log($"LookAtCameraController: TumbleSteps - Camera moved to {cam.position}");
    }


    // Create a matrix using given vectors and poistions.
    private Matrix4x4 createMatrix(Vector3 right, Vector3 up, Vector3 forward, Vector3 position)
    {
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
        m.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
        m.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
        m.SetColumn(3, new Vector4(position.x, position.y, position.z, 1));
        return m;
    }

    // Computes orthonormal for camera given position and target pos.
    private Vector3[] createNewOrientationNormals(Vector3 camPos, Vector3 lookAtObjPos)
    {
        Vector3 forward = (lookAtObjPos - camPos).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        // Trying to stop that forward = up vector normal issue
        if (right.sqrMagnitude < 1e-6f)
        {
            Vector3 alt = Mathf.Abs(forward.y) > 0.9f ? Vector3.right : Vector3.forward;
            right = Vector3.Cross(alt, forward);
        }
        right = right.normalized;

        Vector3 up = Vector3.Cross(forward, right).normalized;
        return new Vector3[] { right, up, forward };
    }

    // Limit the pitch angle so fix pole singularity 
    private float ClampPitchAngle(Vector3 forward, float pitchAngle)
    {
        float currentPitchDeg = Mathf.Asin(Mathf.Clamp(forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        float targetPitchDeg = Mathf.Clamp(currentPitchDeg + pitchAngle, -kMaxPitch, kMaxPitch);
        return targetPitchDeg - currentPitchDeg;
    }

    // Always called in late update to aim LookAtCam at 
    private void FaceTarget()
    {
        AimAt(LookAtCamera, LookAtTarget.position);
    }

    // Orient camera to look at specific position using Matix TRS
    private void AimAt(Transform cam, Vector3 LookAtPos)
    {
        Vector3[] orientation = createNewOrientationNormals(cam.position, LookAtPos);
        Matrix4x4 camTRS = createMatrix(orientation[0], orientation[1], orientation[2], cam.position);
        cam.rotation = camTRS.rotation;
    }
}
