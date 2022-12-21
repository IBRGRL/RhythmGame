using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCircle : MonoBehaviour
{
    // ノーツに対応する時間
    public float secBegin;

    // ノーツに対応するindex
    public int index;

    // スコアやコンボの実装
    public GameController gameController;
    
    Transform circleLeftTransform;
    Transform circleRightTransform;
    bool isLeft;

    GameObject inCircleLeft;
    [SerializeField] GameObject noteObj;
    private CircleRenderer note;

    // 判定幅の取得に利用
    private JudgeLine judgeLine;

    // 判定エフェクトの表示および判定円の色の変更に利用
    private JudgeCircle judgeCircle;
 
    private NoteSpeed noteSpeed;

    private float judgeCircleSize;
    private int threshold;

    private Transform myTransform;

    [SerializeField] LineRenderer noteRenderer;
    [SerializeField] LineRenderer edgeRenderer;

    private float noteEnableSize;

    private Color noteColor;
    private Color edgeColor;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = this.transform;
        note = noteObj.GetComponent<CircleRenderer>();

        //コンポーネントからGameControllerを検出する
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        circleLeftTransform = GameObject.Find("CircleLeft").transform;
        circleRightTransform = GameObject.Find("CircleRight").transform;
 
        judgeLine = GameObject.Find("JudgeLine").GetComponent<JudgeLine>();
        judgeCircle = GameObject.Find("JudgeCircle").GetComponent<JudgeCircle>();

        inCircleLeft = GameObject.Find("CircleLeft/Circle");

        this.noteSpeed = FindObjectOfType<NoteSpeed>(); // インスタンス化

        judgeCircleSize = NoteGenerator.judgeCircleSize;
        threshold = -judgeLine.judgeWidthTime[2]; // missになる判定幅 * (-1)

        noteEnableSize = judgeCircleSize + NoteGenerator.cameraHeight; // ノーツを表示させる閾値

        noteColor = noteRenderer.startColor; // Note が透明度1のときの色
        edgeColor = edgeRenderer.startColor; // Edge が透明度1のときの色
    }

    private void OnEnable()
    {
        noteRenderer.enabled = edgeRenderer.enabled = false;
        isLeft = this.gameObject.CompareTag("NoteLeft");
    }

    // Update is called once per frame
    void Update()
    {
        if (note.radius < noteEnableSize) { 
            noteRenderer.enabled = edgeRenderer.enabled = true;
            float proportion = (noteEnableSize - note.radius) / (noteEnableSize - judgeCircleSize);
            proportion = Mathf.Min(proportion, 1.0f); // 行き過ぎ防止
            float opacity = 1 - Mathf.Pow(1 - proportion, 5); // easeOutQuint
            noteColor.a = edgeColor.a = opacity;
            noteRenderer.startColor = noteRenderer.endColor = noteColor;
            edgeRenderer.startColor = edgeRenderer.endColor = edgeColor;
        }

        // Note の位置を判定円と一致させる
        myTransform.position = isLeft ? circleLeftTransform.position : circleRightTransform.position;

        // radius を1秒あたり speed だけ縮小する
        note.radius = judgeCircleSize + (secBegin - Time.timeSinceLevelLoad) * noteSpeed.speed;

        int diffMilliSec = (int)((secBegin - Time.timeSinceLevelLoad) * 1000);

        if (diffMilliSec < threshold) {
            this.gameObject.SetActive(false);
            gameController.setScore(0, false, -1); // そもそもノーツを叩かなかった時は時間差 -1 ms とする
            gameController.setCombo(0);
            SpawnEffect(0);
            SetCircleColor(0);
        }

        // AUTO モードのとき
        if (SongSelectController.isAuto && diffMilliSec <= 0) {
            this.gameObject.SetActive(false);
            gameController.setScore(1, false, 0); // AUTO なので時間差はもちろん 0ms
            gameController.setCombo(1);
            SpawnEffect(1);
            SetCircleColor(1);
        }
    }

    void SpawnEffect(int judge) {
        judgeCircle.SpawnEffect(myTransform.position, judge, index);
    }

    void SetCircleColor(int judge) {
        judgeCircle.SetCircleColor(judge, index);
    }
}
