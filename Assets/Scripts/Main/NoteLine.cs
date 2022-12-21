using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteLine : MonoBehaviour
{
    // ノーツに対応する時間
    public float secBegin;

    // ノーツに対応するindex
    public int index;

    public GameController gameController;

    private NoteSpeed noteSpeed;
    private float speed;

    // 判定エフェクトの表示に利用
    private JudgeLine judgeLine;
 
    // 叩かなかったノーツの削除に利用
    private int threshold;

    private Transform myTransform;
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
        noteHorizontalPos = myTransform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        // 1秒あたり speed だけ下に落ちる
        float notePos = (secBegin - Time.timeSinceLevelLoad) * speed;
        myTransform.position = new Vector3(noteHorizontalPos, notePos, 0);
        int diffMilliSec = (int)((secBegin - Time.timeSinceLevelLoad) * 1000);

        if (diffMilliSec < threshold) {
            this.gameObject.SetActive(false);
            gameController.setScore(0, false, -1); // そもそもノーツを叩かなかった時は時間差 -1 ms とする
            gameController.setCombo(0);
            SpawnEffect(0);
            SetLineColor(0);
        }

        // AUTO モードのとき
        if (SongSelectController.isAuto && diffMilliSec <= 0) {
            this.gameObject.SetActive(false);
            gameController.setScore(1, false, 0); // AUTO なので時間差はもちろん 0ms
            gameController.setCombo(1);
            SpawnEffect(1);
            SetLineColor(1);
        }
    }

    void SpawnEffect(int judge) {
        judgeLine.SpawnEffect(Vector3.right * noteHorizontalPos, judge);
    }

    void SetLineColor(int judge) {
        judgeLine.SetLineColor(judge, index);
    }
}
