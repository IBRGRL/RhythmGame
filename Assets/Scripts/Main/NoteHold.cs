using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteHold : MonoBehaviour
{
    // ノーツに対応する時間
    public float secBegin;
    public float secEnd;
    public bool holdStartFlag;
    public bool holdFlag;
    public int judgeBegin;

    // ノーツに対応するindex
    public int index;

    // 自身のノーツの色がどちらかを表す変数
    public bool noteType;

    public GameController gameController;

    private NoteSpeed noteSpeed;
    private float speed;

    // 判定エフェクトの表示に利用
    private JudgeLine judgeLine;
    private float effectTime;

    // ホールドを途中で離していた時間
    private float holdTime;
 
    // 叩かなかったノーツの削除に利用
    private int threshold;

    private Transform myTransform;

    [SerializeField] Transform lineTransform;
    [SerializeField] Transform holdTransform;

    private float noteHorizontalPos;
    
    // Start is called before the first frame update
    void Start()
    {
        this.noteSpeed = FindObjectOfType<NoteSpeed>(); // インスタンス化
        speed = noteSpeed.speed;
        
        // コンポーネントからGameControllerを検出する
        gameController = GameObject.Find("GameController").GetComponent<GameController>();

        judgeLine = GameObject.Find("JudgeLine").GetComponent<JudgeLine>();
        threshold = -judgeLine.judgeWidthTime[2]; // missになる判定幅 * (-1)
    }

    private void Awake()
    {
        myTransform = this.transform;
    }

    private void OnEnable()
    {
        holdStartFlag = holdFlag = false;
        judgeBegin = 0;
        effectTime = 1.0f; // HoldEffect の表示を開始するタイミングパラメータ
        holdTime = 0;
        
        noteHorizontalPos = myTransform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        // 1秒あたり speed だけ下に落ちる
        float noteBeginPos = (secBegin - Time.timeSinceLevelLoad) * speed;
        float noteEndPos = (secEnd - Time.timeSinceLevelLoad) * speed;
        UpdateNotePos(noteBeginPos, noteEndPos);

        if (holdStartFlag) {
            noteBeginPos = 0;
            UpdateNotePos(noteBeginPos, noteEndPos);
           
            int endDiffMilliSec = (int)((secEnd - Time.timeSinceLevelLoad) * 1000);
            if (endDiffMilliSec <= judgeLine.judgeWidthTime[0]) { // Perfect
                this.gameObject.SetActive(false);
                gameController.setScore(1, false, Mathf.Abs(endDiffMilliSec));
                gameController.setCombo(1);
                SpawnEffect(1);
                SetLineColor(1);
            } else {
                if (holdFlag) {
                    // TODO できればノーツの色を変えたい？
                    effectTime += Time.deltaTime;
                    if (effectTime > 0.2f) { // ホールド中のエフェクトを控え目に出す 数字は適当
                        effectTime = 0;
                        SpawnHoldEffect();
                    }
                    holdTime = 0;
                } else {
                    // ホールドを途中で離したとき
                    holdTime += Time.deltaTime;
                }
            }

            if (holdTime > 0.15f) { // ホールドを0.15秒以上離したらミスとする 数字は適当
                this.gameObject.SetActive(false);
                int judge = 0; // Miss（初期値）
                if (endDiffMilliSec <= judgeLine.judgeWidthTime[1]) {
                    judge = 2; // Great
                } else if (endDiffMilliSec <= judgeLine.judgeWidthTime[2]) {
                    judge = 3; // Good
                }
                gameController.setScore(judge, true, Mathf.Abs(endDiffMilliSec));
                gameController.setCombo(judge);
                SpawnEffect(judge);
                SetLineColor(judge);
            }
        }

        int beginDiffMilliSec = (int)((secBegin - Time.timeSinceLevelLoad) * 1000);
        if (!holdStartFlag && beginDiffMilliSec < threshold) {
            this.gameObject.SetActive(false);
            gameController.setScore(0, false, -1); // そもそもノーツを叩かなかった時は時間差 -1 ms とする
            gameController.setScore(0, false, -1); // ノーツ2個分なので
            gameController.setCombo(0);
            SpawnEffect(0);
            SetLineColor(0);
        }

        // AUTO モードのとき
        if (SongSelectController.isAuto && beginDiffMilliSec <= 0 && !holdStartFlag) {
            holdStartFlag = true;
            holdFlag = true;
            judgeBegin = 1;
            gameController.setScore(1, false, 0); // AUTO なので時間差はもちろん 0ms
            gameController.setCombo(1);
            SpawnEffect(1);
            SetLineColor(1);
        }
    }

    void UpdateNotePos(float noteBeginPos, float noteEndPos) {
        float notePos = (noteBeginPos + noteEndPos) / 2;
        float noteHeight = noteEndPos - noteBeginPos;
        myTransform.position = new Vector3(noteHorizontalPos, notePos, 0);
        lineTransform.localPosition = new Vector3(0, -noteHeight / 2, 0);
        holdTransform.localScale = new Vector3(1, noteHeight, 1);
    }

    void SpawnEffect(int judge) {
        judgeLine.SpawnEffect(Vector3.right * noteHorizontalPos, judge);
    }

    void SpawnHoldEffect() {
        judgeLine.SpawnHoldEffect(Vector3.right * noteHorizontalPos, noteType);
    }

    void SetLineColor(int judge) {
        judgeLine.SetLineColor(judge, index);
    }
}
