using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Multiplayer testing class ported from the official Unity tutorial

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager netManager;

    private void Awake()
    {
        netManager = GetComponent<NetworkManager>();
        netManager.OnConnectionEvent += OnConnectionOverride;
    }

    void Start()
    {
        if (Application.isEditor)
        {
            return;
        }
        var args = GetCommandlineArgs();
        if (args.TryGetValue("-mode", out string mode))
        {
            switch (mode)
            {
                case "server":
                    netManager.StartServer();
                    break;
                case "host":
                    netManager.StartHost();
                    break;
                case "client":
                    netManager.StartClient();
                    break;
            }
        }
    }

    public void OnConnectionOverride(NetworkManager manager, ConnectionEventData data)
    {
        if (manager.IsServer)
        {
            var clientId = data.ClientId;
            //var playerPrefab = manager.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerPuppetteer>();
            //playerPrefab.AssignPlayerRpc(playerCount.Value);
            //playerCount.Value++;
        }
    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!netManager.IsClient && !netManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            // StatusLabels();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if (GUILayout.Button("Host")) netManager.StartHost();
        if (GUILayout.Button("Client")) netManager.StartClient();
        if (GUILayout.Button("Server")) netManager.StartServer();
    }

    private void StatusLabels()
    {
        var mode = netManager.IsHost ?
            "Host" : netManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            netManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }
}
