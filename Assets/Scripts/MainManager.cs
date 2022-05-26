using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public Brick BrickPrefab;
    public int LineCount = 6;
    public Rigidbody Ball;

    public Text ScoreText;
    public GameObject GameOverText;
    public Text BestScoreText;
    public int BestScore = 0;
    public string PlayerName = "Player";
    
    private bool m_Started = false;
    private int m_Points;
    
    private bool m_GameOver = false;

    
    // Start is called before the first frame update
    void Start()
    {
        const float step = 0.6f;
        int perLine = Mathf.FloorToInt(4.0f / step);
       
        // Load data if available
        SaveData save = new MainManager.SaveData();
        save.LoadBestScore();
        if (!string.IsNullOrEmpty(save.SavedName))
        {
            PlayerName = save.SavedName;
            BestScore = save.SavedScore;
        }

        setBestScore();

        int[] pointCountArray = new [] {1,1,2,2,5,5};
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

        if (m_Points > BestScore)
        {
            BestScore = m_Points;
            setBestScore();
        }
    }

    public void GameOver()
    {
        m_GameOver = true;
        GameOverText.SetActive(true);
        if (m_Points >= BestScore)
        {
            SaveData save = new MainManager.SaveData();
            save.SaveBestScore();
        }
    }

    public string getBestScore()
    {
        return "Best Score : " + getPlayerName() + " : " + BestScore;
    }

    public void setBestScore()
    {
        BestScoreText.text = getBestScore();
    }

    public string getPlayerName()
    {
        return DataManager.Instance.PlayerName;
    }


    [System.Serializable]
    class SaveData
    {
        public string SavedName;
        public int SavedScore;

        public void SaveBestScore()
        {
            SaveData data = new SaveData();
            data.SavedName = GameObject.FindWithTag("MainManager").GetComponent<MainManager>().PlayerName;
            data.SavedScore = GameObject.FindWithTag("MainManager").GetComponent<MainManager>().BestScore;

            string json = JsonUtility.ToJson(data);

            File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        }

        public void LoadBestScore()
        {
            string path = Application.persistentDataPath + "/savefile.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                SavedName = data.SavedName;
                SavedScore = data.SavedScore;
            }
        }
    }
}
