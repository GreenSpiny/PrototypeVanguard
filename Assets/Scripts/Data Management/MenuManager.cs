using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class MenuManager : MonoBehaviour
{
    Animator mainAnimator;
    Image image;

    [SerializeField] Animator characterAnimator;
    [SerializeField] Image characterImage;
    [SerializeField] Image progressBarImage;

    [SerializeField] TextMeshProUGUI binaryVersionText;
    [SerializeField] TextMeshProUGUI cardsVersionText;

    [SerializeField] MenuButton[] menuButtons;
    [SerializeField] CanvasGroup menuButtonsGroup;
    [SerializeField] Image backgroundImage;
    [SerializeField] float colorTransitionSpeed;
    [SerializeField] float spriteHeightMultiplier;

    private Color originalColor;
    private Color targetColor;
    private System.Action transitionOutCallback;
    bool transitioningOut = false;

    private enum MenuState { none, loading, open }
    private MenuState state = MenuState.none;

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
        // If already loaded or testing locally, transition in immediately.
        if (CardLoader.instance != null && (CardLoader.instance.CardsLoaded || CardLoader.instance.downloadMode == CardLoader.DownloadMode.localResources))
        {
            TransitionIn(false);
        }
        // Otherwise, display the loading bar.
        else
        {
            state = MenuState.loading;
        }
    }

    private void Update()
    {
        if (state == MenuState.open)
        {
            backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, colorTransitionSpeed * Time.deltaTime);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TransitionIn(false);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
        }
        else if (state == MenuState.loading)
        {
            if (CardLoader.instance != null)
            {
                RectTransform rect = progressBarImage.rectTransform;
                progressBarImage.rectTransform.localScale = new Vector3(CardLoader.instance.imageDownloadProgress, rect.localScale.y, rect.localScale.z);
            }
            if (CardLoader.instance.CardsLoaded)
            {
                TransitionIn(true);
            }
        }
    }

    public void SetCharacterSprite(Color color, Sprite sprite, float height)
    {
        targetColor = color;
        if (characterImage.sprite != sprite)
        {
            characterImage.sprite = sprite;
            characterImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * spriteHeightMultiplier);
            characterAnimator.Play("Appear", -1, 0f);
        }
    }

    public void TransitionIn(bool fromLoader)
    {
        state = MenuState.open;
        transitioningOut = false;
        menuButtonsGroup.blocksRaycasts = true;
        transitionOutCallback = null;
        mainAnimator.enabled = true;
        characterImage.sprite = null;

        binaryVersionText.text = "engine - " + Application.version;
        cardsVersionText.text = "cards - rev. " + CardLoader.instance.dataVersionObject.cardsFileVersion.ToString();


        if (fromLoader)
        {
            mainAnimator.Play("Loader Exit");
        }
        else
        {
            mainAnimator.Play("Menu Enter");
        }
    }

    public void TransitionOut(System.Action callback)
    {
        if (!transitioningOut)
        {
            menuButtonsGroup.blocksRaycasts = false;
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

    // === Button Functions === //

    public void ToDeckBuilder()
    {
        TransitionOut(() => { SceneManager.LoadScene("DeckBuilderScene"); });
    }

    public void Quit()
    {
        TransitionOut(() => { Application.Quit(); });
    }


}
