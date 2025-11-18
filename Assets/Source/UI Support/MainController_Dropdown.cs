using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
public partial class MainController : MonoBehaviour
{

    public SliderWithEcho N_Slider, M_Slider, CylinderN_Slider, CylinderM_Slider;

    public TMP_Dropdown shapeDropdown;

    void Awake()
    {
        shapeDropdown.onValueChanged.AddListener(OnShapeDropdownChanged);
    }
    void Start()
    {
        OnShapeDropdownChanged(0);
    }


    public void OnShapeDropdownChanged(int index)
    {

        Debug.Log("OnShapeDropdown called with index: " + index);
        if (index == 0) // 0 = Plane
        {
            Debug.Log("Plane Chosen");
            // theMesh.BuildMesh(N_Slider.GetSliderValue(), M_Slider.GetSliderValue());
        }
        else if (index == 1) // 1 = Cylinder
        {
            Debug.Log("Cylinder Chosen");
            // Get values from your other sliders
            // int cylResolution = ... ;
            // int cylSegments = ... ;
            // float cylSweep = ... ;
            // theMesh.BuildCylinderMesh(20, 360);
        }
    }
}