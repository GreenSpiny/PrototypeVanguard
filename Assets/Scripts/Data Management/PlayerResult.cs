using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResult : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] Button playButton;
    [SerializeField] Button kickButton;

    private string playerID;

    public void Initialize(string playerID, string playerName)
    {
        this.playerID = playerID;
        this.playerName.text = playerName;
    }

}
