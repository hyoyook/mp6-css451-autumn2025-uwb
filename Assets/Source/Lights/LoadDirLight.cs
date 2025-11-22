using UnityEngine;

public class LoadDirLight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform LightPosition;

    void Update()
    {
        Shader.SetGlobalVector("LightPosition", LightPosition.localPosition);
    }
}
