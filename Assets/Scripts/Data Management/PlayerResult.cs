using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerResult : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] Button playButton;
    [SerializeField] Button kickButton;
    public string playerID { get; private set; }
    public int playerIndex { get; private set; }
    public string avatarName {  get; private set; }

    private void Start()
    {
        playButton.onClick.AddListener(() => MultiplayerManagerV2.instance.StartPlaying(playerID, playerNameText.text, avatarName));
        kickButton.onClick.AddListener(() => MultiplayerManagerV2.instance.KickPlayer(playerID));
    }

    public void Initialize(LobbyPlayerJoined player)
    {
        playerID = player.Player.Id;
        playerIndex = player.PlayerIndex;
        if (player.Player.Data != null)
        {
            if (player.Player.Data.ContainsKey("Name"))
            {
                playerNameText.text = player.Player.Data["Name"].Value;
            }
            if (player.Player.Data.ContainsKey("Avatar"))
            {
                avatarName = player.Player.Data["Avatar"].Value;
                icon.sprite = CardLoader.instance.avatarBank.GetSprite(avatarName);
            }
        }
    }

    /*
    public void UpdateData(Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>> data)
    {
        if (data.ContainsKey("Name") && (data["Name"].Changed || data["Name"].Added))
        {
            playerNameText.text = data["Name"].Value.Value;
        }
        if (data.ContainsKey("Avatar") && (data["Avatar"].Changed || data["Avatar"].Added))
        {
            avatarName = data["Avatar"].Value.Value;
            icon.sprite = CardLoader.instance.avatarBank.GetSprite(avatarName);
        }
    }
    */

}
