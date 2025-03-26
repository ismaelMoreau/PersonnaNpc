// using UnityEngine;
// using UnityEngine.Networking;
// using System;
// using System.Collections;
// using System.Text;

// public class LLMModelRequestHandler : MonoBehaviour
// {
//     [SerializeField] private string dockerServiceUrl = "http://localhost:8000/generate";

//     // Delegate for response handling
//     public delegate void ResponseHandler(string response);

//     // Struct to match the JSON request structure
//     [Serializable]
//     private class RequestData
//     {
//         public string question;
//     }

//     public void SendDockerRequest(string question, ResponseHandler onResponseReceived)
//     {
//         StartCoroutine(SendPostRequest(question, onResponseReceived));
//     }

//     private IEnumerator SendPostRequest(string question, ResponseHandler onResponseReceived)
//     {
//         // Create request data
//         RequestData requestData = new RequestData { question = question };
//         string jsonRequest = JsonUtility.ToJson(requestData);

//         // Convert JSON to byte array
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);

//         using (UnityWebRequest www = new UnityWebRequest(dockerServiceUrl, "POST"))
//         {
//             // Set up request
//             www.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             www.downloadHandler = new DownloadHandlerBuffer();
//             www.SetRequestHeader("Content-Type", "application/json");

//             // Send request and wait for response
//             yield return www.SendWebRequest();

//             // Check for network errors
//             if (www.result == UnityWebRequest.Result.ConnectionError || 
//                 www.result == UnityWebRequest.Result.ProtocolError)
//             {
//                 Debug.LogError($"Error sending request: {www.error}");
//                 Debug.LogError($"Response Code: {www.responseCode}");
//                 yield break;
//             }
            
//             try
//             {
//                 if (www.result == UnityWebRequest.Result.Success)
//                 {
//                     string rawResponse = www.downloadHandler.text;
//                     Debug.Log($"Raw Response: {rawResponse}");

//                     try
//                     {
//                         // Invoke the callback with the response
//                         onResponseReceived?.Invoke(rawResponse);
//                     }
//                     catch (Exception e)
//                     {
//                         Debug.LogError($"Error processing response: {e.Message}");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogError($"Request failed: {www.error}");
//                     // Optionally invoke callback with error message
//                     onResponseReceived?.Invoke($"Error: {www.error}");
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError($"Error processing response: {e.Message}");
//             }
//         }
//     }

//     private void ProcessResponse(string response)
//     {
//         // Simple processing - log the response
//         Debug.Log($"Processed Response: {response}");

//         // Add any specific processing logic here
//         // For example, update UI, trigger game events, etc.
//     }

//     // // Example method to trigger request from a UI button
//     // public void OnSendButtonClicked(string question)
//     // {
//     //     SendDockerRequest(question);
//     // }
// }