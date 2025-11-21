using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class XfromControl : MonoBehaviour {
    public Toggle T, R, S;
    public SliderWithEcho X, Y, Z;
    public TextMeshProUGUI ObjectName;

    private Transform mSelected;
    private Vector3 mPreviousSliderValues = Vector3.zero;

    // added for mp6
    public TexturePlacement TextureTarget;
    private bool InTextureMode = true;

    // Use this for initialization
    void Start () {
        T.onValueChanged.AddListener(SetToTranslation);
        R.onValueChanged.AddListener(SetToRotation);
        S.onValueChanged.AddListener(SetToScaling);
        X.SetSliderListener(XValueChanged);
        Y.SetSliderListener(YValueChanged);
        Z.SetSliderListener(ZValueChanged);

        T.isOn = true;
        R.isOn = false;
        S.isOn = false;
        SetToTranslation(true);
	}
	
    //---------------------------------------------------------------------------------
    // Initialize slider bars to specific function
    void SetToTranslation(bool v)
    {
        if (InTextureMode)
        {
            // Z disabled in UV mode
            X.InitSliderRange(-4f, 4f, TextureTarget.UV_Translate_X);
            Y.InitSliderRange(-4f, 4f, TextureTarget.UV_Translate_Y);
            Z.InitSliderRange(0f, 0f, 0f);

            X.TheSlider.interactable = true;
            Y.TheSlider.interactable = true;
            Z.TheSlider.interactable = false;

            mPreviousSliderValues = new Vector3(TextureTarget.UV_Translate_X,
                                                TextureTarget.UV_Translate_Y,
                                                0f);
            return;
        }

        Vector3 p = ReadObjectXfrom();
        mPreviousSliderValues = p;
        X.InitSliderRange(-20, 20, p.x);
        Y.InitSliderRange(-20, 20, p.y);
        Z.InitSliderRange(-20, 20, p.z);
        
        X.TheSlider.interactable = true;
        Y.TheSlider.interactable = true;
        Z.TheSlider.interactable = true;

    }

    void SetToScaling(bool v)
    {
        if (InTextureMode)
        {
            // Z disabled in UV mode
            X.InitSliderRange(0.1f, 10f, TextureTarget.UV_Scale_X);
            Y.InitSliderRange(0.1f, 10f, TextureTarget.UV_Scale_Y);
            Z.InitSliderRange(1f, 1f, 1f);

            X.TheSlider.interactable = true;
            Y.TheSlider.interactable = true;
            Z.TheSlider.interactable = false;

            mPreviousSliderValues = new Vector3(TextureTarget.UV_Scale_X,
                                                TextureTarget.UV_Scale_Y,
                                                0f);
            return;
        }

        Vector3 s = ReadObjectXfrom();
        mPreviousSliderValues = s;
        X.InitSliderRange(0.1f, 5, s.x);
        Y.InitSliderRange(0.1f, 5, s.y);
        Z.InitSliderRange(0.1f, 5, s.z);

        X.TheSlider.interactable = true;
        Y.TheSlider.interactable = true;
        Z.TheSlider.interactable = true;
    }

    void SetToRotation(bool v)
    {
        if (InTextureMode)
        {
            // only Z rotation
            X.InitSliderRange(0f, 0f, 0f);
            Y.InitSliderRange(0f, 0f, 0f);
            Z.InitSliderRange(-180f, 180f, TextureTarget.UV_Rotation);

            X.TheSlider.interactable = false;
            Y.TheSlider.interactable = false;
            Z.TheSlider.interactable = true;

            mPreviousSliderValues = new Vector3(0f, 0f, TextureTarget.UV_Rotation);

            return;
        }

        Vector3 r = ReadObjectXfrom();
        mPreviousSliderValues = r;
        X.InitSliderRange(-180, 180, r.x);
        Y.InitSliderRange(-180, 180, r.y);
        Z.InitSliderRange(-180, 180, r.z);
        mPreviousSliderValues = r;

        X.TheSlider.interactable = true;
        Y.TheSlider.interactable = true;
        Z.TheSlider.interactable = true;
    }
    //---------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------
    // resopond to sldier bar value changes
    void XValueChanged(float v)
    {
        if (InTextureMode)
        {
            if (T.isOn)
            {
                // translate in U
                TextureTarget.UV_Translate_X = v;
            }
            else if (S.isOn)
            {
                // scale in U
                TextureTarget.UV_Scale_X = v;
            }
            // no rotation supported about x-axis in uv space
            return;
        }

        if (mSelected == null)
            return;
        Vector3 p = ReadObjectXfrom();
        // if not in rotation, next two lines of work would be wasted
            float dx = v - mPreviousSliderValues.x;
            mPreviousSliderValues.x = v;
        Quaternion q = Quaternion.AngleAxis(dx, Vector3.right);  // **- Please read the notes at the end
        p.x = v;
        UISetObjectXform(ref p, ref q);
    }
    
    void YValueChanged(float v)
    {
        if (InTextureMode)
        {
            if (T.isOn)
            {
                // translate in V
                TextureTarget.UV_Translate_Y = v;
            }
            else if (S.isOn)
            {
                // scale in V
                TextureTarget.UV_Scale_Y = v;
            }
            // no rotation supported about Y-axis in uv space
            return;
        }

        if (mSelected == null)
            return;
        Vector3 p = ReadObjectXfrom();
            // if not in rotation, next two lines of work would be wasted
            float dy = v - mPreviousSliderValues.y;
            mPreviousSliderValues.y = v;
        Quaternion q = Quaternion.AngleAxis(dy, Vector3.up);    // **- Please read the notes at the end
        p.y = v;        
        UISetObjectXform(ref p, ref q);
    }

    void ZValueChanged(float v)
    {
        if (InTextureMode)
        {
            if (R.isOn)
            {
                // only Z is valid for rotation
                TextureTarget.UV_Rotation = v;
            }
            // translation and scale about z-axis NOT supported UV space
            return;
        }

        if (mSelected == null)
            return;
        Vector3 p = ReadObjectXfrom();
            // if not in rotation, next two lines of work would be wasterd
            float dz = v - mPreviousSliderValues.z;
            mPreviousSliderValues.z = v;
        Quaternion q = Quaternion.AngleAxis(dz, Vector3.forward); // **- Please read the notes at the end
        p.z = v;
        UISetObjectXform(ref p, ref q);
    }
    //---------------------------------------------------------------------------------

    // new object selected
    public void SetSelectedObject(Transform xform)
    {
        if (InTextureMode)
        {
            if (ObjectName != null)
            { 
                ObjectName.text = "Selected: Texture"; 
            }
            return;
        }
        mSelected = xform;
        mPreviousSliderValues = Vector3.zero;
        if (xform != null)
            ObjectName.text = "Selected:" + xform.name;
        else
            ObjectName.text = "Selected: none";
        ObjectSetUI();
    }

    public void ObjectSetUI()
    {
        Vector3 p = ReadObjectXfrom();
        X.SetSliderValue(p.x);  // do not need to call back for this comes from the object
        Y.SetSliderValue(p.y);
        Z.SetSliderValue(p.z);
    }

    private Vector3 ReadObjectXfrom()
    {
        Vector3 p;
        
        if (T.isOn)
        {
            if (mSelected != null)
                p = mSelected.localPosition;
            else
                p = Vector3.zero;
        }
        else if (S.isOn)
        {
            if (mSelected != null)
                p = mSelected.localScale;
            else
                p = Vector3.one;
        }
        else
        {
            p = Vector3.zero;
        }
        return p;
    }

    private void UISetObjectXform(ref Vector3 p, ref Quaternion q)
    {
        if (mSelected == null)
            return;

        if (T.isOn)
        {
            mSelected.localPosition = p;
        }
        else if (S.isOn)
        {
            mSelected.localScale = p;
        } else
        {
            mSelected.localRotation = mSelected.localRotation * q; // **- Please read the notes at the end @
        }
    }

    /* ** - Note on Quaternion rotation
    
    The order of concatenating quaternions is important.

        qc = q2 * q1

    Says, q1 rotation occurs BEFORE q2 (q1 first and then q2)

    In this case, the concatnation of subsequent rotations along the major axes

        localRotaiton = localRotation * qr 

            where qr = NewRotation_AlongMajorAxis (either x, y, or z)

    says, qr roation is applied _before_ the current rotation. This is obviously _NOT_ what happens, 
    the user's latest rotation should be applied _last_. So, what we want is:

         localRoation = qr' * localRotation

    where qr' is rotation along the _rotated_ major axes (rotated x, y, or z). Interestingly, 

        localRotation = qr' * locationRotation = locationRotate * qr

    Note: the difference between qr' and qr is the axis of rotation (before and after the localRotation).

    This can be verified by, e.g., for x-axis rotation by theta,

        qr = QFromAxis([1, 0, 0], theta)
        qr' = QFromAxis(Axis, theta)
    where, Axis, is
        Column-0 of RotationMatrix-of-localRotation
    */
    
}