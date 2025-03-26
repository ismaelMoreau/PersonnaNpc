// using System;
// using System.Collections;
// using System.Diagnostics;
// using UnityEngine;
// using UnityEngine.UI;

// public class Audio2FaceTest : MonoBehaviour
// {
//     [Header("Environment Configuration")]
//     [Tooltip("Full path to the A2F virtual environment")]
//     public string a2fEnvironmentPath = "/path/to/a2f_venv";
    
//     [Tooltip("Full path to the a2f_3d.py script")]
//     public string a2fScriptPath = "/path/to/a2f_3d.py";

//     [Tooltip("Full path to directory")]
//     public string a2fDirectoryPath = "/path/to/a2f_3d.py";
    
//     [Tooltip("Full path to the test WAV file")]
//     public string testWavFile = "~/test.wav";
    
//     [Tooltip("Full path to the config file")]
//     public string configFile = "config/config_mark.yml";
    
//     [Tooltip("Server host and port")]
//     public string serverAddress = "0.0.0.0:52000";
    
//     [Header("UI Elements (Optional)")]
//     public Button testButton;
//     public Text statusText;
    
//     void Start()
//     {
//         StartCoroutine(RunA2FTest());
//         // Set up button listener if UI is configured
//         // if (testButton != null)
//         // {
//         //     testButton.onClick.AddListener(() => StartCoroutine(RunA2FTest()));
//         // }
//     }
    
//     // Method that can be called from a UI button or directly
//     public void StartTest()
//     {
//         StartCoroutine(RunA2FTest());
//     }
    
//     private IEnumerator RunA2FTest()
//     {
//         UpdateStatus("Starting Audio2Face test...");
        
//         string command = "/bin/bash";
//         string arguments = $"-c \"source {a2fEnvironmentPath}/bin/activate && cd {a2fDirectoryPath} && python3 {a2fScriptPath} run_inference {testWavFile} {configFile} -u {serverAddress}\"";
        
//         UpdateStatus("Running command: " + command + " " + arguments);
        
//         // Create and configure the process
//         Process process = new Process();
//         process.StartInfo.FileName = command;
//         process.StartInfo.Arguments = arguments;
//         process.StartInfo.UseShellExecute = false;
//         process.StartInfo.RedirectStandardOutput = true;
//         process.StartInfo.RedirectStandardError = true;
//         process.StartInfo.CreateNoWindow = true;
        
//         // Start the process
//         try
//         {
//             process.Start();
            
//             // Create a separate thread to read output to avoid blocking
//             System.Threading.Tasks.Task.Run(() => {
//                 string output = process.StandardOutput.ReadToEnd();
//                 string error = process.StandardError.ReadToEnd();
                
//                 // Log output and errors
//                 if (!string.IsNullOrEmpty(output))
//                 {
//                     UnityEngine.Debug.Log("A2F Output: " + output);
//                 }
                
//                 if (!string.IsNullOrEmpty(error))
//                 {
//                     UnityEngine.Debug.LogError("A2F Error: " + error);
//                 }
//             });
            
//             UpdateStatus("A2F process started. Check Unity console for output.");
//         }
//         catch (Exception e)
//         {
//             UpdateStatus("Error starting A2F process: " + e.Message);
//             UnityEngine.Debug.LogError("Error starting A2F process: " + e.Message);
//         }
        
//         yield return null;
//     }
    
//     private void UpdateStatus(string message)
//     {
//         UnityEngine.Debug.Log(message);
        
//         // Update UI if available
//         if (statusText != null)
//         {
//             statusText.text = message;
//         }
//     }
    
//     // Optional: Check if the process has completed
//     public bool IsProcessRunning(Process process)
//     {
//         try
//         {
//             Process.GetProcessById(process.Id);
//             return true;
//         }
//         catch
//         {
//             return false;
//         }
//     }
    
//     void OnDestroy()
//     {
//         if (testButton != null)
//         {
//             testButton.onClick.RemoveAllListeners();
//         }
//     }
// }