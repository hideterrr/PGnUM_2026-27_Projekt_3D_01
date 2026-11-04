using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game HUD (Score, Timer, High Score) and Game Over screen with Restart button.
/// Features automatic fallback canvas generation if UI objects are not pre-linked in Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In-Game HUD References (Optional)")]
    public Text scoreText;
    public Text timerText;
    public Text highScoreText;

    [Header("Game Over Panel (Optional)")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text finalHighScoreText;
    public Text recordBannerText;
    public Text statsText;
    public Button restartButton;

    private Canvas canvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureUIExists();
    }

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"PUNKTY: {score}";
        }
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"REKORD: {highScore}";
        }
    }

    public void UpdateTime(float secondsRemaining)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(secondsRemaining / 60f);
            int seconds = Mathf.FloorToInt(secondsRemaining % 60f);
            timerText.text = $"CZAS: {minutes:00}:{seconds:00}";

            // Highlight in red when time is running out (< 10 seconds)
            if (secondsRemaining <= 10f)
            {
                timerText.color = new Color(1f, 0.25f, 0.25f);
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }

    public void ShowGameOverPanel(int score, int highScore, bool isNewRecord, int totalHits)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"TWÓJ WYNIK: {score}";
        }

        if (finalHighScoreText != null)
        {
            finalHighScoreText.text = $"NAJLEPSZY REKORD: {highScore}";
        }

        if (recordBannerText != null)
        {
            recordBannerText.gameObject.SetActive(isNewRecord);
            if (isNewRecord)
            {
                recordBannerText.text = "★ NOWY REKORD! ★";
                recordBannerText.color = new Color(1f, 0.85f, 0.1f);
            }
        }

        if (statsText != null)
        {
            statsText.text = $"Zestrzelone piłki: {totalHits}";
        }
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void OnRestartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    /// <summary>
    /// Programmatically generates a complete responsive Canvas UI if not set up in the Scene.
    /// </summary>
    private void EnsureUIExists()
    {
        if (scoreText != null && timerText != null && gameOverPanel != null)
        {
            return; // UI already configured
        }

        // 1. Create or find Canvas
#if UNITY_2023_1_OR_NEWER
        canvas = FindAnyObjectByType<Canvas>();
#else
        canvas = FindObjectOfType<Canvas>();
#endif
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("HUD_Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists for UI button clicks
#if UNITY_2023_1_OR_NEWER
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 2. HUD Top Bar
        if (scoreText == null)
        {
            GameObject scoreObj = CreateTextObject("ScoreText", canvas.transform, new Vector2(40, -40), new Vector2(0, 1), new Vector2(0, 1), TextAnchor.UpperLeft);
            scoreText = scoreObj.GetComponent<Text>();
            scoreText.font = defaultFont;
            scoreText.fontSize = 38;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.text = "PUNKTY: 0";
            scoreText.color = new Color(0.2f, 1f, 0.4f);
        }

        if (timerText == null)
        {
            GameObject timerObj = CreateTextObject("TimerText", canvas.transform, new Vector2(0, -40), new Vector2(0.5f, 1), new Vector2(0.5f, 1), TextAnchor.UpperCenter);
            timerText = timerObj.GetComponent<Text>();
            timerText.font = defaultFont;
            timerText.fontSize = 42;
            timerText.fontStyle = FontStyle.Bold;
            timerText.text = "CZAS: 00:45";
            timerText.color = Color.white;
        }

        if (highScoreText == null)
        {
            GameObject hsObj = CreateTextObject("HighScoreText", canvas.transform, new Vector2(-40, -40), new Vector2(1, 1), new Vector2(1, 1), TextAnchor.UpperRight);
            highScoreText = hsObj.GetComponent<Text>();
            highScoreText.font = defaultFont;
            highScoreText.fontSize = 32;
            highScoreText.fontStyle = FontStyle.Normal;
            highScoreText.text = "REKORD: 0";
            highScoreText.color = new Color(1f, 0.85f, 0.2f);
        }

        // 3. Game Over Panel
        if (gameOverPanel == null)
        {
            GameObject panelObj = new GameObject("GameOverPanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(650, 480);

            Image panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.08f, 0.1f, 0.15f, 0.94f);

            // Title
            GameObject titleObj = CreateTextObject("Title", panelObj.transform, new Vector2(0, 170), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            Text title = titleObj.GetComponent<Text>();
            title.font = defaultFont;
            title.fontSize = 50;
            title.fontStyle = FontStyle.Bold;
            title.text = "KONIEC GRY";
            title.color = Color.white;

            // Record Banner
            GameObject bannerObj = CreateTextObject("RecordBanner", panelObj.transform, new Vector2(0, 115), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            recordBannerText = bannerObj.GetComponent<Text>();
            recordBannerText.font = defaultFont;
            recordBannerText.fontSize = 28;
            recordBannerText.fontStyle = FontStyle.Bold;
            recordBannerText.text = "★ NOWY REKORD! ★";
            recordBannerText.color = new Color(1f, 0.85f, 0.1f);

            // Final Score
            GameObject fScoreObj = CreateTextObject("FinalScore", panelObj.transform, new Vector2(0, 55), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            finalScoreText = fScoreObj.GetComponent<Text>();
            finalScoreText.font = defaultFont;
            finalScoreText.fontSize = 36;
            finalScoreText.fontStyle = FontStyle.Bold;
            finalScoreText.text = "TWÓJ WYNIK: 0";
            finalScoreText.color = new Color(0.3f, 0.95f, 1f);

            // High Score
            GameObject fHsObj = CreateTextObject("FinalHighScore", panelObj.transform, new Vector2(0, 5), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            finalHighScoreText = fHsObj.GetComponent<Text>();
            finalHighScoreText.font = defaultFont;
            finalHighScoreText.fontSize = 28;
            finalHighScoreText.text = "NAJLEPSZY REKORD: 0";
            finalHighScoreText.color = new Color(0.8f, 0.8f, 0.8f);

            // Stats
            GameObject statsObj = CreateTextObject("Stats", panelObj.transform, new Vector2(0, -45), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            statsText = statsObj.GetComponent<Text>();
            statsText.font = defaultFont;
            statsText.fontSize = 24;
            statsText.text = "Zestrzelone piłki: 0";
            statsText.color = new Color(0.7f, 0.7f, 0.7f);

            // Restart Button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(panelObj.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -145);
            btnRect.sizeDelta = new Vector2(280, 65);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.65f, 0.35f, 1f);

            restartButton = btnObj.AddComponent<Button>();
            ColorBlock cb = restartButton.colors;
            cb.highlightedColor = new Color(0.24f, 0.8f, 0.45f);
            cb.pressedColor = new Color(0.12f, 0.5f, 0.25f);
            restartButton.colors = cb;

            GameObject btnTextObj = CreateTextObject("BtnText", btnObj.transform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), TextAnchor.MiddleCenter);
            Text btnText = btnTextObj.GetComponent<Text>();
            btnText.font = defaultFont;
            btnText.fontSize = 28;
            btnText.fontStyle = FontStyle.Bold;
            btnText.text = "ZAGRAJ PONOWNIE";
            btnText.color = Color.white;

            gameOverPanel = panelObj;
            gameOverPanel.SetActive(false);
        }
    }

    private GameObject CreateTextObject(string name, Transform parent, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(450, 70);

        Text txt = obj.AddComponent<Text>();
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        // Subtle drop shadow for crisp readability against 3D objects
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        return obj;
    }
}
