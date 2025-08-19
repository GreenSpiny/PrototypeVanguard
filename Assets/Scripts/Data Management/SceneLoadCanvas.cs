using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadCanvas : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image image;
    [SerializeField] UnityEvent onLoad;
    [SerializeField] string unloadScene;
    
    [SerializeField] float fadeTransitionSpeed;
    [SerializeField] float colorTransitionSpeed;
    [SerializeField] public Color fadeOutColor;
    [SerializeField] public Color fadeInColor;
    
    [SerializeField] bool transitionOnStart;
    [SerializeField] bool escapeToQuit;

    private bool transitioningIn;
    private bool transitionInComplete;
    private bool transitioningOut;


    private void Awake()
    {
        if (transitionOnStart)
        {
            transitioningIn = true;
            canvasGroup.alpha = 0;
            image.color = fadeInColor;
        }
    }

    private void Update()
    {
        if (escapeToQuit && Input.GetKeyDown(KeyCode.Escape))
        {
            TransitionOut();
        }
        if (transitioningOut)
        {
            canvasGroup.alpha = Mathf.Clamp(canvasGroup.alpha - Time.deltaTime * fadeTransitionSpeed, 0, 1f);
            if (image != null)
            {
                image.color = Color.Lerp(image.color, fadeOutColor, Time.deltaTime * colorTransitionSpeed);
            }
            if (canvasGroup.alpha == 0)
            {
                transitioningOut = false;
                SceneManager.LoadScene(unloadScene);
            }
        }
        else if (transitioningIn && !transitionInComplete)
        {
            canvasGroup.alpha = Mathf.Clamp(canvasGroup.alpha + Time.deltaTime * fadeTransitionSpeed, 0, 1f);
            if (image != null)
            {
                image.color = Color.Lerp(image.color, fadeInColor, Time.deltaTime * colorTransitionSpeed);
            }
            if (canvasGroup.alpha == 1)
            {
                transitioningIn = false;
                transitionInComplete = true;
                if (onLoad != null)
                {
                    onLoad.Invoke();
                }
            }
        }
    }

    public void TransitionIn()
    {
        if (!transitioningIn && !transitionInComplete)
        {
            canvasGroup.blocksRaycasts = true;
            transitioningIn = true;
        }
    }

    public void TransitionOut()
    {
        if (!transitioningOut)
        {
            canvasGroup.blocksRaycasts = false;
            transitioningOut = true;
        }
    }

}
