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

    [SerializeField] TextMeshProUGUI initialDownloadText;
    [SerializeField] TextMeshProUGUI downloadStatusText;
    private float downloadProgressAnimationSpeed = 5f;

    public static Color originalColor;
    private MenuButton hoveredButton;
    private Color targetColor;
    private System.Action transitionOutCallback;
    bool transitioningOut = false;

    private enum MenuState { none, loading, open }
    private MenuState state = MenuState.none;

    public bool ButtonsGroupIn { get { return menuButtonsGroup.alpha > .99f; } }

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
            progressBarImage.rectTransform.localScale = new Vector3(0, 1, 1);
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
                // float targetScale = CardLoader.instance.imageDownloadProgress;
                // float currentScale = Mathf.Clamp(rect.localScale.x + Time.deltaTime * downloadProgressAnimationSpeed, 0f, targetScale);
                rect.localScale = new Vector3(CardLoader.instance.imageDownloadProgress, 1, 1);

                if (CardLoader.instance.versionDownloadProgress > 0)
                {
                    initialDownloadText.gameObject.SetActive(CardLoader.instance.oldVersionNumber == 0);
                }
                if (CardLoader.instance.IsError && string.IsNullOrEmpty(downloadStatusText.text))
                {
                    downloadStatusText.text = "A connection error occurred. Internet is required for first load, and to receive new card upates.";
                    downloadStatusText.gameObject.SetActive(true);
                }
            }
            if (CardLoader.instance.CardsLoaded)
            {
                if (!CardLoader.instance.IsError)
                {
                    downloadStatusText.text = "Success!";
                    downloadStatusText.gameObject.SetActive(true);
                }
                TransitionIn(true);
            }
        }
    }

    public void SetCharacterSprite(MenuButton button)
    {
        if (hoveredButton != button)
        {
            if (ButtonsGroupIn)
            {
                targetColor = button.color;
            }
            hoveredButton = button;
            characterImage.sprite = button.sprite;
            characterImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, button.height * spriteHeightMultiplier);
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
            if (hoveredButton != null)
            {
                targetColor = hoveredButton.color;
            }
            menuButtonsGroup.blocksRaycasts = false;
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

    public void ToMultiplayer()
    {
        TransitionOut(() => { SceneManager.LoadScene("MatchingScene"); });
    }

    public void ToDeckBuilder()
    {
        TransitionOut(() => { SceneManager.LoadScene("DeckBuilderScene"); });
    }

    public void ToTutorial()
    {
        TransitionOut(() => { SceneManager.LoadScene("TutorialScene"); });
    }

    public void ToAbout()
    {
        TransitionOut(() => { SceneManager.LoadScene("AboutScene"); });
    }

    public void Quit()
    {
        TransitionOut(() => { Application.Quit(); });
    }


}
