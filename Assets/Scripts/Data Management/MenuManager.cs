using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    Animator mainAnimator;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Image characterImage;
    [SerializeField] MenuButton[] menuButtons;
    [SerializeField] Image backgroundImage;
    [SerializeField] float colorTransitionSpeed;
    
    private Color targetColor;
    private System.Action transitionOutCallback;
    bool transitioningOut = false;

    private void Awake()
    {
        mainAnimator = GetComponent<Animator>();
        targetColor = backgroundImage.color;
        foreach (var button in menuButtons)
        {
            button.Init(this);
        }
    }

    private void Start()
    {
        if (true || CardLoader.instance.CardsLoaded) // todo - wait on first load
        {
            TransitionIn();
        }
    }

    private void Update()
    {
        backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, colorTransitionSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TransitionOut(() => { Application.Quit(); });
        }
    }

    public void SetCharacterSprite(Color color, Sprite sprite)
    {
        targetColor = color;
        if (characterImage.sprite != sprite)
        {
            characterImage.sprite = sprite;
            characterAnimator.Play("Appear", -1, 0f);
        }
    }

    public void TransitionIn()
    {
        transitionOutCallback = null;
        mainAnimator.enabled = true;
        mainAnimator.Play("Menu Enter");
    }

    public void TransitionOut(System.Action callback)
    {
        if (!transitioningOut)
        {
            transitioningOut = true;
            transitionOutCallback = callback;
            mainAnimator.enabled = true;
            mainAnimator.Play("Menu Exit");
            characterAnimator.Play("Disappear");
        }
    }

    public void TransitionOutCompleteEvent()
    {
        transitioningOut = false;
        if (transitionOutCallback != null)
        {
            transitionOutCallback.Invoke();
        }
    }
}
