using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerPuppeteer : NetworkBehaviour
{
    private int playerIndex;
    private Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetupPlayerRpc(ulong clientID, int playerIndex)
    {
        if (NetworkManager.LocalClientId == clientID)
        {
            StartCoroutine(WaitForSetup(clientID, playerIndex));
        }
    }

    public IEnumerator WaitForSetup(ulong clientID, int playerIndex)
    {
        this.playerIndex = playerIndex;
        foreach (var player in GameManager.instance.players)
        {
            player.playerCamera.gameObject.SetActive(false);
        }
        if (playerIndex < 2)
        {
            // Align player and set control
            player = GameManager.instance.players[playerIndex];
            player.playerCamera.gameObject.SetActive(true);

            transform.position = player.playerCamera.transform.position;
            Node.cameraTransform = player.playerCamera.transform;

            foreach (var node in GameManager.instance.allNodes.Values)
            {
                node.SetDirty();
            }

            DragManager.instance.controllingPlayer = player;
            GameManager.instance.letterboxedCanvas.GetCameras()[1].camera = player.playerCamera;
            GameManager.instance.letterboxedCanvas.Refresh();
            GameManager.instance.boardOverlayCanvas.worldCamera = player.playerCamera;
            GameManager.instance.boardUnderlayCanvas.worldCamera = player.playerCamera;

            foreach (GameObject element in player.ownedUIRoots)
            {
                element.SetActive(true);
            }

            foreach (RectTransform rect in player.reversedUIRoots)
            {
                rect.localRotation = Quaternion.Euler(rect.localRotation.x, rect.localRotation.y, rect.localRotation.z + 180 * playerIndex);
            }

            // Wait for all card loading to be finished
            while (!CardLoader.instance.CardsLoaded)
            {
                yield return null;
            }

            // Submit decklist
            CardInfo.DeckList deckList;
            if (GameManager.localPlayerDecklist1 != null)
            {
                deckList = GameManager.localPlayerDecklist1;
            }
            else
            {
                deckList = CardInfo.CreateRandomDeck();
            }
            GameManager.instance.utilityButtons.Configure(playerIndex, deckList.toolbox.Length > 0);
            GameManager.instance.SetPlayerIcons(playerIndex, GameManager.localPlayerName, GameManager.localAvatar);
            GameManager.instance.SubmitDeckListToServerRpc(playerIndex, GameManager.localPlayerName, deckList.nation, deckList.mainDeck, deckList.rideDeck, deckList.strideDeck, deckList.toolbox);
        }
    }

}
