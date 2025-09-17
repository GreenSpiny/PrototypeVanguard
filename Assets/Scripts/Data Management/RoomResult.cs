using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System;

public class RoomResult : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI roomGameVersion;
    [SerializeField] private TextMeshProUGUI roomCardsVersion;
    [SerializeField] private TextMeshProUGUI roomCode;
    [SerializeField] private TextMeshProUGUI roomCodeLabel;
    [SerializeField] private TextMeshProUGUI ownerMessage;
    [SerializeField] private Button joinButton;

    [SerializeField] private Color ownerColor;

    private uint cardsVersion;
    public string Code { get; private set; }
    public Lobby lobby;

    public void Start()
    {
        joinButton.onClick.AddListener(() => MultiplayerManagerV2.instance.StartLeechingAsync(this));
    }

    public void Initialize(Lobby lobby, bool isOwner)
    {
        this.lobby = lobby;

        roomName.text = lobby.Name;
        roomGameVersion.text = lobby.Data["GameVersion"].Value;
        roomCardsVersion.text = lobby.Data["CardsVersion"].Value;
        roomCode.text = lobby.Data["RoomCode"].Value;

        Code = roomCode.text;
        cardsVersion = Convert.ToUInt32(roomCardsVersion.text);
        if (CardLoader.CardsLoaded)
        {
            avatar.sprite = CardLoader.instance.avatarBank.GetSprite(lobby.Data["Avatar"].Value);
        }
        roomCode.gameObject.SetActive(isOwner);
        roomCodeLabel.gameObject.SetActive(isOwner);
        ownerMessage.gameObject.SetActive(isOwner);
        if (isOwner)
        {
            background.color = ownerColor;
        }
    }

    public void SetInteractable(bool interactable)
    {
        joinButton.gameObject.SetActive(VersionMatch && interactable);
    }
    private bool VersionMatch
    {
        get { return CardLoader.CardsLoaded && CardLoader.instance.dataVersionObject.cardsFileVersion == cardsVersion; }
    }

}
