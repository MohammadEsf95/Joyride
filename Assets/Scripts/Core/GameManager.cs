using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("UI")]
    public TMP_Text coinText;
    public TMP_Text distanceText;
    
    private bool _gameOver;
    private int _totalCoins;
    private PlayerCamera _playerCamera;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        _playerCamera = FindObjectOfType<PlayerCamera>();
        UpdateDistanceUI();
    }

    void Update()
    {
        if (!_gameOver)
            UpdateDistanceUI();

        if (_gameOver && Input.anyKeyDown)
        {
            Restart();
        }
    }
    
    public void AddCoins(int amount)
    {
        _totalCoins += amount;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (coinText != null)
            coinText.text = "Score: " + _totalCoins;
    }

    void UpdateDistanceUI()
    {
        if (distanceText == null || _playerCamera == null)
            return;

        int meters = Mathf.FloorToInt(_playerCamera.DistanceTraveled);
        distanceText.text = meters + " m";
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        _gameOver = true;
    }

    void Restart()
    {
        Time.timeScale = 1;
        _gameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}