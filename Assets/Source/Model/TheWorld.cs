/// ---------------------------------------------------------------------------------
/// TheWorld.cs
/// Author: 
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// Manages scene reset
///
/// Reference:
/// * Unity Technologies. (n.d.). "SceneManager.GetActiveScene."
///   Unity Documentation. https://docs.unity3d.com/6000.2/Documentation/ScriptReference/SceneManagement.SceneManager.GetActiveScene.html
/// ---------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;   // for reset

public class TheWorld : MonoBehaviour
{
    public void onResetButtonClicked()
    { 
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);         // reload the scene
        Debug.Log("[TheWorld] Reset Scene");
    }
}
