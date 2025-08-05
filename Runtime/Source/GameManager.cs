using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages game state, score, and game over logic. Handles UI updates and scene restarts.
/// </summary>
public sealed class GameManager : MonoBehaviour
{
    private int _currentScore;                                          // Current player score

    [SerializeField]
    private GameObject _gameOverPanel;                                  // UI panel shown on game over

    [SerializeField]
    private TextMeshProUGUI _score;                                     // UI text displaying the score

    private static GameManager _instance;                               // Singleton instance
    public static GameManager Instance => _instance;                    // Public accessor for singleton

    /// <summary>
    /// Initializes the singleton, hides the game over panel, and resets the score.
    /// </summary>
    private void Awake()
    {
        if (_instance == null) {
            _instance = this;
        }

        if (_gameOverPanel != null) {
            _gameOverPanel.SetActive(false);
        }

        _currentScore = 0;
        UpdateScore();
    }

    /// <summary>
    /// Handles input for restarting the game after game over.
    /// </summary>
    private void Update()
    {
        if (_gameOverPanel.activeInHierarchy && Input.anyKeyDown) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Triggers game over state and pauses the game.
    /// </summary>
    public void GameOver()
    {
        if (_gameOverPanel != null) {
            _gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Adds to the player's score and updates the UI.
    /// </summary>
    /// <param name="add">Amount to add to the score.</param>
    public void AddScore(int add)
    {
        _currentScore += add;

        UpdateScore();
    }

    /// <summary>
    /// Updates the score display UI.
    /// </summary>
    private void UpdateScore()
    {
        _score.text = _currentScore.ToString("D5");
    }
}