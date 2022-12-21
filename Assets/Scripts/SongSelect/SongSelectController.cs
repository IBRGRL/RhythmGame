using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongSelectController : MonoBehaviour
{
    [SerializeField] GameObject startPanel = default;
    [SerializeField] GameObject loadingPanel = default;
    [SerializeField] GameObject songSelectPanel = default;
    
    [SerializeField] Text songTitleText;
    public static string songTitle = ""; // 別シーンからでも参照できるようにstatic
    
    [SerializeField] Toggle autoToggle;
    public static bool isAuto = false; // 別シーンからでも参照できるようにstatic

    [SerializeField] Text difficultyText;
    public static int difficulty = 0; // 別シーンからでも参照できるようにstatic
    string[] difficulties = new string[4] {"Easy", "Normal", "Hard", "Special"};
    Color[] difficultyColors = new Color[4] {Color.cyan, Color.green, Color.red, Color.yellow};
    
    void Awake()
    {
        autoToggle.isOn = isAuto;
        difficultyText.text = difficulties[difficulty];
        difficultyText.color = difficultyColors[difficulty];

        Time.timeScale = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator LoadSceneAndWait(string sceneName, float waitTime) {
        float start = Time.realtimeSinceStartup;
        AsyncOperation ope = SceneManager.LoadSceneAsync(sceneName);
        ope.allowSceneActivation = false;

        while (Time.realtimeSinceStartup - start < waitTime) // waitTime 秒後にシーン切り替え
        {
            yield return null;
        }
        ope.allowSceneActivation = true;
    }

    public void OnStart()
    {
        startPanel.SetActive(false);
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAndWait("Main", 1.0f)); // 少なくとも1秒待ってから遷移
    }

    public void OnClickBack()
    {
        startPanel.SetActive(false);
        songSelectPanel.SetActive(true);
    }

    public void OnClickSong(string title)
    {
        songTitle = title;
        songTitleText.text = title;
        songSelectPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    public void OnClickAutoToggle()
    {
        isAuto = autoToggle.isOn;
    }

    public void OnClickDifficulty()
    {
        difficulty = (difficulty + 1) % 4;
        difficultyText.text = difficulties[difficulty];
        difficultyText.color = difficultyColors[difficulty];
    }
}
