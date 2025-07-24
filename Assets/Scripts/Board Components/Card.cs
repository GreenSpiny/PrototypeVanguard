using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

// CARD represents a physical card in the game.
public class Card : MonoBehaviour
{
    [SerializeField] private BoxCollider mainCollider;
    [SerializeField] private BoxCollider nudgeCollider;
    [SerializeField] private MeshRenderer meshRenderer;
    public bool CollidersEnabled { get { return mainCollider.enabled; } }

    public const float cardWidth = 0.68605f;
    public const float cardHeight = 1f;
    public const float cardDepth = 0.005f;

    [NonSerialized] public Player player;
    [NonSerialized] public CardInfo cardInfo;

    [NonSerialized] public Vector3 anchoredPosition;        // The intended position of the card
    [NonSerialized] public Vector3 anchoredPositionOffset;  // Offset of the final position, i.e. on hover
    [NonSerialized] public Vector3 targetEuler;             // The direction the card should face

    [NonSerialized] public Node node;
    [NonSerialized] public bool isToken = false;

    public bool flip { get; private set; }
    public bool rest { get; private set; }

    private bool revealed = false;
    private float revealTime = 0f;
    private Coroutine revealCoroutine;
    [NonSerialized] public bool wasRevealed;    // If a card is revealed, this flag remains true until it changes nodes

    private Material cardFrontMaterial;
    private Material cardBackMaterial;
    private Material cardSideMaterial;

    public void SetTexture(Material mat, bool front)
    {
        if (front)
        {
            cardFrontMaterial = mat;
            name = cardFrontMaterial.mainTexture.name;
        }
        else
        {
            cardBackMaterial = mat;
        }
        meshRenderer.SetMaterials(new List<Material> { cardFrontMaterial, cardBackMaterial, cardSideMaterial });
    }
    public void SetOrientation(bool flip, bool rest)
    {
        this.flip = flip;
        this.rest = rest;
        node.SetDirty();
    }

    public int cardID { get; set; }     // Unique card identifier for networking purposes

    public enum CardUIState { normal, hovered, selected };
    private CardUIState state;

    private bool anim = false;  // temporary
    public float CardMoveSpeed { get { return 10f * Time.deltaTime; } }
    public float CardFlipSpeed { get { return 14f * Time.deltaTime; } }

    public void ToggleColliders(bool toggle)
    {
        mainCollider.enabled = toggle;
    }

    public void Init(Node node)
    {
        gameObject.SetActive(false);
        this.node = node;
        player = node.player;
        transform.SetParent(player.transform, true);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        SetOrientation(node.initialFlip, false);

        cardFrontMaterial = meshRenderer.materials[0];
        cardBackMaterial = meshRenderer.materials[1];
        cardSideMaterial = meshRenderer.materials[2];

        cardInfo = CardInfo.GenerateDefaultCardInfo(); // For testing purposes
    }

    public Texture GetTexture()
    {
        return cardFrontMaterial.mainTexture;
    }

    public void SetRevealed(bool reveal, float revealDuration)
    {
        if (reveal)
        {
            revealTime = Mathf.Max(revealDuration, revealTime);
            revealCoroutine = StartCoroutine(RevealCoroutine());
        }
        else
        {
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
            }
            revealed = false;
            revealTime = 0;
        }
        node.SetDirty();
    }

    private IEnumerator RevealCoroutine()
    {
        revealed = true;
        wasRevealed = true;
        while (revealTime > 0f)
        {
            yield return null;
            revealTime -= Time.deltaTime;
        }
        revealed = false;
        node.SetDirty();
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
        if (rest) // rotate resting cards
        {
            targetEuler.y += 90f;
        }
        if (DragManager.instance.controllingPlayer != null) // rotate cards to face the active player
        {
            targetEuler.y += 180f * (DragManager.instance.controllingPlayer.playerIndex);
        }
        if (flip) // if a card is in the flip state, turn it upside down from its node's normal orientation
        {
            targetEuler.z += 180f;
        }

        // Face cards away from players who should not see them
        else if (!revealed && node.privateKnowledge && player != DragManager.instance.controllingPlayer)
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

    public void ResetPower()
    {
        if (cardInfo.powerModifier != 0 || cardInfo.critModifier != 0 || cardInfo.driveModifier != 0)
        {
            cardInfo.powerModifier = 0;
            cardInfo.critModifier = 0;
            cardInfo.driveModifier = 0;
            if (node.NodeUI != null)
            {
                node.NodeUI.needsPulse = true;
                node.SetDirty();
            }
        }
    }

    public void EditPower(int powerModifier, int critModifier, int driveModifier)
    {
        if (powerModifier != 0 || critModifier != 0 || driveModifier != 0)
        {
            cardInfo.powerModifier += powerModifier;
            cardInfo.critModifier += critModifier;
            cardInfo.driveModifier += driveModifier;
            if (node.NodeUI != null)
            {
                node.NodeUI.needsPulse = true;
                node.SetDirty();
            }
        }
    }
    
}
