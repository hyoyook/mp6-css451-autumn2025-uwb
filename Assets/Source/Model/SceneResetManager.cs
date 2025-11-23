/// ---------------------------------------------------------------------------------
/// SceneResetManager.cs
/// Author: Hyobin Yook, Alec Situ, Julia Nguyen (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// Handles reloading the scene to restore all objects, UI, and runtime data to 
/// their initial state.
///
/// Reference:
/// * Unity Technologies. (n.d.). "SceneManager.GetActiveScene." Unity Documentation. 
///   https://docs.unity3d.com/6000.2/Documentation/ScriptReference/SceneManagement.SceneManager.GetActiveScene.html
/// ---------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;   // for reset

public class SceneResetManager : MonoBehaviour
{
    public void onResetButtonClicked()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);   // reload the scene
    }
}
