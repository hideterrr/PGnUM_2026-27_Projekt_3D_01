using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Core Game Manager coordinating time, scoring, ranking (PlayerPrefs),
/// input processing (touch & mouse shooting), and game-over state.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Mode Configuration")]
    [Tooltip("Total round duration in seconds.")]
    public float gameDuration = 45.0f;

    [Header("Current State")]
    public int currentScore = 0;
    public int highScore = 0;
    public float timeRemaining { get; private set; }
    public bool IsGameActive { get; private set; } = false;

    [Header("Statistics")]
    public int totalHits = 0;
    public int bonusHits = 0;
    public int hazardHits = 0;

    private const string HIGH_SCORE_KEY = "BallShooter_HighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Load persisted high score for ranking
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        currentScore = 0;
        totalHits = 0;
        bonusHits = 0;
        hazardHits = 0;
        timeRemaining = gameDuration;
        IsGameActive = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateHighScore(highScore);
            UIManager.Instance.UpdateTime(timeRemaining);
            UIManager.Instance.HideGameOverPanel();
        }
    }

    private void Update()
    {
        if (!IsGameActive) return;

        // Countdown timer
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTime(timeRemaining);
        }

        // Handle raycast click/tap shooting for mobile and PC
        ProcessShootingInput();
    }

    private void ProcessShootingInput()
    {
        bool shotTriggered = false;
        Vector2 screenPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            shotTriggered = true;
            screenPosition = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            shotTriggered = true;
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (!shotTriggered && Input.GetMouseButtonDown(0))
        {
            shotTriggered = true;
            screenPosition = Input.mousePosition;
        }
#endif

        if (shotTriggered)
        {
            PerformRaycastShoot(screenPosition);
        }
    }

    private void PerformRaycastShoot(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Ball ball = hit.collider.GetComponent<Ball>();
            if (ball != null)
            {
                ball.Hit();
            }
        }
    }

    /// <summary>
    /// Invoked whenever any ball is shot / clicked.
    /// </summary>
    public void OnBallHit(Ball ball, Vector3 hitPosition)
    {
        if (!IsGameActive) return;

        totalHits++;
        if (ball.ballType == BallType.Bonus) bonusHits++;
        if (ball.ballType == BallType.Hazard) hazardHits++;

        // Update score (score cannot drop below 0)
        currentScore = Mathf.Max(0, currentScore + ball.scoreValue);

        // Spawn 3D floating score popup
        ScorePopup.Create(hitPosition, ball.scoreValue);

        // Update High Score live if surpassed
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
            UIManager.Instance.UpdateHighScore(highScore);
        }
    }

    public void EndGame()
    {
        if (!IsGameActive) return;
        IsGameActive = false;

        // Save High Score to PlayerPrefs for persistent ranking
        bool isNewRecord = false;
        int savedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (currentScore > savedHighScore)
        {
            isNewRecord = true;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, currentScore);
            PlayerPrefs.Save();
            highScore = currentScore;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel(currentScore, highScore, isNewRecord, totalHits);
        }
    }

    public void RestartGame()
    {
        // Reload active scene to reset everything cleanly
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
