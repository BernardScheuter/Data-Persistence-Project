using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    private int ScoreCount = 0;
    public Text HighScoreText;
    private int HighScoreCount;

    private string PlayerName;

    public GameObject GameOverText;
    public GameObject UserInputWindow;

    private bool m_Started = false;
    private int m_Points;
    private bool m_GameOver = false;

    void Start()
    {
        UserInputWindow.SetActive(false);
        Load(); //load from disk if savefile (HighScoreSave.json) exists
        PlayerPrefs.DeleteAll(); // clear local score
        PlayerPrefs.SetString("ppPlayerName", PlayerName);
        PlayerPrefs.SetInt("ppHighScoreCount", HighScoreCount);
        UpdateHighScore();
        Debug.Log("Loaded from disk at start:" + "\n" + "PlayerName: " + PlayerName + ", " + "HighScoreCount: " + HighScoreCount);

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
    }

    public void GameOver()
    {
        m_GameOver = true;
        GameOverText.SetActive(true);
        ScoreCount = m_Points;
        if (ScoreCount > HighScoreCount)
        {
            GetuserName();
        }
        else { return; }
    }
    public void GetuserName()
    {
        UserInputWindow?.SetActive(true);
    }

    public void ReadInputField(Text userName)
    {
        PlayerName = userName.text;
        Debug.Log("UserName bij ReadInputField: "+ userName.text);
        HighScoreCount = ScoreCount;
        Debug.Log("ReadInputField()" + "\n" + PlayerName.ToString());
        SaveDataToReg(PlayerName, HighScoreCount);
        Save();
        CloseUserInputWindow();
    }

    public void CloseUserInputWindow()
    {
        UserInputWindow.SetActive(false);
    }

    public void UpdateHighScore()
    {
        HighScoreText.text = $"HighScore : {HighScoreCount}" + "\n" + "PlayerName: " + PlayerName;
    }

    #region DatainRegister
    public void SaveDataToReg(string _name, int _score)
    {
        if (_name == null || _name == "")
        {
            _name = "Unnamed User!";
        }
        PlayerPrefs.SetString("ppPlayerName", _name);
        PlayerPrefs.SetInt("ppHighScoreCount", _score);
        Debug.Log("SaveDataToReg(): " + "\n" + "PlayerName: " + _name + "\n" + "ppHighScoreCount: " + _score);
    }
    public void DeleteDataFromReg()
    {
        PlayerPrefs.DeleteAll();
        HighScoreCount = 0;
        PlayerName = "PlayerErased";
        UpdateHighScore();
        Debug.Log("Data erased!");
    }
    public void LoadDataFromReg()
    {
        HighScoreCount = PlayerPrefs.GetInt("ppHighScoreCount", 0);
        if (HighScoreCount == 0 || HighScoreCount < 1)
        {
            HighScoreCount = 0;
        }
        PlayerName = PlayerPrefs.GetString("ppPlayerName");
        if (PlayerName == "" || PlayerName == null)
        {
            PlayerName = "NewUser(Empty registry)";
        }
        UpdateHighScore();
        Debug.Log("Data Loaded: " + "\n" + "PlayerName: " + PlayerName + "\n" + "ppHighScoreCount: " + HighScoreCount);
    }
    #endregion

    [System.Serializable]
    public class DiskAccess
    {
        public string Name;
        public int Score;
    }

    public void Save()
    {
        DiskAccess currentSave = new DiskAccess
        {
            Name = PlayerName,
            Score = HighScoreCount
        };

        File.WriteAllText(Application.persistentDataPath + "/HighScoreSave.json", JsonUtility.ToJson(currentSave));
    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/HighScoreSave.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DiskAccess loadData = JsonUtility.FromJson<DiskAccess>(json);
            PlayerName = loadData.Name;
            HighScoreCount = loadData.Score;
        }
        else return;
    }

    public void ExitApp()
    {
        Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
