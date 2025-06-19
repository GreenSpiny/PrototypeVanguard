using System;
using UnityEngine;

// CARD represents a physical card in the game.
public class Card : MonoBehaviour
{
    [SerializeField] protected BoxCollider mainCollider;
    [SerializeField] protected BoxCollider nudgeCollider;
    public bool CollidersEnabled { get { return mainCollider.enabled; } }

    public const float cardWidth = 0.68605f;
    public const float cardHeight = 1f;
    public const float cardDepth = 0.01f;

    [NonSerialized] public Player player;
    [NonSerialized] public Node node;
    [NonSerialized] public CardInfo cardInfo;

    public Vector3 anchoredPosition;        // The intended position of the card
    public Vector3 anchoredPositionOffset;  // Offset of the final position, i.e. on hover
    public Vector3 lastAnchoredPosition;    // If applicable, the previous position of the card before a transition animation
    protected Vector3 targetEuler;          // The direction the card should face

    public bool isToken = false;
    public bool flipRotation = false;

    public enum CardUIState { normal, hovered, selected };
    protected CardUIState state;

    public bool anim = false;  // temporary
    public float AnimationSpeed { get { return 10f * Time.deltaTime; } }

    public void ToggleColliders(bool toggle)
    {
        mainCollider.enabled = toggle;
    }

    private void Awake()
    {
        SharedGamestate.allCards.Add(this);
    }
    private void Update()
    {
        // Animate position & rotation
        // If no animation exist, do a smooth lerp
        if (!anim)
        {
            transform.position = Vector3.Lerp(transform.position, node.transform.position + anchoredPosition + anchoredPositionOffset, AnimationSpeed);  
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetEuler), AnimationSpeed);
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
            //targetEuler.x = Mathf.Atan(Mathf.Abs(target.transform.position.y - transform.position.y) / Mathf.Abs(target.transform.position.z - transform.position.z)) * (180f / Mathf.PI) - 90f;
            targetEuler.x = Mathf.Atan(Mathf.Abs(target.transform.position.y - node.transform.position.y + anchoredPosition.y) / Mathf.Abs(target.transform.position.z - node.transform.position.z + anchoredPosition.z)) * (180f / Mathf.PI) - 90f;
        }
        if (flipRotation)
        {
            targetEuler.z = 180f;
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
                GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else if (state == CardUIState.hovered)
            {
                Nudge();
                GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.6f, 0.5f);
            }
            else if (state == CardUIState.selected)
            {
                Nudge();
                GetComponent<MeshRenderer>().material.color = new Color(0.6f, 0.5f, 0.5f);
            }
        }
    }

    protected void Nudge()
    {
        anchoredPositionOffset = transform.forward * node.NudgeDistance.z;
        nudgeCollider.enabled = node.NudgeDistance.z > 0.001;
    }
    protected void DeNudge()
    {
        anchoredPositionOffset = Vector3.zero;
        nudgeCollider.enabled = false;
    }
    
}
