using UnityEngine;
using TMPro;
public class LightControl : MonoBehaviour
{
    public Transform LightPosition;
    public Transform MainCamera;
    public Transform DirectionalLight;

    public SliderWithEcho XSlider, YSlider, ZSlider;

    public TextMeshProUGUI LightName;

    private bool dirLight = true;


    void Awake()
    {
        XSlider.SetSliderListener(XValueChanged);
        YSlider.SetSliderListener(YValueChanged);
        ZSlider.SetSliderListener(ZValueChanged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (dirLight)
        {
            DirectionalLight.localRotation = Quaternion.LookRotation(MainCamera.forward, Vector3.up);
            DirectionalLight.position = MainCamera.position;
        } else
        {
            Shader.SetGlobalVector("LightPosition", LightPosition.localPosition);
        }
            
    }

    private void XValueChanged(float newValue)
    {
        

    }

    private void YValueChanged(float newValue)
    {
        

    }

    private void ZValueChanged(float newValue)
    {
        

    }
}
