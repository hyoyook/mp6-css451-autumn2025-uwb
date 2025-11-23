/// ---------------------------------------------------------------------------------
/// AudioLoop.cs
/// Author: Julia Nguyen, Alec Situ, Hyobin Yook (CSS451, Team 8)
/// Last Edited: November 22, 2025
/// ---------------------------------------------------------------------------------
/// Created for MP6, CSS451, UWB. 
/// 
/// References:
/// * Unity Technologies. (n.d.). "AudioSource.mute." Unity Documentation.
///   https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AudioSource-mute.html
/// * Unity Technologies. (n.d.). "AudioSource.loop." Unity Documentation.
///   https://docs.unity3d.com/6000.2/Documentation/ScriptReference/AudioSource-loop.html
/// * OpenAI, LLC. (n.d.). "Sequential audio looping" ChatGPT
///   https://chat.openai.com
/// ---------------------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public class AudioLoop : MonoBehaviour
{
    public AudioSource audioA;
    public AudioSource audioB;

    private Coroutine loopRoutine;

    private void Start()
    {
        StartLoop();
    }

    // Source: Unity Technologies., "AudioSource.mute."
    private void Update()
    {
        // press 'M' to mute / unmute
        if (Input.GetKeyDown(KeyCode.M))
        {
            bool muteState = !audioA.mute;
            audioA.mute = muteState; 
            audioB.mute = muteState;
        }
    }

    // Source: Unity Technologies., "AudioSource.loop."
    private void StartLoop() 
    {
        if (loopRoutine != null) 
        {
            // if already looping, move on
            return;
        }
        loopRoutine = StartCoroutine(PlayList());
    }
   
    // Source: OpenAI, LLC. "Sequential audio looping"
    private IEnumerator PlayList() 
    {
        audioA.enabled = true;
        audioB.enabled = true;
        while (true)
        {
            audioA.Play();
            yield return new WaitForSeconds(audioA.clip.length);

            audioB.Play();
            yield return new WaitForSeconds(audioB.clip.length);
        }
    }

}
