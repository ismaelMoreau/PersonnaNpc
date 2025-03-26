// using UnityEngine;
// using System;
// using System.Collections;
// using System.Diagnostics;
// using System.IO;

// public class ConversationManager : MonoBehaviour
// {
//     [Header("LLM and Speech Components")]
//     [SerializeField] private LLMModelRequestHandler llmHandler;
//     [SerializeField] private PiperTTSRequestHandler ttsHandler;

//     [Header("Audio2Face Configuration")]
//     [SerializeField] private string a2fbasePath = "/path/to/a2f";
//     [SerializeField] private string a2fScriptName = "a2f_3d.py";
//     [SerializeField] private string serverAddress = "0.0.0.0:52000";


//     [Header("Blendshape Animation")]
//     [SerializeField] private Audio2FaceBlendshapePlayer blendshapePlayer;
//     [SerializeField] private SkinnedMeshRenderer characterMeshRenderer;
//     [SerializeField] private AudioSource characterAudioSource;

//     [Header("UI Elements")]
//     public UnityEngine.UI.Text conversationStatusText;

//     // Tracking variables
//     private Process currentA2FProcess;
//     private string currentWavFilePath;
//     private string currentCSVAnimationPath;

//     // Main conversation initiation method
//     public void InitiateConversation(string userInput)
//     {
//         // Reset any previous state
//         ResetConversationState();

//         // Send input to LLM
//         llmHandler.SendDockerRequest(userInput, OnLLMResponseReceived);
//     }

//     // Callback when LLM responds
//     private void OnLLMResponseReceived(string llmResponse)
//     {
//         // Clean and log the response
//         string cleanedResponse = CleanupResponseText(llmResponse);
//         UpdateStatusText($"LLM Response: {cleanedResponse}");

//         // Generate speech from the LLM response
//         GenerateSpeechFromResponse(cleanedResponse);
//     }

//     // Generate speech and prepare for Audio2Face
//     private void GenerateSpeechFromResponse(string response)
//     {
//         // Use TTS handler with a callback for when audio is ready
//         ttsHandler.SendTTSRequest(response, OnTTSGenerationComplete);
//     }

//     // Callback when TTS audio is generated
//     private void OnTTSGenerationComplete(string wavFilePath)
//     {
//         UpdateStatusText($"Speech generated: {wavFilePath}");
        
//         // Store the current wav file path
//         currentWavFilePath = wavFilePath;

//         // Start Audio2Face processing
//         StartCoroutine(RunAudio2FaceProcessing());
//     }

//     // Process audio with Audio2Face
//     private IEnumerator RunAudio2FaceProcessing()
//     {
//         UpdateStatusText("Starting Audio2Face processing...");

//         // Prepare A2F command
//         string command = "/bin/bash";
//         string arguments = $"-c \"source {a2fbasePath}/.venv/bin/activate && " +
//                            $"cd {a2fbasePath} && " +
//                            $"python3 {a2fbasePath}/{a2fScriptName} run_inference {currentWavFilePath} " +
//                            $"config/config_mark.yml -u {serverAddress}\"";

//         UpdateStatusText($"Executing: {command} {arguments}");

//         // Create and configure the process
//         currentA2FProcess = new Process();
//         currentA2FProcess.StartInfo.FileName = command;
//         currentA2FProcess.StartInfo.Arguments = arguments;
//         currentA2FProcess.StartInfo.UseShellExecute = false;
//         currentA2FProcess.StartInfo.RedirectStandardOutput = true;
//         currentA2FProcess.StartInfo.RedirectStandardError = true;
//         currentA2FProcess.StartInfo.CreateNoWindow = true;

//         try
//         {
//             // Start the process
//             currentA2FProcess.Start();

//             // Wait for process to complete
//             currentA2FProcess.WaitForExit();

//             // Find the most recent CSV file in the A2F output directory
//             string outputDir = Path.Combine(a2fbasePath, "output");
//             string[] csvFiles = Directory.GetFiles(outputDir, "*.csv", SearchOption.TopDirectoryOnly);
            
//             if (csvFiles.Length > 0)
//             {
//                 // Get the most recently created CSV file
//                 Array.Sort(csvFiles, (a, b) => File.GetCreationTime(b).CompareTo(File.GetCreationTime(a)));
//                 currentCSVAnimationPath = csvFiles[0];

//                 // Start blendshape synchronization
//                 StartBlendshapeSynchronization();
//             }
//             else
//             {
//                 UpdateStatusText("No CSV animation file found!");
//             }

//             UpdateStatusText("Audio2Face processing completed");
//         }
//         catch (Exception e)
//         {
//             UpdateStatusText($"A2F Processing Error: {e.Message}");
//         }

//         yield return null;
//     }
//     private void StartBlendshapeSynchronization()
// {
//     if (blendshapePlayer == null)
//     {
//         GameObject blendshapePlayerObj = new GameObject("A2FSync");
//         blendshapePlayer = blendshapePlayerObj.AddComponent<Audio2FaceBlendshapePlayer>();
//     }

//     // Load audio clip directly
//     AudioClip audioClip = AudioClip.Load(currentWavFilePath);
    
//     // Configure and start synchronization
//     blendshapePlayer.skinnedMeshRenderer = characterMeshRenderer;
//     blendshapePlayer.audioSource = characterAudioSource;
//     blendshapePlayer.InitializeSynchronization(currentCSVAnimationPath, audioClip);

//     UpdateStatusText("Blendshape Synchronization Started");
// }
//     // Cleanup method
//     private void ResetConversationState()
//     {
//         // Stop any ongoing processes
//         if (currentA2FProcess != null && !currentA2FProcess.HasExited)
//         {
//             try
//             {
//                 currentA2FProcess.Kill();
//             }
//             catch { }
//         }

//         // Clear previous file paths
//         currentWavFilePath = string.Empty;
//     }

//     // Utility method to clean response text
//     private string CleanupResponseText(string originalResponse)
//     {
//         // Remove the 'response':' prefix
//         string cleanedResponse = originalResponse.Replace("'response':", "").Replace("\"response\":", "");
        
//         // Remove the leading number and newline
//         cleanedResponse = cleanedResponse.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\n');
        
//         // Remove the special end-of-sentence marker
//         cleanedResponse = cleanedResponse.Replace("<｜end▁of▁sentence｜>", "");
        
//         // Remove asterisks (used for action/emotion descriptions)
//         cleanedResponse = System.Text.RegularExpressions.Regex.Replace(cleanedResponse, @"\*[^*]*\*", "");
        
//         // Trim whitespace and remove extra spaces
//         cleanedResponse = System.Text.RegularExpressions.Regex.Replace(cleanedResponse, @"\s+", " ").Trim();
        
//         // Remove surrounding quotes if present
//         cleanedResponse = cleanedResponse.Trim('"', '\'');
        
//         return cleanedResponse;
//     }

//     // Status update method
//     private void UpdateStatusText(string message)
//     {
//         UnityEngine.Debug.Log(message);
        
//         if (conversationStatusText != null)
//         {
//             conversationStatusText.text = message;
//         }
//     }

//     // Cleanup on object destruction
//     void OnDestroy()
//     {
//         ResetConversationState();
//     }
// }

