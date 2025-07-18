using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chatbox : MonoBehaviour
{
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private TextMeshProUGUI chatRecord;
    private TMP_InputField inputField;
    private const int maxInputCharacters = 1000;
    private const int maxRecordCharacters = 100000;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(inputField.text) && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            string message = inputField.text.Trim(new char[] {' ', '\n', '\r', '\t' });
            if (message.Length > maxInputCharacters)
            {
                message = message.Substring(0, maxInputCharacters);
            }
            GameManager.instance.RequestSendChatMessageRpc(DragManager.instance.controllingPlayer.playerIndex, message);
            inputField.text = string.Empty;
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void RecieveMessage(int playerID, string message)
    {
        string preMessage = "<b>";
        if (!string.IsNullOrEmpty(chatRecord.text))
        {
            preMessage += "\n";
        }
        preMessage += GameManager.instance.players[playerID].name + ": </b>";
        string newChatRecord = chatRecord.text + preMessage + message;
        if (newChatRecord.Length > maxRecordCharacters)
        {
            newChatRecord = newChatRecord.Substring(newChatRecord.Length - maxRecordCharacters);
        }
        chatRecord.text = newChatRecord;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatRecord.rectTransform);
    }

}
