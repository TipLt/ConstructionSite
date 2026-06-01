using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    public void Update()
    {
        ShowTimer();
    }

    public void ShowTimer()
    {
        timeText.SetText(Mathf.FloorToInt(GameManager.Instance.timeLimit).ToString());
    }
}
