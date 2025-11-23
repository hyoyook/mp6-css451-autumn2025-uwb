using UnityEngine;

public class AxisController : MonoBehaviour
{
    // Define the possible directions for the manipulator
    public enum Axis { X, Y, Z };
    public Axis axisDirection;

    private Renderer mRenderer;
    private Color mDefaultColor;
    // Highlight color (can be set in Inspector, defaults to yellow)
    public Color mHighlightColor = Color.yellow; 

    private void Awake()
    {
        mRenderer = GetComponent<Renderer>();
        if (mRenderer != null && mRenderer.material != null)
        {
            // Create a new instance of the material so we don't change all other manipulators
            mRenderer.material = mRenderer.material;
            mDefaultColor = mRenderer.material.color; 
        }
    }

    // Called by MainController to change the color
    public void Highlight(bool highlight)
    {
        if (mRenderer != null)
        {
            mRenderer.material.color = highlight ? mHighlightColor : mDefaultColor;
        }
    }
}