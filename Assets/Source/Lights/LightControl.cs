using UnityEngine;
using TMPro;
public class LightControl : MonoBehaviour
{
    public Transform LightPosition;
    public Transform MainCamera;
    public Transform DirectionalLight;

    public SliderWithEcho XSlider, YSlider, ZSlider;

    public TextMeshProUGUI LightName;

    [SerializeField] private bool dirLight = true;


    void Awake()
    {
        initSlider();
        XSlider.SetSliderListener(XValueChanged);
        YSlider.SetSliderListener(YValueChanged);
        ZSlider.SetSliderListener(ZValueChanged);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"LightControl Start: dirLight = {dirLight}");

    }

    // Update is called once per frame
    void Update()
    {
        // Toggle with T key for testing
        if (Input.GetKeyDown(KeyCode.T))
        {
            dirLight = !dirLight;
            // Debug.Log($"Toggled dirLight to: {dirLight}");
        }

        // Debug.Log($"LightControl Update: dirLight={dirLight}");
        if (dirLight)
        {
            dirLightOn();
        }
        else
        {
            dirLightOff();
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

    private void dirLightOn()
    {
        // Debug.Log($"Directional Light Active: {dirLight}");
        slidersOff();
        DirectionalLight.gameObject.SetActive(true);
        LightPosition.gameObject.SetActive(false);
        Shader.SetGlobalFloat("_EnableDirLight", 1.0f);
        Shader.SetGlobalFloat("_EnablePointLight", 0.0f);
        LightName.text = "Directional Light";
        DirectionalLight.localRotation = Quaternion.LookRotation(MainCamera.forward, Vector3.up);
        DirectionalLight.position = MainCamera.position;
    }

    private void dirLightOff()
    {
        // Debug.Log($"Directional Light InActive: {dirLight}");
        slidersOn();
        DirectionalLight.gameObject.SetActive(false);
        LightPosition.gameObject.SetActive(true);
        Shader.SetGlobalFloat("_EnableDirLight", 0.0f);
        Shader.SetGlobalFloat("_EnablePointLight", 1.0f);
        LightName.text = "Point Light";
        Shader.SetGlobalVector("_LightPosition", LightPosition.position);
    }

    private void initSlider() {
        XSlider.SetSliderValue(LightPosition.localPosition.x);
        YSlider.SetSliderValue(LightPosition.localPosition.y);
        ZSlider.SetSliderValue(LightPosition.localPosition.z);
    }

    private void slidersOff() {
        XSlider.DisableSlider();
        YSlider.DisableSlider();
        ZSlider.DisableSlider();
    }

    private void slidersOn() {
        XSlider.EnableSlider();
        YSlider.EnableSlider();
        ZSlider.EnableSlider();
    }

}
