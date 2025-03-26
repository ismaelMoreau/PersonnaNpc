using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System.Text;

public class WorkflowDataRetriever : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer characterMeshRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Audio2FaceBlendshapePlayer blendshapePlayer;

    [System.Serializable]
    public class WorkflowStartResponse
    {
        public string workflow_id;
        public string text_response;
        public string speech_path;
        public string status;
    }

    // Base URL for your FastAPI server
    private const string BASE_URL = "http://localhost:8020";

    // References to other components
    // private Audio2FaceBlendshapePlayer blendshapePlayer;

    void Awake()
    {
        // Ensure we have the blendshape player component
        // blendshapePlayer = GetComponent<Audio2FaceBlendshapePlayer>();
        
        // // If not attached, add the component
        // if (blendshapePlayer == null)
        // {
        //     blendshapePlayer = gameObject.AddComponent<Audio2FaceBlendshapePlayer>();
        // }

        // // Configure blendshape player
        // blendshapePlayer.skinnedMeshRenderer = characterMeshRenderer;
        // blendshapePlayer.audioSource = audioSource;
    }

    // Method to start the workflow
    public void StartWorkflow(string question)
    {
        StartCoroutine(StartWorkflowRoutine(question));
    }

    [System.Serializable]
    public class WorkflowRequest
    {
        public string question;
    }

    private IEnumerator StartWorkflowRoutine(string question)
    {
        // Create and serialize the request object
        WorkflowRequest requestData = new WorkflowRequest { question = question };
        string jsonData = JsonUtility.ToJson(requestData);

        Debug.Log($"Sending JSON: {jsonData}"); // Debug output to verify JSON structure

        using (UnityWebRequest www = new UnityWebRequest($"{BASE_URL}/start_workflow", "POST"))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(jsonBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                WorkflowStartResponse response = 
                    JsonUtility.FromJson<WorkflowStartResponse>(www.downloadHandler.text);

                StartCoroutine(ProcessWorkflowData(response.workflow_id));
            }
            else
            {
                Debug.LogError($"Workflow start failed: {www.responseCode} {www.error} - {www.downloadHandler.text}");
            }
        }
    }


   private IEnumerator ProcessWorkflowData(string workflowId)
    {
        // Retrieve Audio
        AudioClip audioClip = null;
        string csvContent = null;

        // Get Audio Stream
        using (UnityWebRequest www = UnityWebRequest.Get($"{BASE_URL}/audio_stream/{workflowId}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Save audio to temporary location
                string tempAudioPath = Path.Combine(Application.temporaryCachePath, $"temp_audio_{workflowId}.wav");
                File.WriteAllBytes(tempAudioPath, www.downloadHandler.data);

                // Load AudioClip
                using (UnityWebRequest audioLoad = UnityWebRequestMultimedia.GetAudioClip($"file://{tempAudioPath}", AudioType.WAV))
                {
                    yield return audioLoad.SendWebRequest();

                    if (audioLoad.result == UnityWebRequest.Result.Success)
                    {
                        audioClip = DownloadHandlerAudioClip.GetContent(audioLoad);
                    }
                }
            }
        }

        // Get Animation Frames CSV as byte stream
        using (UnityWebRequest www = UnityWebRequest.Get($"{BASE_URL}/animation_frames/{workflowId}"))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Convert byte stream directly to string
                csvContent = Encoding.UTF8.GetString(www.downloadHandler.data);
            }
        }

        // Check if both audio and CSV are retrieved successfully
        if (audioClip != null && !string.IsNullOrEmpty(csvContent))
        {
            // Create a temporary CSV file
            // string csvPath = Path.Combine(Application.temporaryCachePath, $"animation_frames_{workflowId}.csv");
            // File.WriteAllText(csvPath, csvContent);

            // Initialize blendshape synchronization
            blendshapePlayer.InitializeSynchronization(csvContent, audioClip);
        }
        else
        {
            Debug.LogError("Failed to retrieve audio or CSV data");
        }
    }

    // Example method to trigger workflow from elsewhere in your Unity script
    public void OnButtonClick(string question)
    {
        StartWorkflow(question);
    }   
}