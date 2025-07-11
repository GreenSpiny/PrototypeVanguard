using System;
using UnityEngine;

// CARD represents a physical card in the game.
public class Card : MonoBehaviour
{
    [SerializeField] protected BoxCollider mainCollider;
    [SerializeField] protected BoxCollider nudgeCollider;
    [SerializeField] protected MeshRenderer meshRenderer;
    public bool CollidersEnabled { get { return mainCollider.enabled; } }

    public const float cardWidth = 0.68605f;
    public const float cardHeight = 1f;
    public const float cardDepth = 0.005f;

    [NonSerialized] public Player player;
    [NonSerialized] public Node node;
    [NonSerialized] public CardInfo cardInfo;

    [NonSerialized] public Vector3 anchoredPosition;        // The intended position of the card
    [NonSerialized] public Vector3 anchoredPositionOffset;  // Offset of the final position, i.e. on hover
    [NonSerialized] public Vector3 targetEuler;             // The direction the card should face

    [NonSerialized] public bool isToken = false;
    [NonSerialized] public bool rest = false;
    [NonSerialized] public bool flipRotation = false;

    protected Material cardFrontMaterial;
    protected Material cardBackMaterial;

    public enum CardUIState { normal, hovered, selected };
    protected CardUIState state;

    private bool anim = false;  // temporary
    public float CardMoveSpeed { get { return 10f * Time.deltaTime; } }
    public float CardFlipSpeed { get { return 14f * Time.deltaTime; } }

    public void ToggleColliders(bool toggle)
    {
        mainCollider.enabled = toggle;
    }

    private void Awake()
    {
        cardFrontMaterial = meshRenderer.materials[0];
        cardBackMaterial = meshRenderer.materials[1];
    }
    private void Update()
    {
        // Animate position & rotation
        // If no animation exist, do a smooth lerp
        if (!anim && node != null)
        {
            transform.position = Vector3.Lerp(transform.position, node.cardAnchor.transform.position + anchoredPosition + anchoredPositionOffset, CardMoveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetEuler), CardFlipSpeed);
            transform.localScale = Vector3.Lerp(transform.localScale, node.cardScale, CardMoveSpeed);
        }
        // If an animation exists, follow the procedure
        else
        {

        }
    }

    public void LookAt(Transform target)
    {
        targetEuler = Vector3.zero;
        if (target != null)
        {
            targetEuler.x = Mathf.Atan(Mathf.Abs(target.transform.position.y - node.transform.position.y + anchoredPosition.y) / Mathf.Abs(target.transform.position.z - node.transform.position.z + anchoredPosition.z)) * (180f / Mathf.PI) - 90f;
        }
        else
        {
            targetEuler = node.cardRotation;
        }
        if (rest)
        {
            targetEuler.y += 90f;
        }
        if (flipRotation)
        {
            targetEuler.z += 180f;
        }
        if (node.Type == Node.NodeType.hand && !player.isActivePlayer)
        {
            targetEuler.z += 180f;
        }
    }

    public CardUIState UIState
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if (state == CardUIState.normal)
            {
                DeNudge();
                cardFrontMaterial.color = Color.white;
                cardBackMaterial.color = Color.white;
            }
            else if (state == CardUIState.hovered)
            {
                Nudge();
                cardFrontMaterial.color = new Color(1f, 1f, 0.5f);
                cardBackMaterial.color = new Color(1f, 1f, 0.5f);
            }
            else if (state == CardUIState.selected)
            {
                Nudge();
                cardFrontMaterial.color = new Color(0.5f, 0.75f, 1f);
                cardBackMaterial.color = new Color(0.5f, 0.75f, 1f);
            }
        }
    }

    protected void Nudge()
    {
        Vector3 anchoredPositionOffsetX = transform.right * node.nudgeDistance.x * node.cardScale.x;
        Vector3 anchoredPositionOffsetY = transform.up * node.nudgeDistance.y * node.cardScale.y;
        Vector3 anchoredPositionOffsetZ = transform.forward * node.nudgeDistance.z * node.cardScale.z;
        anchoredPositionOffset = anchoredPositionOffsetX + anchoredPositionOffsetY + anchoredPositionOffsetZ;
        nudgeCollider.enabled = anchoredPositionOffset.magnitude > 0.0001f;
    }
    protected void DeNudge()
    {
        anchoredPositionOffset = Vector3.zero;
        nudgeCollider.enabled = false;
    }
    
}
