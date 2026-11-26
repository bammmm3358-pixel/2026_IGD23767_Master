using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour

{

    public static GameManager Instance { get; private set; }



    [Header("Rules")]
    public int killsToWin = 10;
    [Header("UI Panels (ไม่บังคับ)")]
    public GameObject winPanel;
    public GameObject losePanel;
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int> OnScoreChanged;

    void Awake()
    {

        Instance = this;

        Time.timeScale = 1f;

    }



    void Start()

    {
    
        if (winPanel) winPanel.SetActive(false);
    
        if (losePanel) losePanel.SetActive(false);
    

        GameObject player = GameObject.FindGameObjectWithTag("Player");
    
        if (player != null)
        
        {
        
            Health h = player.GetComponent<Health>();
        
            if (h != null) h.OnDeath += Lose;
        
        }
    
        OnScoreChanged?.Invoke(Score);
    
    }



    public void AddScore(int amount)

    {
     if (IsGameOver) return;
    
        Score += amount;
        OnScoreChanged?.Invoke(Score);
        if (Score >= killsToWin * 10) Win();
    
    }

    public void Win()

    {
    
        if (IsGameOver) return;
    
        IsGameOver = true;
    
        if (winPanel) winPanel.SetActive(true);
    
        if (AudioManager.Instance) AudioManager.Instance.PlayWin();
    
        Time.timeScale = 0f;
    
    }



    public void Lose()

    {
    
        if (IsGameOver) return;
    
        IsGameOver = true;
    
        if (losePanel) losePanel.SetActive(true);
    
        if (AudioManager.Instance) AudioManager.Instance.PlayLose();
    
        Time.timeScale = 0f;
    
    }
    public void Restart()

    {
    
        Time.timeScale = 1f;
    
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    
    }



}