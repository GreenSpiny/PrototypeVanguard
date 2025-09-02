using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System;

public class RoomResult : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI roomGameVersion;
    [SerializeField] private TextMeshProUGUI roomCardsVersion;
    [SerializeField] private Button joinButton;
    public uint CardsVersion { get; private set; }
    public Lobby lobby;

    public void Initialize(Lobby lobby)
    {
        this.lobby = lobby;
        roomName.text = lobby.Name;
        roomGameVersion.text = lobby.Data["GameVersion"].Value;
        roomCardsVersion.text = lobby.Data["CardsVersion"].Value;
        CardsVersion = Convert.ToUInt32(roomCardsVersion.text);
    }

    public void SetInteractable(bool interactable)
    {
        joinButton.gameObject.SetActive(VersionMatch && interactable);
    }
    private bool VersionMatch
    {
        get { return CardLoader.instance != null && CardLoader.instance.dataVersionObject.cardsFileVersion == CardsVersion; }
    }

}
