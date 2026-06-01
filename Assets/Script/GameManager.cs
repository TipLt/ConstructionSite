using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float timeLimit = 30f;
    public bool isFinished = false;
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance != null)
                {
                    GameObject gameObject = new GameObject();
                    instance = gameObject.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    public void Update()
    {
        if (!isFinished)
        {
            timeLimit -= Time.deltaTime;
            if (timeLimit <= 0)
            {
                EndGame();
            }
        }
    }

    public void EndGame()
    {
        isFinished = true;
    }
}
