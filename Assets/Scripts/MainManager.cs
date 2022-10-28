using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public Text HighScoreText; //
    public int HighScoreCount;
    public string PlayerName;
    public GameObject GameOverText;

    private bool m_Started = false;
    private int m_Points;

    private bool m_GameOver = false;


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("ppHighScoreCount"))
        {
            LoadData();
        }
        else
        {
            if (HighScoreCount == 0 || HighScoreCount < 1)
            {
                HighScoreCount = 0;
            }
            if (PlayerName == "" || PlayerName == null)
            {
                PlayerName = "TempUser";
            }
            return;
        }

        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);

        int[] pointCountArray = new[] { 1, 1, 2, 2, 5, 5 };
        for (int i = 0; i < LineCount; ++i)
        {
            for (int x = 0; x < perLine; ++x)
            {
                Vector3 position = new Vector3(-1.5f + step * x, 2.5f + i * 0.3f, 0);
                var brick = Instantiate(BrickPrefab, position, Quaternion.identity);
                brick.PointValue = pointCountArray[i];
                brick.onDestroyed.AddListener(AddPoint);
            }
        }
    }

    private void Update()
    {
        #region DebugKeys
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadData();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveData();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeleteData();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitApp();
        }
        #endregion

        

        if (!m_Started)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_Started = true;
                float randomDirection = Random.Range(-1.0f, 1.0f);
                Vector3 forceDir = new Vector3(randomDirection, 1, 0);
                forceDir.Normalize();

                Ball.transform.SetParent(null);
                Ball.AddForce(forceDir * 2.0f, ForceMode.VelocityChange);
            }
        }
        else if (m_GameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void AddPoint(int point)
    {
        m_Points += point;
        ScoreText.text = $"Score : {m_Points}";
        if (HighScoreCount < 1 || m_Points >= HighScoreCount)
        {
            HighScoreCount = m_Points;
            HighScoreText.text = $"HighScore : {HighScoreCount}" + "\n" + "PlayerName: " + PlayerName;
        }
    }

    public void GameOver()
    {
        SaveData();
        m_GameOver = true;
        GameOverText.SetActive(true);
    }

    public void SaveData()
    {
        //Save to registry
        if (PlayerName == null)
        {
            PlayerName = "Unknown User at save";
        }
        if (HighScoreCount > 0)
        {
            PlayerPrefs.SetString("ppPlayerName", PlayerName);
            PlayerPrefs.SetInt("ppHighScoreCount", HighScoreCount);
            Debug.Log("Data Saved: " + "\n" + "PlayerName: " + PlayerName + "\n" + "ppHighScoreCount: " + HighScoreCount);
        }
        else
        {
            Debug.Log("No new data/highscore!");
            return;
        }
    }

    public void LoadData()
    {
        //Load from registry
        PlayerName = PlayerPrefs.GetString("PlayerName");
        HighScoreCount = PlayerPrefs.GetInt("ppHighScoreCount");
        Debug.Log("Data Loaded: " + "\n" + "PlayerName: " + PlayerName + "\n" + "ppHighScoreCount: " + HighScoreCount);
    }

    public void DeleteData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Data erased!");
    }

    public void ExitApp()
    {
        SaveData();
        Application.Quit();
    }
}
