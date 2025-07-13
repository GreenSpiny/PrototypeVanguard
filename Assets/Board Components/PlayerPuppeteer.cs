using Unity.Netcode;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

public class PlayerPuppeteer : NetworkBehaviour
{
    public NetworkVariable<int> playerIndex = new NetworkVariable<int>();
    public Player player;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SetupPlayerRpc()
    {
        Debug.Log(playerIndex.Value);
        foreach (var player in GameManager.instance.players)
        {
            player.playerCamera.gameObject.SetActive(false);
        }
        if (playerIndex.Value < 2)
        {
            player = GameManager.instance.players[playerIndex.Value];
            player.playerCamera.gameObject.SetActive(true);
            transform.position = player.playerCamera.transform.position;
            DragManager.instance.controllingPlayer = player;

            GameManager.instance.letterboxedCanvas.GetCameras()[1].camera = player.playerCamera;
            GameManager.instance.letterboxedCanvas.Refresh();

        }
    }

}
