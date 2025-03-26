using UnityEngine;
using TMPro;
using UnityEngine.UI;  // For Button

public class EditableWorldTextZone : MonoBehaviour
{
    public TMP_Text textZone;  // The TMP_Text component in the world (Text Zone)
    private string currentText = "";  // Variable to hold the current text
    private bool isEditing = false;  // Flag to check if we're in edit mode
    private Camera mainCamera;
    public WorkflowDataRetriever WorkflowDataRetriever;
    //public ConversationManager ConversationManager;  // Reference to the ConversationManager script
    public Button sendButton;  // Reference to the send button (trigger custom function)

    void Start()
    {
        mainCamera = Camera.main;
        textZone.text = "Click to start typing...";

        // Set up button listener
        sendButton.onClick.AddListener(SendText);

        // Initially, the button won't do anything until text is typed
        sendButton.interactable = false;
    }

    void Update()
    {
        if (isEditing)
        {
            HandleTyping();
        }

        // If there's any text, enable the button
        if (!string.IsNullOrEmpty(currentText))
        {
            sendButton.interactable = true;
        }
        else
        {
            sendButton.interactable = false;  // Disable button when no text is typed
        }

        // Detect if the player clicks on the text zone to start editing
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == textZone.transform)
                {
                    StartEditing();
                }
            }
        }
    }

    // Start editing the text
    void StartEditing()
    {
        isEditing = true;
        textZone.text = "";  // Clear the text when editing starts
    }

    // Handle typing in the world text zone
    void HandleTyping()
    {
        foreach (char c in Input.inputString)
        {
            if (c == '\b')  // Backspace
            {
                if (currentText.Length > 0)
                {
                    currentText = currentText.Substring(0, currentText.Length - 1);
                }
            }
            else if (c == '\n')  // Enter key (optional: finish typing with enter)
            {
                // You could trigger something on Enter key if needed
            }
            else
            {
                currentText += c;  // Append typed character
            }
        }

        // Update the text zone with the current typed text
        textZone.text = currentText;
    }

    // Custom function triggered by the send button
    void SendText()
    {
        Debug.Log("Sending text: " + currentText);
        WorkflowDataRetriever.OnButtonClick(currentText);
        //LLMModelRequestHandler.OnSendButtonClicked(currentText);
        //ConversationManager.InitiateConversation(currentText);
        // Your custom logic to send the text elsewhere goes here
        // For example, send the text to a server, save it, etc.
    }
}
