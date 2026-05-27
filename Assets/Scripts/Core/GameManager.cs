using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("UI")]
    public TMP_Text coinText;
    
    private bool _gameOver;
    private int _totalCoins;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
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