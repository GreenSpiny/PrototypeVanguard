using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class AnimatorEventUtility : MonoBehaviour
{
    public Animator anim;
    private Image image;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        image = GetComponent<Image>();
    }

    public void Close()
    {
        anim.enabled = false;
        if (image != null)
        {
            image.enabled = false;
        }
        for (int i = 2; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void SendGameStartEvent()
    {
        if (GameManager.instance.networkManager.IsHost)
        {
            GameManager.instance.RequestGameStartRpc();
        }
    }
}
