// using UnityEngine;
// using UnityEngine.Networking;
// using System;
// using System.Collections;
// using System.Text;
// using System.IO;

// public class PiperTTSRequestHandler : MonoBehaviour
// {
//     [SerializeField] private string dockerServiceUrl = "http://localhost:8010/generate_speech";
//     [SerializeField] private string downloadBaseUrl = "http://localhost:8010";
    
//     // Delegate for TTS generation completion
//     public delegate void TTSGenerationCompleteHandler(string wavFilePath);

//     [Serializable]
//     private class TTSRequestData
//     {
//         public string text;
//         public string model = "en_US-joe-medium";
//     }

//     [Serializable]
//     public class TTSResponse
//     {
//         public string status;
//         public string file_url;
//     }

//     public void SendTTSRequest(string text, TTSGenerationCompleteHandler onGenerationComplete = null, string model = "en_US-joe-medium")
//     {
//         StartCoroutine(SendPostRequestCoroutine(text, onGenerationComplete, model));
//     }
//      private IEnumerator SendPostRequestCoroutine(string text, TTSGenerationCompleteHandler onGenerationComplete, string model)
//     {
//         // Create request data
//         TTSRequestData requestData = new TTSRequestData 
//         { 
//             text = text, 
//             model = model 
//         };

//         string jsonRequest = JsonUtility.ToJson(requestData);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

//         using (UnityWebRequest www = new UnityWebRequest(dockerServiceUrl, "POST"))
//         {
//             www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             yield return www.SendWebRequest();

//              // Check for errors first
//             if (www.result == UnityWebRequest.Result.ConnectionError ||
//                 www.result == UnityWebRequest.Result.ProtocolError)
//             {
//                 Debug.LogError($"Error sending TTS request: {www.error}");
//                 onGenerationComplete?.Invoke(null);
//                 yield break;
//             }

//             // Log raw response for debugging
//             string rawResponse = www.downloadHandler.text;
//             Debug.Log($"Raw Response: {rawResponse}");

//             // Parse response
//             TTSResponse response = null;
//             try
//             {
//                 response = JsonUtility.FromJson<TTSResponse>(rawResponse);
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"JSON Parsing Error: {e.Message}");
//                 onGenerationComplete?.Invoke(null);
//                 yield break;
//             }

//             // Handle response after parsing
//             if (response != null && !string.IsNullOrEmpty(response.file_url))
//             {
//                 // Download the audio file
//                 yield return StartCoroutine(DownloadAudioFileCoroutine(response.file_url, onGenerationComplete));
//             }
//             else
//             {
//                 Debug.LogError("Invalid response from TTS service");
//                 onGenerationComplete?.Invoke(null);
//             }
//         }
//     }

//  private IEnumerator DownloadAudioFileCoroutine(string fileUrl, TTSGenerationCompleteHandler onGenerationComplete)
// {
//     string fullUrl = fileUrl.StartsWith("http") ? fileUrl : $"{downloadBaseUrl}{fileUrl}";

//     using (UnityWebRequest www = UnityWebRequest.Get(fullUrl)) // Download raw file
//     {
//         yield return www.SendWebRequest();

//         if (www.result == UnityWebRequest.Result.Success)
//         {
//             // Save the downloaded file
//             string localFilePath = Path.Combine(Application.persistentDataPath, "tts_output.wav");
//             File.WriteAllBytes(localFilePath, www.downloadHandler.data);

//             Debug.Log($"Audio file saved to: {localFilePath}");

//             // // Play the downloaded audio file
//             // ProcessAudioClip(LoadAndPlayAudio(localFilePath));

//             // Invoke callback
//             onGenerationComplete?.Invoke(localFilePath);
//         }
//         else
//         {
//             Debug.LogError($"Error downloading audio: {www.error}");
//             onGenerationComplete?.Invoke(null);
//         }
//     }
// }

//     private void ProcessAudioClip(AudioClip audioClip)
//     {
//         if (audioClip != null)
//         {
//             AudioSource audioSource = GetComponent<AudioSource>();
//             if (audioSource == null)
//             {
//                 audioSource = gameObject.AddComponent<AudioSource>();
//             }
            
//             audioSource.clip = audioClip;
//             audioSource.Play();

//             Debug.Log($"Playing generated speech. Length: {audioClip.length} seconds");
//         }
//     }

//     public void OnSendTTSButtonClicked(string text)
//     {
//         SendTTSRequest(text);
//     }
// }