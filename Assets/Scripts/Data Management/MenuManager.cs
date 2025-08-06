using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    Animator mainAnimator;
    Image image;
    [SerializeField] Animator characterAnimator;
    [SerializeField] Image characterImage;
    [SerializeField] MenuButton[] menuButtons;
    [SerializeField] Image backgroundImage;
    [SerializeField] float colorTransitionSpeed;

    private Color originalColor;
    private Color targetColor;
    private System.Action transitionOutCallback;
    bool transitioningOut = false;

    private void Awake()
    {
        mainAnimator = GetComponent<Animator>();
        image = GetComponent<Image>();
        originalColor = image.color;
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TransitionIn();
        }
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
        transitioningOut = false;
        transitionOutCallback = null;
        mainAnimator.enabled = true;
        characterImage.sprite = null;
        mainAnimator.Play("Menu Enter");
    }

    public void TransitionOut(System.Action callback)
    {
        if (!transitioningOut)
        {
            targetColor = originalColor;
            transitioningOut = true;
            transitionOutCallback = callback;
            mainAnimator.enabled = true;
            mainAnimator.Play("Menu Exit");
            if (characterImage.sprite != null)
            {
                characterAnimator.Play("Disappear");
            }
        }
    }

    public void TransitionOutCompleteEvent()
    {
        if (transitionOutCallback != null)
        {
            transitionOutCallback.Invoke();
        }
    }
}
