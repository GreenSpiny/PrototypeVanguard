using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player ActivePlayer { get; private set; }
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected Node_Drag dragNode;

    private void Awake()
    {
        Player.ActivePlayer = this;
    }

}
