/// ---------------------------------------------------------------------------------
/// SphereController.cs
/// Author: Julia Nguyen, Alec Situ, Hyobin Yook (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// Handles sphere controller selection and visualize the selection by chaning
/// its color (default white, selected red)
/// ---------------------------------------------------------------------------------

using UnityEngine;

public class SphereController : MonoBehaviour
{
    private Renderer mRenderer;
    private Color mDefaultColor  = Color.white;
    private Color mSelectedColor = Color.red;

    // Cache the Renderer and set initial color
    private void Awake()
    {
        mRenderer = GetComponent<Renderer>();
        if (mRenderer != null)
        {
            mRenderer.material.color = mDefaultColor;
        }
    }

    // Called BY MainController
    public void Select()
    {
        if (mRenderer == null)
        { 
            return; 
        }
        mRenderer.material.color = mSelectedColor; // red
    }

    public void Deselect()
    {
        if (mRenderer == null)
        { 
            return; 
        }
        mRenderer.material.color = mDefaultColor;
    }

}
