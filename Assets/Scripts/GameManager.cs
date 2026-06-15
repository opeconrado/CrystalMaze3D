using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Interface")]
    public Text crystalText;
    public Text timerText;
    public Text messageText;
    public GameObject endPanel;

    [Header("Configuração da partida")]
    public float startTime = 90f;

    private int totalCrystals;
    private int collectedCrystals;
    private float timeRemaining;
    private bool gameEnded;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        timeRemaining = startTime;
        totalCrystals = FindObjectsOfType<CrystalCollectible>().Length;

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateUI();
    }

    void Update()
    {
        if (gameEnded)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            GameOver("Tempo esgotado!");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMenu();
        }

        UpdateUI();
    }

    public void CollectCrystal()
    {
        if (gameEnded)
        {
            return;
        }

        collectedCrystals++;
        UpdateUI();

        if (collectedCrystals >= totalCrystals)
        {
            WinGame();
        }
    }

    public void GameOver(string reason)
    {
        EndGame("Fim de jogo!\n" + reason);
    }

    public void WinGame()
    {
        EndGame("Você venceu!\nTodos os cristais foram coletados.");
    }

    void EndGame(string message)
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void UpdateUI()
    {
        if (crystalText != null)
        {
            crystalText.text = "Cristais: " + collectedCrystals + " / " + totalCrystals;
        }

        if (timerText != null)
        {
            timerText.text = "Tempo: " + Mathf.CeilToInt(timeRemaining);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
