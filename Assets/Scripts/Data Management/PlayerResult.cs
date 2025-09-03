using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResult : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] Button playButton;
    [SerializeField] Button kickButton;

    public string playerID { get; private set; }
    public string playerName { get; private set; }
    public string avatarName {  get; private set; }

    public void Initialize(string playerID)
    {
        this.playerID = playerID;
    }

    public void SetData(string playerName, string avatarName)
    {
        this.playerName = playerName;
        playerNameText.text = playerName;

        this.avatarName = avatarName;
    }

}
