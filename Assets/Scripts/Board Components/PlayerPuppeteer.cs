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

                // Submit decklist
                CardInfo.DeckList randomDeck = CardInfo.CreateRandomDeck();
                GameManager.instance.SubmitDeckListRpc(playerIndex, "Random Deck", randomDeck.cardSleeves, randomDeck.mainDeck, randomDeck.rideDeck, randomDeck.strideDeck, randomDeck.toolbox);
            }
        }
    }

}
