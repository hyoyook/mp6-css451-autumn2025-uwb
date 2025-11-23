using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public partial class MainController : MonoBehaviour
{

    public SliderWithEcho N_Slider, M_Slider, CylinderN_Slider, CylinderM_Slider, Cylinder_Rotation_Slider;

    public TMP_Dropdown shapeDropdown;

    void Awake()
    {
        shapeDropdown.onValueChanged.AddListener(OnShapeDropdownChanged);
        N_Slider.SetSliderListener(ResolutionSliderChanged);
        M_Slider.SetSliderListener(ResolutionSliderChanged);

        CylinderN_Slider.SetSliderListener(ResolutionSliderChanged);
        CylinderM_Slider.SetSliderListener(ResolutionSliderChanged);
        Cylinder_Rotation_Slider.SetSliderListener(CylinderRotationSliderChanged);
    }
    void Start()
    {
        OnShapeDropdownChanged(0);

        // added to force UI hookup to texture placement
        if (UV_XformControl != null && TexturePlacement != null)
        {
            UV_XformControl.TextureTarget = TexturePlacement;
        }
    }


    public void OnShapeDropdownChanged(int index)
    {

        int N, M;
        HandleDeselection();
        resetState();
        // Debug.Log("OnShapeDropdown called with index: " + index);
        if (index == 0) // 0 = Plane
        {
            // Debug.Log("Plane Chosen");
            theMesh.Build_Plane_Mesh((int)N_Slider.GetSliderValue(), (int)M_Slider.GetSliderValue());
            theMesh.CylinderModeOff();
        }
        else if (index == 1) // 1 = Cylinder
        {
            // Debug.Log("Cylinder Chosen");
            theMesh.CylinderModeOn();
            N = (int)CylinderN_Slider.GetSliderValue();
            M = (int)CylinderM_Slider.GetSliderValue();
            int cylinderRotation = (int)Cylinder_Rotation_Slider.GetSliderValue();
            theMesh.Build_Cylinder_Mesh(N, M, cylinderRotation);
        }
    }

    public void ResolutionSliderChanged(float __)
    {
        int shapeChoice = shapeDropdown.value;
        int N, M;
        HandleDeselection();
        resetState();
        if (shapeChoice == 0) // Plane
        {

            N = (int)N_Slider.GetSliderValue();
            M = (int)M_Slider.GetSliderValue();
            // Debug.Log($"ResolutionSliderChanged on shapeChoice={shapeChoice}: Plane N={N}, M={M}");
            theMesh.Build_Plane_Mesh(N, M);
        }
        else if (shapeChoice == 1) // Cylinder
        {
            N = (int)CylinderN_Slider.GetSliderValue();
            M = (int)CylinderM_Slider.GetSliderValue();
            int cylinderRotation = (int)Cylinder_Rotation_Slider.GetSliderValue();
            // Debug.Log($"ResolutionSliderChanged on shapeChoice={shapeChoice}: Cylinder N={N}, M={M}, Cylinder Rotation={cylinderRotation}");
            theMesh.Build_Cylinder_Mesh(N, M, cylinderRotation);


        }
    }

    public void CylinderRotationSliderChanged(float rotation)
    {
        int shapeChoice = shapeDropdown.value;
        if (shapeChoice != 1) return; // Not Cylinder

        // Use the new update method instead of rebuilding
        theMesh.UpdateCylinderRotation((int)rotation);
        Debug.Log($"CylinderRotationSliderChanged: Updated cylinder rotation to {rotation}°");
    }
}