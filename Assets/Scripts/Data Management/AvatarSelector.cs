using UnityEngine;
using UnityEngine.UI;

public class AvatarSelector : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup container;
    [SerializeField] HorizontalLayoutGroup rowPrefab;
    [SerializeField] Button avatarPrefab;
    [SerializeField] private int spritesPerRow;
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            Populate();
        }
    }

    public void Populate()
    {
        if (CardLoader.instance != null)
        {
            AvatarBank bank = CardLoader.instance.avatarBank;
            int count = 0;
            HorizontalLayoutGroup currentRow = null;
            for (int i = 1; i < bank.sprites.Count; i++)
            {
                Sprite sprite = bank.sprites[i];
                if (count > spritesPerRow)
                {
                    count = 0;
                }
                if (count == 0)
                {
                    currentRow = Instantiate(rowPrefab, container.transform);
                }
                Button newAvatar = Instantiate(avatarPrefab, currentRow.transform);
                newAvatar.onClick.AddListener(() => AssignAvatar(sprite.name));
                newAvatar.GetComponent<Image>().sprite = sprite;
                count++;
            }
            initialized = true;
        }
    }

    private void AssignAvatar(string avatar)
    {
        if (MultiplayerManagerV2.instance != null)
        {
            MultiplayerManagerV2.instance.SetAvatar(avatar);
            MultiplayerManagerV2.instance.ChangeMultiplayerState(MultiplayerManagerV2.MultiplayerState.none);
            gameObject.SetActive(false);
        }
    }
}
