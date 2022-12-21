using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteGenerator : MonoBehaviour
{
    // 譜面データを読み込んでノーツを生成する
    // 同時に音楽の再生も行う

    [SerializeField] GameController gameController;
    [SerializeField] Slider musicSlider;

    [SerializeField] NoteLine noteLine;
    [SerializeField] NoteLine noteLine2;

    [SerializeField] NoteHold noteHold;
    [SerializeField] NoteHold noteHold2;

    [SerializeField] NoteCircle noteCircle;
    [SerializeField] NoteCircle noteCircle2;

    [SerializeField] Transform circleLeftTransform;
    [SerializeField] Transform circleRightTransform;

    [SerializeField] GameObject inCircleLeft;
    [SerializeField] GameObject inCircleRight;

    // pool 化を行う
    // Prefab を格納する配列を宣言
    private NoteLine[] noteLineArray = new NoteLine[100];
    private NoteLine[] noteLine2Array = new NoteLine[100];
    private NoteHold[] noteHoldArray = new NoteHold[50];
    private NoteHold[] noteHold2Array = new NoteHold[50];
    private NoteCircle[] noteCircleArray = new NoteCircle[50];
    private NoteCircle[] noteCircle2Array = new NoteCircle[50];

    float[] judgeAreaPos = new float[5]; // 配列の宣言
    string[] judgeAreaName = new string[5] {"LineZero", "LineFirst", "LineSecond", "LineThird", "LineFourth"};
    string[] noteTag = new string[5] {"NoteZero", "NoteFirst", "NoteSecond", "NoteThird", "NoteFourth"};
    string[] holdTag = new string[5] {"HoldZero", "HoldFirst", "HoldSecond", "HoldThird", "HoldFourth"};

    public static int cameraHeight = 9; // 線ノーツが見える領域
    public static float judgeCircleSize = (3.0f + 2.6f) / 2 / 2; // judgeCirlce の見た目に依存

    private int noteNum;
    public int noteNumIncludingHolds;
    float bpm;
    NoteSpeed noteSpeed;

    // 音源の再生を行う
    public AudioSource audioSource;
    private AudioClip musicFile; 
    float musicProgress;
    
    // 譜面データの読み込み
    [System.Serializable]
    public class ChartTime
    {
        public int[] beat;
        public float bpm;
    }

    [System.Serializable]
    public class ChartNote
    {
        public int[] beat;
        public int[] endbeat;
        public int column;
        public string sound;
        public int vol;
        public int offset;
    }
    
    [System.Serializable]
    public class Chart
    {
        public ChartTime[] time;
        public ChartNote[] note;
    }

    Chart chart = default;
    
    SpriteRenderer circleLeftSpriteRenderer;
    SpriteRenderer circleRightSpriteRenderer;
    
    void Awake()
    {
        // 最初にInstantiateで全て生成してprefabArrayに格納しておく
        for (int i = 0; i < noteLineArray.Length; i++)
        {
            NoteLine note = Instantiate(noteLine);
            // この時生成したprefabは一旦非表示状態にしておく
            note.gameObject.SetActive(false);
            noteLineArray[i] = note;
        }
        for (int i = 0; i < noteLine2Array.Length; i++)
        {
            NoteLine note = Instantiate(noteLine2);
            note.gameObject.SetActive(false);
            noteLine2Array[i] = note;
        }
        for (int i = 0; i < noteHoldArray.Length; i++)
        {
            NoteHold note = Instantiate(noteHold);
            note.noteType = false;
            note.gameObject.SetActive(false);
            noteHoldArray[i] = note;
        }
        for (int i = 0; i < noteHold2Array.Length; i++)
        {
            NoteHold note = Instantiate(noteHold2);
            note.noteType = true;
            note.gameObject.SetActive(false);
            noteHold2Array[i] = note;
        }
        for (int i = 0; i < noteCircleArray.Length; i++)
        {
            NoteCircle note = Instantiate(noteCircle);
            note.gameObject.SetActive(false);
            noteCircleArray[i] = note;
        }
        for (int i = 0; i < noteCircle2Array.Length; i++)
        {
            NoteCircle note = Instantiate(noteCircle2);
            note.gameObject.SetActive(false);
            noteCircle2Array[i] = note;
        }

        // 譜面データの読み込み
        string songTitle = SongSelectController.songTitle; // Song Select Sceneから曲名を読み込む 
        string difficulty = SongSelectController.difficulty.ToString(); // 難易度を読み込む
        string data = Resources.Load<TextAsset>("Charts/" + songTitle + "/" + difficulty).text;
        chart = JsonUtility.FromJson<Chart>(data);
        bpm = chart.time[0].bpm; // BPMの取得 TODO BPM変動がある場合はちゃんと考える必要がある
        noteNum = chart.note.Length - 1; // ノーツ数

        ChartNote sound = chart.note[noteNum];
        musicFile = Resources.Load<AudioClip>("Musics/" + songTitle);
        audioSource.clip = musicFile;
        audioSource.volume = (float)sound.vol / 100;
        musicSlider.value = 0;

        float offset = (float)sound.offset / 1000;
        Debug.Log("offset = " + offset);
        audioSource.PlayDelayed(offset);

        noteNumIncludingHolds = noteNum;
        for (int i = 0; i < noteNum; i++) {
            if (chart.note[i].endbeat is object) {
                noteNumIncludingHolds++;
            }
        }
        Debug.Log("NoteNum = " + noteNumIncludingHolds);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        circleLeftSpriteRenderer = inCircleLeft.GetComponent<SpriteRenderer>();
        circleRightSpriteRenderer = inCircleRight.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 5; i++) {
            judgeAreaPos[i] = GameObject.Find(judgeAreaName[i]).gameObject.transform.position.x;
        }

        this.noteSpeed = FindObjectOfType<NoteSpeed>(); // インスタンス化

        // 円ノーツの個数
        int circleLeftCount = 0, circleRightCount = 0;
        // 円ノーツと下ノーツの同時押しがあるかどうか確認
        bool[] colorChangeLine = new bool[noteNum];
        // 円ノーツの同時押しがあるかどうか確認
        bool[] colorChangeCircle = new bool[noteNum];
        for (int i = 0; i < noteNum; i++) {
            colorChangeLine[i] = false;
            colorChangeCircle[i] = false;
        }
        for (int i = 0; i < noteNum; i++) {
            ChartNote circleNote = chart.note[i];
            if (circleNote.column == 0) {
                circleLeftCount++;
                for (int j = 1; i + j < noteNum; j++) {
                    ChartNote lineNote = chart.note[i + j];
                    if (circleNote.beat[0] == lineNote.beat[0] && circleNote.beat[1] * lineNote.beat[2] == circleNote.beat[2] * lineNote.beat[1]) {
                        if (lineNote.column != 6) {
                            colorChangeLine[i + j] = true;
                        } else {
                            colorChangeCircle[i] = colorChangeCircle[i + j] = true;
                        }
                    } else {
                        break;
                    }
                }
            } else if (circleNote.column == 6) {
                circleRightCount++;
                for (int j = 1; i - j >= 0; j++) {
                    ChartNote lineNote = chart.note[i - j];
                    if (circleNote.beat[0] == lineNote.beat[0] && circleNote.beat[1] * lineNote.beat[2] == circleNote.beat[2] * lineNote.beat[1]) {
                        if (lineNote.column != 0) {
                            colorChangeLine[i - j] = true;
                        }
                    } else {
                        break;
                    }
                }
            }
        }

        // 円形ノーツが降ってくるタイミングを格納しておくList
        List<float> circleLeftTimes = new List<float>();
        List<float> circleRightTimes = new List<float>();

        float userOffset = OffsetSlider.offset; // Song Select Scene で設定した値を読み込む 
        for (int i = 0; i < noteNum; i++) {
            ChartNote note = chart.note[i];
            float beat = note.beat[0] + (float)note.beat[1] / note.beat[2];
            float secBegin = beat * (60 / bpm) + userOffset;
            float valBegin = secBegin * noteSpeed.speed;
            float endbeat = 0, secEnd = 0, valEnd = 0;
            if (note.endbeat is object) {
                // ホールドノーツの場合
                endbeat = note.endbeat[0] + (float)note.endbeat[1] / note.endbeat[2];
                secEnd = endbeat * (60 / bpm) + userOffset;
                valEnd = secEnd * noteSpeed.speed;
            }
            if (note.column == 0) {
                circleLeftTimes.Add(secBegin);
            } else if (note.column == 6) {
                circleRightTimes.Add(secBegin);
            }
            // 初期化では可視領域（の2倍）のみノーツを設置する
            if (valBegin < cameraHeight * 2) {
                if (note.column == 0 || note.column == 6) {
                    // 円ノーツ
                    SpawnNote(valBegin, valEnd, note.column, colorChangeCircle[i], secBegin, secEnd);
                } else {
                    // 線ノーツ or ホールドノーツ
                    SpawnNote(valBegin, valEnd, note.column, colorChangeLine[i], secBegin, secEnd);
                }
            } else {
                // 可視領域に入る直前にノーツを設置する
                int delayNum = (int)valBegin / cameraHeight - 1; // 何回遅延させるか
                float delay = delayNum * cameraHeight / noteSpeed.speed; // 開始からdelay秒後にノーツを設置
                float newValBegin = valBegin - delayNum * cameraHeight;
                float newValEnd = (endbeat == 0) ? 0 : (valEnd - delayNum * cameraHeight);
                if (note.column == 0 || note.column == 6) {
                    // 円ノーツ
                    StartCoroutine(SpawnNoteWithDelayCoroutine(delay, newValBegin, newValEnd, note.column, colorChangeCircle[i], secBegin, secEnd));
                } else {
                    // 線ノーツ or ホールドノーツ
                    StartCoroutine(SpawnNoteWithDelayCoroutine(delay, newValBegin, newValEnd, note.column, colorChangeLine[i], secBegin, secEnd));
                }
            }
        }

        // 円形判定エリアの色を変える
        float[] leftChangeColorTimeIn = new float[circleLeftCount];
        float[] leftChangeColorTimeOut = new float[circleLeftCount];
        float[] rightChangeColorTimeIn = new float[circleRightCount];
        float[] rightChangeColorTimeOut = new float[circleRightCount];
       
        float judgeCircleColorTime = 1.0f; // 判定エリアの色を変えるタイミングパラメータ（秒）
        for (int i = 0; i < circleLeftCount; i++) {
            // ノーツが可視領域に入るタイミング
            float inTime = circleLeftTimes[i] - (circleLeftTransform.position.y + 1) / noteSpeed.speed;
            leftChangeColorTimeIn[i] = Mathf.Max(inTime - judgeCircleColorTime, 0);
            // ノーツが見えなくなるタイミング
            leftChangeColorTimeOut[i] = circleLeftTimes[i];
        }
        for (int i = 0; i < circleRightCount; i++) {
            float inTime = circleRightTimes[i] - (circleRightTransform.position.y + 1) / noteSpeed.speed;
            rightChangeColorTimeIn[i] = Mathf.Max(inTime - judgeCircleColorTime, 0);
            rightChangeColorTimeOut[i] = circleRightTimes[i];
        }

        // 色を切り替えるタイミングを格納しておく
        List<float> leftChangeColorTimes = new List<float>();
        List<float> rightChangeColorTimes = new List<float>();
 
        if (circleLeftCount != 0) {
            leftChangeColorTimes.Add(leftChangeColorTimeIn[0]);
        }
        for (int i = 0; i < circleLeftCount - 1; i++) {
            if (leftChangeColorTimeIn[i + 1] <= leftChangeColorTimeOut[i]) {
                continue;
            }
            leftChangeColorTimes.Add(leftChangeColorTimeOut[i]);
            leftChangeColorTimes.Add(leftChangeColorTimeIn[i + 1]);
        }
        if (circleLeftCount != 0) {
            leftChangeColorTimes.Add(leftChangeColorTimeOut[circleLeftCount - 1]);
        } 

        if (circleRightCount != 0) {
            rightChangeColorTimes.Add(rightChangeColorTimeIn[0]);
        }
        for (int i = 0; i < circleRightCount - 1; i++) {
            if (rightChangeColorTimeIn[i + 1] <= rightChangeColorTimeOut[i]) {
                continue;
            }
            rightChangeColorTimes.Add(rightChangeColorTimeOut[i]);
            rightChangeColorTimes.Add(rightChangeColorTimeIn[i + 1]);
        }
        if (circleRightCount != 0) {
            rightChangeColorTimes.Add(rightChangeColorTimeOut[circleRightCount - 1]);
        } 
 
        bool leftFlag = false, rightFlag = false;
        foreach (float time in leftChangeColorTimes) {
            leftFlag = !leftFlag;
            StartCoroutine(CircleChangeColorCoroutine(time, 0, leftFlag));
        }
        foreach (float time in rightChangeColorTimes) {
            rightFlag = !rightFlag;
            StartCoroutine(CircleChangeColorCoroutine(time, 1, rightFlag));
        } 
    }

    // Update is called once per frame
    void Update()
    {
        musicProgress = audioSource.time / musicFile.length;
        if (musicProgress != 0) {
            musicSlider.value = musicProgress;
        } else if (musicSlider.value > 0 && !audioSource.isPlaying) {
            gameController.OnEndEvent();
            musicSlider.value = 0;
        }
    }

    IEnumerator SpawnNoteWithDelayCoroutine(float delay, float valBegin, float valEnd, int index, bool colorChange, float secBegin, float secEnd)
    {
        yield return new WaitForSeconds(delay);
        SpawnNote(valBegin, valEnd, index, colorChange, secBegin, secEnd);
    }

    public void SpawnNote(float valBegin, float valEnd, int index, bool colorChange, float secBegin, float secEnd)
    {
        if (index == 0) {
            SpawnNoteCircle(valBegin, 0, colorChange, secBegin);
        } else if (index == 6) {
            SpawnNoteCircle(valBegin, 1, colorChange, secBegin);
        } else if (valEnd == 0) {
            SpawnNoteLine(valBegin, index - 1, colorChange, secBegin);
        } else {
            SpawnNoteHold(valBegin, valEnd, index - 1, colorChange, secBegin, secEnd);
        }
    }

    // NoteLineを生成する
    public void SpawnNoteLine(float val, int index, bool colorChange, float secBegin)
    {
        if (!colorChange) {
            // 現在非表示状態のprefabを探す
            for (int i = 0; i < noteLineArray.Length; i++) {
                if (noteLineArray[i].gameObject.activeSelf == false) {
                    // 位置を指定して出現させる
                    noteLineArray[i].gameObject.transform.position = new Vector3(judgeAreaPos[index], val, 0);
                    noteLineArray[i].gameObject.SetActive(true);
                    noteLineArray[i].tag = noteTag[index];
                    noteLineArray[i].secBegin = secBegin;
                    noteLineArray[i].index = index;
                    // 一つでも見つけたらfor文を抜ける
                    break;
                }
            }
        } else {
            for (int i = 0; i < noteLine2Array.Length; i++) {
                if (noteLine2Array[i].gameObject.activeSelf == false) {
                    noteLine2Array[i].gameObject.transform.position = new Vector3(judgeAreaPos[index], val, 0);
                    noteLine2Array[i].gameObject.SetActive(true);
                    noteLine2Array[i].tag = noteTag[index];
                    noteLine2Array[i].secBegin = secBegin;
                    noteLine2Array[i].index = index;
                    break;
                }
            }
        }
    }

    // NoteHoldを生成する
    public void SpawnNoteHold(float valBegin, float valEnd, int index, bool colorChange, float secBegin, float secEnd)
    {
        float notePos = (valBegin + valEnd) / 2;
        float noteHeight = valEnd - valBegin;
        if (!colorChange) {
            // 現在非表示状態のprefabを探す
            for (int i = 0; i < noteHoldArray.Length; i++) {
                if (noteHoldArray[i].gameObject.activeSelf == false) {
                    // 位置を指定して出現させる
                    Transform noteTransform = noteHoldArray[i].gameObject.transform;
                    noteTransform.position = new Vector3(judgeAreaPos[index], notePos, 0);
                    noteHoldArray[i].gameObject.SetActive(true);
                    Transform insideLine = noteTransform.Find("Line");
                    Transform insideHold = noteTransform.Find("Hold");
                    insideLine.localPosition = new Vector3(0, -noteHeight / 2, 0);
                    insideHold.localScale = new Vector3(1, noteHeight, 1);
                    noteHoldArray[i].tag = holdTag[index];
                    noteHoldArray[i].secBegin = secBegin;
                    noteHoldArray[i].secEnd = secEnd;
                    noteHoldArray[i].index = index;
                    // 一つでも見つけたらfor文を抜ける
                    break;
                }
            }
        } else {
            for (int i = 0; i < noteHold2Array.Length; i++) {
                if (noteHold2Array[i].gameObject.activeSelf == false) {
                    Transform noteTransform = noteHold2Array[i].gameObject.transform;
                    noteTransform.position = new Vector3(judgeAreaPos[index], notePos, 0);
                    noteHold2Array[i].gameObject.SetActive(true);
                    Transform insideLine = noteTransform.Find("Line");
                    Transform insideHold = noteTransform.Find("Hold");
                    insideLine.localPosition = new Vector3(0, -noteHeight / 2, 0);
                    insideHold.localScale = new Vector3(1, noteHeight, 1);
                    noteHold2Array[i].tag = holdTag[index];
                    noteHold2Array[i].secBegin = secBegin;
                    noteHold2Array[i].secEnd = secEnd;
                    noteHold2Array[i].index = index;
                    break;
                }
            }
        }
    }

    // NoteCircleを生成する
    public void SpawnNoteCircle(float val, int index, bool colorChange, float secBegin)
    {
        if (!colorChange) {
            // 現在非表示状態のprefabを探す
            for (int i = 0; i < noteCircleArray.Length; i++) {
                if (noteCircleArray[i].gameObject.activeSelf == false) {
                    // 位置を指定して出現させる
                    Transform noteTransform = noteCircleArray[i].gameObject.transform;
                    if (index == 0) {
                        noteTransform.position = circleLeftTransform.position;
                        noteCircleArray[i].tag = "NoteLeft";
                    } else {
                        noteTransform.position = circleRightTransform.position;
                        noteCircleArray[i].tag = "NoteRight";
                    }
                    noteCircleArray[i].gameObject.SetActive(true);
                    CircleRenderer note = noteTransform.Find("Note").gameObject.GetComponent<CircleRenderer>();
                    note.radius = val + judgeCircleSize;
                    noteCircleArray[i].secBegin = secBegin;
                    noteCircleArray[i].index = index;
                    // 一つでも見つけたらfor文を抜ける
                    break;
                }
            }
        } else {
            for (int i = 0; i < noteCircle2Array.Length; i++) {
                if (noteCircle2Array[i].gameObject.activeSelf == false) {
                    Transform noteTransform = noteCircle2Array[i].gameObject.transform;
                    if (index == 0) {
                        noteTransform.position = circleLeftTransform.position;
                        noteCircle2Array[i].tag = "NoteLeft";
                    } else {
                        noteTransform.position = circleRightTransform.position;
                        noteCircle2Array[i].tag = "NoteRight";
                    }
                    noteCircle2Array[i].gameObject.SetActive(true);
                    CircleRenderer note = noteTransform.Find("Note").gameObject.GetComponent<CircleRenderer>();
                    note.radius = val + judgeCircleSize;
                    noteCircle2Array[i].secBegin = secBegin;
                    noteCircle2Array[i].index = index;
                    break;
                }
            }
        }
    }

    // judgeCircle の色を変える
    IEnumerator CircleChangeColorCoroutine(float time, int index, bool flag)
    {
        yield return new WaitForSeconds(time);
        if (index == 0) {
            if (flag) {
                circleLeftSpriteRenderer.color = new Color(1.0f, 0.5f, 0.5f, 1.0f); // 赤色
            } else {
                circleLeftSpriteRenderer.color = new Color(0.5f, 0.5f, 1.0f, 1.0f); // 青色
            }
        } else {
            if (flag) {
                circleRightSpriteRenderer.color = new Color(1.0f, 0.5f, 0.5f, 1.0f); // 赤色
            } else {
                circleRightSpriteRenderer.color = new Color(0.5f, 0.5f, 1.0f, 1.0f); // 青色
            }
        } 
    }
}
