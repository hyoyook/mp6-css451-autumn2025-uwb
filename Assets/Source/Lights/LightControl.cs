using UnityEngine;
using TMPro;
public class LightControl : MonoBehaviour
{
    public Transform LightPosition;
    public Transform MainCamera;
    public Transform DirectionalLight;

    public SliderWithEcho XSlider, YSlider, ZSlider;

    public TextMeshProUGUI LightName;

    public bool dirLight = true;


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
            DirectionalLight.gameObject.SetActive(dirLight);
            LightPosition.gameObject.SetActive(!dirLight);
            DirectionalLight.localRotation = Quaternion.LookRotation(MainCamera.forward, Vector3.up);
            DirectionalLight.position = MainCamera.position;
        } else
        {
            DirectionalLight.gameObject.SetActive(dirLight);
            LightPosition.gameObject.SetActive(!dirLight);
            
            Shader.SetGlobalVector("LightPosition", LightPosition.localPosition);
        }
            
    }

    private void XValueChanged(float newValue)
    {
        LightPosition.localPosition = new Vector3(newValue, LightPosition.localPosition.y, LightPosition.localPosition.z);
    }

    private void YValueChanged(float newValue)
    {
        LightPosition.localPosition = new Vector3(LightPosition.localPosition.x, newValue, LightPosition.localPosition.z);
    }

    private void ZValueChanged(float newValue)
    {
        LightPosition.localPosition = new Vector3(LightPosition.localPosition.x, LightPosition.localPosition.y, newValue);
        

    }
}
