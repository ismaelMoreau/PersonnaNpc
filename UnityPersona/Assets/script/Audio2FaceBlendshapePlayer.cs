using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Audio2FaceBlendshapePlayer : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public AudioSource audioSource;
    public float playbackSpeed = 1.0f;

    private List<float> timeStamps;
    private List<Dictionary<string, float>> blendshapeData;
    private Dictionary<string, int> blendshapeMap;

    private float[] frameDurations;

    // Method to initialize synchronization with CSV content
    public void InitializeSynchronization(string csvContent, AudioClip audioClip)
    {
        // Load CSV data from the content string
        LoadCSVData(csvContent);

        // Set audio clip
        audioSource.clip = audioClip;

        // Start audio and animation
        StartCoroutine(StartAudioAndAnimation());
    }

    IEnumerator StartAudioAndAnimation()
    {
        // Precompute frame durations for efficiency
        frameDurations = new float[timeStamps.Count - 1];
        for (int i = 0; i < timeStamps.Count - 1; i++)
        {
            frameDurations[i] = timeStamps[i + 1] - timeStamps[i];
        }

        // Use AudioSettings.dspTime for accurate scheduling
        double startTime = AudioSettings.dspTime + 0.5; // Half second delay to ensure everything is ready

        audioSource.PlayScheduled(startTime);
        Debug.Log($"🎵 Audio scheduled to play at {startTime} DSP time");

        // Wait until the audio actually starts
        while (audioSource.time <= 0)
        {
            yield return null;
        }

        Debug.Log($"✅ Audio Started: {audioSource.time}");
        StartCoroutine(AnimateBlendshapes());
    }

    IEnumerator AnimateBlendshapes()
    {
        float audioDuration = audioSource.clip.length;
        int totalFrames = timeStamps.Count;

        Debug.Log($"🔍 Audio Duration: {audioDuration}s, Total Frames: {totalFrames}");

        while (audioSource.isPlaying)
        {
            float audioTime = audioSource.time * playbackSpeed;

            // Find the correct frame pair to interpolate between
            int currentFrame = 0;
            while (currentFrame < totalFrames - 1 && timeStamps[currentFrame + 1] <= audioTime)
            {
                currentFrame++;
            }

            // If we're at the last frame or exactly on a timestamp, just apply that frame
            if (currentFrame >= totalFrames - 1 || Mathf.Approximately(timeStamps[currentFrame], audioTime))
            {
                ApplyBlendshapeFrame(blendshapeData[currentFrame]);
            }
            // Otherwise interpolate between current and next frame
            else
            {
                int nextFrame = currentFrame + 1;
                float frameDuration = timeStamps[nextFrame] - timeStamps[currentFrame];
                float t = frameDuration > 0 ? (audioTime - timeStamps[currentFrame]) / frameDuration : 0;

                InterpolateAndApplyBlendshapes(blendshapeData[currentFrame], blendshapeData[nextFrame], t);
            }

            yield return null;
        }
    }

    void InterpolateAndApplyBlendshapes(Dictionary<string, float> startFrame, Dictionary<string, float> endFrame, float t)
    {
        foreach (var blendshapeName in blendshapeMap.Keys)
        {
            float startValue = startFrame.TryGetValue(blendshapeName, out float start) ? start : 0f;
            float endValue = endFrame.TryGetValue(blendshapeName, out float end) ? end : 0f;
            float interpolatedValue = Mathf.Lerp(startValue, endValue, t);

            skinnedMeshRenderer.SetBlendShapeWeight(blendshapeMap[blendshapeName], interpolatedValue);
        }
    }

    void ApplyBlendshapeFrame(Dictionary<string, float> frameData)
    {
        foreach (var entry in frameData)
        {
            if (blendshapeMap.TryGetValue(entry.Key, out int blendshapeIndex))
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndex, entry.Value);
            }
            else
            {
                Debug.LogWarning($"⚠️ Blendshape {entry.Key} not found!");
            }
        }
    }

    // Modified LoadCSVData to handle CSV content directly as a string
    void LoadCSVData(string csvContent)
    {
        timeStamps = new List<float>();
        blendshapeData = new List<Dictionary<string, float>>();
        blendshapeMap = new Dictionary<string, int>();

        if (string.IsNullOrWhiteSpace(csvContent))
        {
            Debug.LogError("CSV content is empty.");
            return;
        }

        string[] lines = csvContent.Split('\n');
        if (lines.Length < 2)
            return;

        string[] headers = lines[0].Split(',');

        // Blendshape name mapping (same as previous implementation)
        for (int i = 2; i < headers.Length; i++)
        {
            string blendshapeName = headers[i].Trim().Replace("blendShapes.", "");

            // Adjust mapping here to match your FBX names
            string fbxName = blendshapeName switch
            {
                "EyeBlinkLeft" => "eyeBlink_L",
                "EyeLookDownLeft" => "eyeLookDown_L",
                "EyeLookInLeft" => "eyeLookIn_L",
                "EyeLookOutLeft" => "eyeLookOut_L",
                "EyeLookUpLeft" => "eyeLookUp_L",
                "EyeSquintLeft" => "eyeSquint_L",
                "EyeWideLeft" => "eyeWide_L",
                "EyeBlinkRight" => "eyeBlink_R",
                "EyeLookDownRight" => "eyeLookDown_R",
                "EyeLookInRight" => "eyeLookIn_R",
                "EyeLookOutRight" => "eyeLookOut_R",
                "EyeLookUpRight" => "eyeLookUp_R",
                "EyeSquintRight" => "eyeSquint_R",
                "EyeWideRight" => "eyeWide_R",
                "JawForward" => "jawForward",
                "JawLeft" => "jawLeft",
                "JawRight" => "jawRight",
                "JawOpen" => "jawOpen",
                "MouthClose" => "mouthClose",
                "MouthFunnel" => "mouthFunnel",
                "MouthPucker" => "mouthPucker",
                "MouthLeft" => "mouthLeft",
                "MouthRight" => "mouthRight",
                "MouthSmileLeft" => "mouthSmile_L",
                "MouthSmileRight" => "mouthSmile_R",
                "MouthFrownLeft" => "mouthFrown_L",
                "MouthFrownRight" => "mouthFrown_R",
                "MouthDimpleLeft" => "mouthDimple_L",
                "MouthDimpleRight" => "mouthDimple_R",
                "MouthStretchLeft" => "mouthStretch_L",
                "MouthStretchRight" => "mouthStretch_R",
                "MouthRollLower" => "mouthRollLower",
                "MouthRollUpper" => "mouthRollUpper",
                "MouthShrugLower" => "mouthShrugLower",
                "MouthShrugUpper" => "mouthShrugUpper",
                "MouthPressLeft" => "mouthPress_L",
                "MouthPressRight" => "mouthPress_R",
                "MouthLowerDownLeft" => "mouthLowerDown_L",
                "MouthLowerDownRight" => "mouthLowerDown_R",
                "MouthUpperUpLeft" => "mouthUpperUp_L",
                "MouthUpperUpRight" => "mouthUpperUp_R",
                "BrowDownLeft" => "browDown_L",
                "BrowDownRight" => "browDown_R",
                "BrowInnerUp" => "browInnerUp_R", // or "_R" depending on your model
                "BrowOuterUpLeft" => "browOuterUp_L",
                "BrowOuterUpRight" => "browOuterUp_R",
                "CheekPuff" => "cheekPuff_L", // or "_R" depending on your model
                "CheekSquintLeft" => "cheekSquint_L",
                "CheekSquintRight" => "cheekSquint_R",
                "NoseSneerLeft" => "noseSneer_L",
                "NoseSneerRight" => "noseSneer_R",
                "TongueOut" => "tongueOut",
                "HeadRoll" => "headRoll",
                "HeadPitch" => "headPitch",
                "HeadYaw" => "headYaw",
                _ => blendshapeName // Default case if no match found
            };

            int blendshapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(fbxName);
            if (blendshapeIndex != -1)
            {
                blendshapeMap[blendshapeName] = blendshapeIndex;
            }
            else
            {
                Debug.LogWarning($"❌ No mapping defined for {blendshapeName}");
            }
        }

        // Read animation data
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] values = lines[i].Split(',');
            if (values.Length < headers.Length) continue;

            if (!float.TryParse(values[1], out float time)) continue;

            Dictionary<string, float> frameData = new Dictionary<string, float>();
            for (int j = 2; j < values.Length; j++)
            {
                string blendshapeName = headers[j].Trim().Replace("blendShapes.", "");
                if (blendshapeMap.ContainsKey(blendshapeName) && float.TryParse(values[j], out float weight))
                {
                    frameData[blendshapeName] = weight * 100f;
                }
            }

            timeStamps.Add(time);
            blendshapeData.Add(frameData);
        }
    }
}
