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
    public int playerIndex { get; private set; }
    public string playerName { get; private set; }
    public string avatarName {  get; private set; }

    private void Start()
    {
        playButton.onClick.AddListener(() => MultiplayerManagerV2.instance.StartPlaying(playerID));
        kickButton.onClick.AddListener(() => MultiplayerManagerV2.instance.KickPlayer(playerID));
    }

    public void Initialize(string playerID, int playerIndex)
    {
        this.playerID = playerID;
        this.playerIndex = playerIndex;
    }

    public void SetData(string playerName, string avatarName)
    {
        this.playerName = playerName;
        playerNameText.text = playerName;

        this.avatarName = avatarName;
    }

}
