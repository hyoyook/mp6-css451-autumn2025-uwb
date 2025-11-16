using UnityEngine;

public class SphereController : MonoBehaviour
{
    private Renderer mRenderer;
    private Color mDefaultColor = Color.white;
    private Color mSelectedColor = Color.red;
    private bool mIsSelected = false;
    
    
    // Cache the Renderer and set initial color
    private void Awake()
    {
        mRenderer = GetComponent<Renderer>();
        if (mRenderer != null)
        {
            mRenderer.material.color = mDefaultColor;
        }
    }

    // Called when user LMB clicks on this sphere
    /*private void OnMouseDown()
    {
        mIsSelected = !mIsSelected;

        if (mRenderer == null)
        { 
            return; 
        }

        // Change color based on selection
        if (mIsSelected)
        {
            mRenderer.material.color = mSelectedColor; // red
        }
        else
        { 
            mRenderer.material.color = mDefaultColor; 
        }
    }*/

    // Called BY MainController
    public void Select()
    {
        if (mRenderer == null)
        { 
            return; 
        }
        //if (mRenderer != null)
            mRenderer.material.color = mSelectedColor; // red
    }

    public void Deselect()
    {
        if (mRenderer == null)
        { 
            return; 
        }
        //if (mRenderer != null)
            mRenderer.material.color = mDefaultColor;
    }

}
