using UnityEngine;
using TMPro;

// Lighting modes
public enum LightMode
{
    DirectionalFollowCamera,
    DirectionalFixed,
    PointLight
}


public class LightControl : MonoBehaviour
{
    public Transform LightPosition;
    public Transform MainCamera;
    public Transform DirectionalLight;

    public SliderWithEcho XSlider, YSlider, ZSlider;

    public TextMeshProUGUI LightName;

    [SerializeField] private LightMode currentLightMode = LightMode.DirectionalFollowCamera;


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


    }

    // Update is called once per frame
    void Update()
    {
        // Toggle with "T"
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentLightMode = (LightMode)(((int)currentLightMode + 1) % 3);
            Debug.Log($"Switched to: {currentLightMode}");
        }

        // Debug.Log($"LightControl Update: dirLight={dirLight}");
        switch (currentLightMode)
        {
            case LightMode.DirectionalFollowCamera:
                dirLightFollowCamera();
                break;
            case LightMode.DirectionalFixed:
                dirLightFixed();
                break;
            case LightMode.PointLight:
                pointLightOn();
                break;
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

    private void dirLightFollowCamera()
    {
        slidersOff();
        DirectionalLight.gameObject.SetActive(true);
        LightPosition.gameObject.SetActive(false);
        Shader.SetGlobalFloat("_EnableDirLight", 1.0f);
        Shader.SetGlobalFloat("_EnablePointLight", 0.0f);
        LightName.text = "Directional Light (Following Camera)";

        // Follow camera direction and position
        DirectionalLight.rotation = Quaternion.LookRotation(MainCamera.forward, Vector3.up);
        DirectionalLight.position = MainCamera.position;
    }

    private void dirLightFixed()
    {
        slidersOn(); // Enable sliders to control directional light direction
        DirectionalLight.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        DirectionalLight.gameObject.SetActive(true);
        LightPosition.gameObject.SetActive(false);
        Shader.SetGlobalFloat("_EnableDirLight", 1.0f);
        Shader.SetGlobalFloat("_EnablePointLight", 0.0f);
        LightName.text = "Directional Light (Fixed)";

        // Don't update position/rotation - let sliders control it
        // The sliders will now control the directional light's direction
    }

    private void pointLightOn()
    {
        slidersOn();
        DirectionalLight.gameObject.SetActive(false);
        LightPosition.gameObject.SetActive(true);
        Shader.SetGlobalFloat("_EnableDirLight", 0.0f);
        Shader.SetGlobalFloat("_EnablePointLight", 1.0f);
        LightName.text = "Point Light";
        Shader.SetGlobalVector("_LightPosition", LightPosition.position);
    }

    private void initSlider()
    {
        XSlider.SetSliderValue(LightPosition.localPosition.x);
        YSlider.SetSliderValue(LightPosition.localPosition.y);
        ZSlider.SetSliderValue(LightPosition.localPosition.z);
    }

    private void slidersOff()
    {
        XSlider.DisableSlider();
        YSlider.DisableSlider();
        ZSlider.DisableSlider();
    }

    private void slidersOn()
    {
        XSlider.EnableSlider();
        YSlider.EnableSlider();
        ZSlider.EnableSlider();
    }

}
