using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    int score, currentScore;
    int combo, maxCombo;
    
    // スコア・コンボ表示
    [SerializeField] GameObject scoreTextObj;
    [SerializeField] GameObject comboTextObj;
    [SerializeField] GameObject scoreSliderObj;

    private Text scoreText = default;
    private Text comboText = default;
    private Animator scoreTextAnimator = default;
    private Animator comboTextAnimator = default;
 
    private Slider scoreSlider = default;
 
    // 判定表示
    [SerializeField] GameObject judgeTextLeftObj = default;
    [SerializeField] GameObject judgeTextRightObj = default;
    [SerializeField] GameObject fastLateTextObj = default;
    [SerializeField] GameObject fastLateTimeTextObj = default;

    private Text judgeTextLeft = default;     
    private Text judgeTextRight = default;
    private Text fastLateText = default;
    private Text fastLateTimeText = default;

    private Animator judgeTextLeftAnimator = default;
    private Animator judgeTextRightAnimator = default;
    private Animator fastLateAnimator = default;
    private Animator fastLateTimeAnimator = default;
    private int judgeTextExpandHash;
    private int judgeTextRiseHash;
 
    // 音源の再生・停止に用いる
    [SerializeField] GameObject noteGenerator = default;
    AudioSource audioSource;
    int noteNum;

    // ゲーム一時停止時のパネル
    [SerializeField] GameObject pausePanel = default;
    [SerializeField] GameObject restartPanel = default;
    // カウントダウンのテキスト
    [SerializeField] Text countDownText = default;

    // リザルト画面のパネル
    [SerializeField] GameObject resultPanel = default;

    // ロード画面のパネル
    [SerializeField] GameObject loadingPanel = default;

    // リザルト画面に表示される数値
    [SerializeField] Text resultScoreText = default;
    [SerializeField] Text maxComboText = default;
    [SerializeField] Text difficultyText = default;
    string[] difficulties = new string[4] {"Easy", "Normal", "Hard", "Special"};
    Color[] difficultyColors = new Color[4] {Color.cyan, Color.green, Color.red, Color.yellow};

    // 画面背景の難易度に対応する色パネル TODO ライフゲージ機能もここに追加？
    [SerializeField] GameObject difficultyImageLeftObj = default;
    [SerializeField] GameObject difficultyImageRightObj = default;
    private Image difficultyImageLeft = default;
    private Image difficultyImageRight = default;
    
    int[] judgeCount = new int[4] {0, 0, 0, 0};
    int[] fastLateCount = new int[8] {0, 0, 0, 0, 0, 0, 0, 0};

    // プレイ画面の内訳表示カウンターおよびそれらのラベル
    [SerializeField] GameObject judgeCountTextObj1 = default;
    [SerializeField] GameObject judgeCountTextObj2 = default;
    [SerializeField] GameObject judgeCountTextObj3 = default;
    [SerializeField] GameObject judgeCountTextObj4 = default;
    [SerializeField] GameObject judgeCountTitleObj1 = default;
    [SerializeField] GameObject judgeCountTitleObj2 = default;
    [SerializeField] GameObject judgeCountTitleObj3 = default;
    [SerializeField] GameObject judgeCountTitleObj4 = default;

    private Text playingJudgeText1 = default;
    private Text playingJudgeText2 = default;
    private Text playingJudgeText3 = default;
    private Text playingJudgeText4 = default;

    private Animator judgeCountTextAnimator1 = default;
    private Animator judgeCountTextAnimator2 = default;
    private Animator judgeCountTextAnimator3 = default;
    private Animator judgeCountTextAnimator4 = default;
 
    private Image playingJudgeImage1 = default;
    private Image playingJudgeImage2 = default;
    private Image playingJudgeImage3 = default;
    private Image playingJudgeImage4 = default;

    private Animator judgeCountTitleAnimator1 = default;
    private Animator judgeCountTitleAnimator2 = default;
    private Animator judgeCountTitleAnimator3 = default;
    private Animator judgeCountTitleAnimator4 = default;
    private int judgeCountRiseHash; 
    private int judgeCountFallHash; 

    // リザルト画面の数字たち
    [SerializeField] GameObject resultJudgeTextObj1 = default;
    [SerializeField] GameObject resultJudgeTextObj2 = default;
    [SerializeField] GameObject resultJudgeTextObj3 = default;
    [SerializeField] GameObject resultJudgeTextObj4 = default;
    [SerializeField] GameObject resultFastLateTextObj1 = default;
    [SerializeField] GameObject resultFastLateTextObj2 = default;
    [SerializeField] GameObject resultFastLateTextObj3 = default;
    [SerializeField] GameObject resultFastLateTextObj4 = default;
    [SerializeField] GameObject resultFastLateTextObj5 = default;
    [SerializeField] GameObject resultFastLateTextObj6 = default;

    private Text resultJudgeText1 = default;
    private Text resultJudgeText2 = default;
    private Text resultJudgeText3 = default;
    private Text resultJudgeText4 = default;
    private Text resultFastLateText1 = default;
    private Text resultFastLateText2 = default;
    private Text resultFastLateText3 = default;
    private Text resultFastLateText4 = default;
    private Text resultFastLateText5 = default;
    private Text resultFastLateText6 = default;

    [SerializeField] Text autoText = default;
    
    [SerializeField] GameObject playCanvas = default;
    [SerializeField] GameObject stage = default;
    
    public bool isPaused {get; set;}

    bool isLeftPushed, isRightPushed;
    int leftCount, rightCount;

    bool judgeDisappearBeginFlag, judgeDisappearFlag, fastLateDisappearBeginFlag, fastLateDisappearFlag;
    int judgeDisappearCount, fastLateDisappearCount;

    // keep a copy of the executing script
    private IEnumerator judgeTextCoroutine;
    private IEnumerator fastLateTextCoroutine;
    private IEnumerator comboTextCoroutine;

    void Awake()
    {
        score = currentScore = combo = maxCombo = leftCount = rightCount = judgeDisappearCount = fastLateDisappearCount = 0;
        isPaused = isLeftPushed = isRightPushed = judgeDisappearBeginFlag = judgeDisappearFlag = fastLateDisappearBeginFlag = fastLateDisappearFlag = false;
 
        Time.timeScale = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreText = scoreTextObj.GetComponent<Text>();
        comboText = comboTextObj.GetComponent<Text>();
        scoreTextAnimator = scoreTextObj.GetComponent<Animator>();
        comboTextAnimator = comboTextObj.GetComponent<Animator>();
        scoreSlider = scoreSliderObj.GetComponent<Slider>();

        judgeTextLeft = judgeTextLeftObj.GetComponent<Text>();
        judgeTextRight = judgeTextRightObj.GetComponent<Text>();
        fastLateText = fastLateTextObj.GetComponent<Text>();
        fastLateTimeText = fastLateTimeTextObj.GetComponent<Text>();
        judgeTextLeftAnimator = judgeTextLeftObj.GetComponent<Animator>();
        judgeTextRightAnimator = judgeTextRightObj.GetComponent<Animator>();
        fastLateAnimator = fastLateTextObj.GetComponent<Animator>();
        fastLateTimeAnimator = fastLateTimeTextObj.GetComponent<Animator>();

        difficultyImageLeft = difficultyImageLeftObj.GetComponent<Image>();
        difficultyImageRight = difficultyImageRightObj.GetComponent<Image>();
        
        playingJudgeText1 = judgeCountTextObj1.GetComponent<Text>();
        playingJudgeText2 = judgeCountTextObj2.GetComponent<Text>();
        playingJudgeText3 = judgeCountTextObj3.GetComponent<Text>();
        playingJudgeText4 = judgeCountTextObj4.GetComponent<Text>();

        judgeCountTextAnimator1 = judgeCountTextObj1.GetComponent<Animator>();
        judgeCountTextAnimator2 = judgeCountTextObj2.GetComponent<Animator>();
        judgeCountTextAnimator3 = judgeCountTextObj3.GetComponent<Animator>();
        judgeCountTextAnimator4 = judgeCountTextObj4.GetComponent<Animator>();

        playingJudgeImage1 = judgeCountTitleObj1.GetComponent<Image>();
        playingJudgeImage2 = judgeCountTitleObj2.GetComponent<Image>();
        playingJudgeImage3 = judgeCountTitleObj3.GetComponent<Image>();
        playingJudgeImage4 = judgeCountTitleObj4.GetComponent<Image>();

        judgeCountTitleAnimator1 = judgeCountTitleObj1.GetComponent<Animator>();
        judgeCountTitleAnimator2 = judgeCountTitleObj2.GetComponent<Animator>();
        judgeCountTitleAnimator3 = judgeCountTitleObj3.GetComponent<Animator>();
        judgeCountTitleAnimator4 = judgeCountTitleObj4.GetComponent<Animator>();

        resultJudgeText1 = resultJudgeTextObj1.GetComponent<Text>();
        resultJudgeText2 = resultJudgeTextObj2.GetComponent<Text>();
        resultJudgeText3 = resultJudgeTextObj3.GetComponent<Text>();
        resultJudgeText4 = resultJudgeTextObj4.GetComponent<Text>();
        resultFastLateText1 = resultFastLateTextObj1.GetComponent<Text>();
        resultFastLateText2 = resultFastLateTextObj2.GetComponent<Text>();
        resultFastLateText3 = resultFastLateTextObj3.GetComponent<Text>();
        resultFastLateText4 = resultFastLateTextObj4.GetComponent<Text>();
        resultFastLateText5 = resultFastLateTextObj5.GetComponent<Text>();
        resultFastLateText6 = resultFastLateTextObj6.GetComponent<Text>();

        audioSource = noteGenerator.GetComponent<NoteGenerator>().audioSource;
        noteNum = noteGenerator.GetComponent<NoteGenerator>().noteNumIncludingHolds;

        judgeTextExpandHash = Animator.StringToHash("Base Layer.JudgeText@expand");
        judgeTextRiseHash = Animator.StringToHash("Base Layer.JudgeText@rise");
        judgeCountRiseHash = Animator.StringToHash("Base Layer.JudgeCount@rise");
        judgeCountFallHash = Animator.StringToHash("Base Layer.JudgeCount@fall");

        scoreSlider.value = 0;
        scoreText.text = comboText.text = judgeTextLeft.text = judgeTextRight.text = fastLateText.text = fastLateTimeText.text = "";
        playingJudgeText1.text = playingJudgeText2.text = playingJudgeText3.text = playingJudgeText4.text = "0";
        resultScoreText.text = maxComboText.text = "";
        resultJudgeText1.text = resultJudgeText2.text = resultJudgeText3.text = resultJudgeText4.text = ""; 
        resultFastLateText1.text = resultFastLateText2.text = resultFastLateText3.text = resultFastLateText4.text = resultFastLateText5.text = resultFastLateText6.text = ""; 

        difficultyImageLeft.color = difficultyImageRight.color = difficultyColors[SongSelectController.difficulty];

        // TODO result画面のテキスト 今はTextを直接Inspectorから指定してるけどいずれ変える気がする
        difficultyText.text = difficulties[SongSelectController.difficulty];
        difficultyText.color = difficultyColors[SongSelectController.difficulty];

        if (SongSelectController.isAuto) {
            autoText.text = "Auto";
        } else {
            autoText.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentScore > 0) {
            scoreText.text = currentScore.ToString("D7"); // 出力が7桁になる
        }
    }

    void FixedUpdate() { // 間隔 0.05s
        if (isLeftPushed && isRightPushed) {
            OnPause();
            isLeftPushed = isRightPushed = false;
            leftCount = rightCount = 0;
        }
        if (isLeftPushed) {
            leftCount++;
        }
        if (leftCount > 2) {
            isLeftPushed = false;
            leftCount = 0;
        }
        if (isRightPushed) {
            rightCount++;
        }
        if (rightCount > 2) {
            isRightPushed = false;
            rightCount = 0;
        }
        
        // 一定時間経過したら判定表示のテキストを消す
        if (judgeDisappearBeginFlag) {
            if (judgeDisappearFlag) {
                judgeDisappearFlag = false;
                judgeDisappearCount = 0;
            } else {
                judgeDisappearCount++;
            }
        }
        if (judgeDisappearCount >= 6) { // 0.30s経過
            judgeDisappearBeginFlag = false;
            judgeDisappearCount = 0;
            judgeTextCoroutine = judgeTextFadeOutCoroutine(judgeTextLeft.color, 0.30f);
            StartCoroutine(judgeTextCoroutine);
        }

        if (fastLateDisappearBeginFlag) {
            if (fastLateDisappearFlag) {
                fastLateDisappearFlag = false;
                fastLateDisappearCount = 0;
            } else {
                fastLateDisappearCount++;
            }
        }
        if (fastLateDisappearCount >= 6) { // 0.30s経過
            fastLateDisappearBeginFlag = false;
            fastLateDisappearCount = 0;
            fastLateTextCoroutine = fastLateTextFadeOutCoroutine(fastLateText.color, fastLateTimeText.color, 0.30f);
            StartCoroutine(fastLateTextCoroutine);
        }
    }

    IEnumerator judgeTextFadeOutCoroutine(Color c, float fadeTime)
    {
        Color startColor = c;
        float time = 0;
        float maxOpacity = c.a; // テキストの不透明度
        while (time < fadeTime) {
            time += Time.deltaTime;
            startColor.a = maxOpacity * Mathf.Max(1 - time / fadeTime, 0);
            judgeTextLeft.color = judgeTextRight.color = startColor;
            yield return null;
        }
    }

    IEnumerator fastLateTextFadeOutCoroutine(Color c1, Color c2, float fadeTime)
    {
        Color startColor1 = c1; // FAST / LATE
        Color startColor2 = c2; // 時間差(ms)
        float maxOpacity = c1.a; // テキストの不透明度(c1とc2で共通)
        float time = 0;
        while (time < fadeTime) {
            time += Time.deltaTime;
            float opacity = maxOpacity * (1 - time / fadeTime);
            startColor1.a = startColor2.a = Mathf.Max(opacity, 0);
            fastLateText.color = startColor1;
            fastLateTimeText.color = startColor2;
            yield return null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused && !resultPanel.activeSelf) {
            OnPause();
        }
    }

    public void OnSideButtonLeft()
    {
        isLeftPushed = true;
    }

    public void OnSideButtonRight()
    {
        isRightPushed = true;
    }

    private void OnPause()
    {
        isPaused = true;
        audioSource.Pause();
        pausePanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnRestart()
    {
        StartCoroutine(RestartCoroutine());
    }

    IEnumerator RestartCoroutine()
    {
        pausePanel.SetActive(false); 
        restartPanel.SetActive(true); 
        countDownText.text = "3";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "2";
        yield return new WaitForSecondsRealtime(1);
        countDownText.text = "1";
        yield return new WaitForSecondsRealtime(1);
        isPaused = false;
        audioSource.UnPause();
        restartPanel.SetActive(false); 
        Time.timeScale = 1.0f;
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

    public void OnRetry()
    {
        playCanvas.SetActive(false); 
        resultPanel.SetActive(false); 
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAndWait("Main", 1.0f)); // 少なくとも1秒待ってから遷移
    }

    public void OnNext()
    {
        playCanvas.SetActive(false); 
        resultPanel.SetActive(false); 
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAndWait("Song Select", 1.0f)); // 少なくとも1秒待ってから遷移
    }

    public void OnEndEvent()
    {
        StartCoroutine(ResultCoroutine());
    }

    IEnumerator ResultCoroutine()
    {
        playCanvas.SetActive(false); 
        stage.SetActive(false); 
        audioSource.Stop();
        resultPanel.SetActive(true); 
        yield return new WaitForSecondsRealtime(1);
        resultScoreText.text = score.ToString("D7");
        maxComboText.text = maxCombo.ToString();
        yield return new WaitForSecondsRealtime(1);
        resultJudgeText1.text = judgeCount[1].ToString(); // Perfect
        resultJudgeText2.text = judgeCount[2].ToString(); // Great
        resultJudgeText3.text = judgeCount[3].ToString(); // Good
        resultJudgeText4.text = judgeCount[0].ToString(); // Miss
        resultFastLateText1.text = fastLateCount[2].ToString();
        resultFastLateText2.text = fastLateCount[3].ToString();
        resultFastLateText3.text = fastLateCount[0].ToString();
        resultFastLateText4.text = fastLateCount[6].ToString();
        resultFastLateText5.text = fastLateCount[7].ToString();
        resultFastLateText6.text = fastLateCount[4].ToString();
    }

    string[] judges = new string[4] {"MISS", "PERFECT", "GREAT", "GOOD"};
    int[] scores = new int[4] {0, 100, 60, 30}; // スコアの割合
    Color[] colors = new Color[4] {Color.red, Color.cyan, Color.yellow, Color.green};
    
    void showJudge(int judge, bool isFast, int timediff)
    {
        judgeTextLeft.text = judgeTextRight.text = judges[judge];
       
        // テキストの色変化
        judgeTextLeft.color = judgeTextRight.color = colors[judge];
        judgeDisappearBeginFlag = judgeDisappearFlag = true;
        if (judgeTextCoroutine is object) {
            StopCoroutine(judgeTextCoroutine);
        }

        if (judge != 1) {
            fastLateText.text = isFast ? "FAST" : "LATE";
            fastLateText.color = isFast ? new Color(0.0f, 0.5f, 1.0f, 0.7f) : new Color(1.0f, 0.0f, 0.0f, 0.7f);
            fastLateTimeText.color = new Color(1.0f, 1.0f, 1.0f, 0.7f);
            
            // ノーツを叩かなかった場合または, ホールドをあまりにも早く離しすぎた場合
            if (timediff == -1 || timediff > 999) {
                fastLateTimeText.text = "n/a";
            } else {
                fastLateTimeText.text = timediff.ToString();
            }

            fastLateDisappearBeginFlag = fastLateDisappearFlag = true;
            if (fastLateTextCoroutine is object) {
                StopCoroutine(fastLateTextCoroutine);
            }
        }

        // テキストが膨張するアニメーション
        judgeTextLeftAnimator.Play(judgeTextExpandHash, 0, 0.0f);
        judgeTextRightAnimator.Play(judgeTextExpandHash, 0, 0.0f);
        if (judge != 1) {
            fastLateAnimator.Play(judgeTextExpandHash, 0, 0.0f);
            fastLateTimeAnimator.Play(judgeTextExpandHash, 0, 0.0f);
        }
    }

    // scoreの増加をゆっくりにするアニメーション
    IEnumerator ScoreAnimationCoroutine(int before, int after, float time)
    {
        float elapsedTime = 0;
        int scoreBefore = before;
        while (elapsedTime < time) {
            elapsedTime += Time.deltaTime;
            float proportion = Mathf.Min(elapsedTime / time, 1.0f); // 行き過ぎ防止
            float easeOut = 1 - Mathf.Pow(1 - proportion, 5); // easeOutQuint
            int scoreAfter = before + (int)((after - before) * easeOut);
            currentScore += (scoreAfter - scoreBefore);
            scoreBefore = scoreAfter;
            yield return null;
        }
    }

    public void setScore(int judge, bool isFast, int timediff)
    {
        judgeCount[judge]++;
        // update playingJudgeTexts
        if (judge == 1) {
            playingJudgeImage1.color = colors[judge];
            playingJudgeText1.color = Color.white;
            playingJudgeText1.text = judgeCount[judge].ToString();
            judgeCountTitleAnimator1.Play(judgeCountRiseHash, 0, 0.0f);
            judgeCountTextAnimator1.Play(judgeCountFallHash, 0, 0.0f);
        } else if (judge == 2) {
            playingJudgeImage2.color = colors[judge];
            playingJudgeText2.color = Color.white;
            playingJudgeText2.text = judgeCount[judge].ToString();
            judgeCountTitleAnimator2.Play(judgeCountRiseHash, 0, 0.0f);
            judgeCountTextAnimator2.Play(judgeCountFallHash, 0, 0.0f);
        } else if (judge == 3) {
            playingJudgeImage3.color = colors[judge];
            playingJudgeText3.color = Color.white;
            playingJudgeText3.text = judgeCount[judge].ToString();
            judgeCountTitleAnimator3.Play(judgeCountRiseHash, 0, 0.0f);
            judgeCountTextAnimator3.Play(judgeCountFallHash, 0, 0.0f);
        } else if (judge == 0) {
            playingJudgeImage4.color = colors[judge];
            playingJudgeText4.color = Color.white;
            playingJudgeText4.text = judgeCount[judge].ToString();
            judgeCountTitleAnimator4.Play(judgeCountRiseHash, 0, 0.0f);
            judgeCountTextAnimator4.Play(judgeCountFallHash, 0, 0.0f);
        }

        int scoreBefore = score; 
        // update score
        int tmp = 0;
        for (int i = 0; i < 4; i++) {
            tmp += judgeCount[i] * scores[i];
        }
        score = tmp * 10000 / noteNum;

        // Miss の場合は score は変わらないので scoreText も変える必要ない
        if (judge != 0) {
            // （第3引数）秒後に表示されるスコアが収束するようなアニメーション
            StartCoroutine(ScoreAnimationCoroutine(scoreBefore, score, 0.5f));
            scoreTextAnimator.Play(judgeTextRiseHash, 0, 0.0f);
        }

        scoreSlider.value = score;
        showJudge(judge, isFast, timediff);

        if (isFast) {
            fastLateCount[judge]++;
        } else {
            fastLateCount[judge + 4]++;
        }
    }

    // コンボテキストが表示される最小のコンボ数パラメータ
    const int comboLimit = 5;
    public void setCombo(int judge)
    {      
        if (judge != 0) {
            combo++;
            if (combo >= comboLimit) {
                comboText.text = combo.ToString();
                comboText.color = Color.white; // とりあえず色は白色
                comboTextAnimator.Play(judgeTextRiseHash, 0, 0.0f);
                if (comboTextCoroutine is object) {
                    StopCoroutine(comboTextCoroutine);
                }
            }
        } else {
            if (combo >= comboLimit) {
                // combo が表示された状態で Miss した時のアニメーション
                comboTextCoroutine = comboTextFadeOutCoroutine(comboText.color, 0.30f);
                StartCoroutine(comboTextCoroutine);
            }
            combo = 0;
        }
        maxCombo = Mathf.Max(maxCombo, combo);
    }

    IEnumerator comboTextFadeOutCoroutine(Color c, float fadeTime)
    {
        Color startColor = c;
        float time = 0;
        float maxOpacity = c.a; // テキストの不透明度
        while (time < fadeTime) {
            time += Time.deltaTime;
            startColor.a = maxOpacity * Mathf.Max(1 - time / fadeTime, 0);
            comboText.color = startColor;
            yield return null;
        }
    }
}
