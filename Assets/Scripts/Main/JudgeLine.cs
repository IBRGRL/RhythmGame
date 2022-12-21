using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeLine : MonoBehaviour
{
    // スコアやコンボの実装
    [SerializeField] GameController gameController = default;
    [SerializeField] SpriteRenderer[] judgeLineRenderers = new SpriteRenderer[5]; // 判定線の色を変えるのに使用

    Vector3[] judgeAreaPos = new Vector3[5]; // 配列の宣言
    Vector3[] judgeAreaScale = new Vector3[5]; // 配列の宣言
    string[] judgeAreaName = new string[5] {"LineZero", "LineFirst", "LineSecond", "LineThird", "LineFourth"};
    string[] noteTag = new string[5] {"NoteZero", "NoteFirst", "NoteSecond", "NoteThird", "NoteFourth"};
    string[] holdTag = new string[5] {"HoldZero", "HoldFirst", "HoldSecond", "HoldThird", "HoldFourth"};

    // 判定エフェクトの表示
    [SerializeField] GameObject lineEffectPrefab = default;
    [SerializeField] GameObject holdEffectPrefab = default;

    [System.NonSerialized] public int[] judgeWidthTime = new int[4] {50, 100, 150, 250}; // 判定幅(単位はms)

    NoteHold[] touchingHolds = new NoteHold[5]; // 触っている途中のホールドノーツを格納
    Camera cam;

    // keep a copy of the executing script
    private IEnumerator[] judgeLineCoroutines = new IEnumerator[5];
 
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 5; i++) {
            Transform judgeAreaTransform = transform.Find(judgeAreaName[i]).gameObject.transform;
            judgeAreaPos[i] = judgeAreaTransform.position;
            judgeAreaScale[i] = judgeAreaTransform.localScale;

            judgeLineRenderers[i].color = Color.clear;
        }
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // ホールドノーツを離した時の処理
        // Editor では Touch を取得できないため実機でしか正常に判定できないので注意
        if (!SongSelectController.isAuto && !gameController.isPaused) {
            for (int index = 0; index < 5; index++) {
                if (touchingHolds[index] is object) {
                    if (touchingHolds[index].gameObject.activeSelf && touchingHolds[index].holdStartFlag) {
                        int touchingPoints = 0;
                        for (int i = 0; i < Input.touchCount; i++) {
                            // タッチ数の分だけタッチ情報を確認する
                            Touch touch = Input.GetTouch(i);
                            Vector3 touchPos = cam.ScreenToWorldPoint(touch.position);
                            if (isInRect(touchPos, judgeAreaPos[index], judgeAreaScale[index])) {
                                touchingPoints++;
                                break;
                            }
                        }
                        if (touchingPoints > 0) {
                            touchingHolds[index].holdFlag = true;
                        } else {
                            touchingHolds[index].holdFlag = false;
                        }
                    } else {
                        touchingHolds[index] = null;
                    }
                }
            }
        }
    }

    bool isInRect(Vector3 target, Vector3 position, Vector3 scale)
    {
        // Box Collider 2D のサイズがオブジェクトと同じなのでこれでOK
        float xmin = position.x - scale.x / 2;
        float xmax = position.x + scale.x / 2;
        float ymin = position.y - scale.y / 2;
        float ymax = position.y + scale.y / 2;
        return (target.x > xmin && target.x < xmax) && (target.y > ymin && target.y < ymax);
    }

    // index must be 0,1,2,3,4, bool false:tap true:hold
    GameObject FindTargetNote(int index, bool type)
    {
        GameObject[] gos;
        if (!type) {
            gos = GameObject.FindGameObjectsWithTag(noteTag[index]);
        } else {
            gos = GameObject.FindGameObjectsWithTag(holdTag[index]);
        }
        GameObject closest = null;
        float min = Mathf.Infinity;
        foreach (GameObject go in gos)
        {
            float notePos = go.transform.position.y;
            if (notePos < min)
            {
                closest = go;
                min = notePos;
            }
        }
        return closest;
    }

    // タップノーツの処理
    void ClickNote(NoteLine note, int index)
    {
        float diffSec = note.secBegin - Time.timeSinceLevelLoad;
        int absDiffMilliSec = (int)(Mathf.Abs(diffSec) * 1000);
        bool isFast = diffSec > 0;
        if (absDiffMilliSec <= judgeWidthTime[3]) {
            note.gameObject.SetActive(false);
            int judge = 0; // Miss（初期値）
            if (absDiffMilliSec <= judgeWidthTime[0]) {
                judge = 1; // Perfect
            } else if (absDiffMilliSec <= judgeWidthTime[1]) {
                judge = 2; // Great
            } else if (absDiffMilliSec <= judgeWidthTime[2]) {
                judge = 3; // Good
            }
            gameController.setScore(judge, isFast, absDiffMilliSec);
            gameController.setCombo(judge);
            SpawnEffect(judgeAreaPos[index], judge);
            SetLineColor(judge, index);
        }
    }

    // ホールドノーツの（始点の）処理
    void ClickHold(NoteHold hold, int index)
    {
        float diffSec = hold.secBegin - Time.timeSinceLevelLoad;
        int absDiffMilliSec = (int)(Mathf.Abs(diffSec) * 1000);
        bool isFast = diffSec > 0;
        if (absDiffMilliSec <= judgeWidthTime[3]) {
            hold.tag = "Untagged"; // タグを変更して2回以上判定されることを防ぐ
            hold.holdStartFlag = true;
            hold.holdFlag = true;
            touchingHolds[index] = hold;

            int judge = 0; // Miss（初期値）
            if (absDiffMilliSec <= judgeWidthTime[0]) {
                judge = 1; // Perfect
            } else if (absDiffMilliSec <= judgeWidthTime[1]) {
                judge = 2; // Great
            } else if (absDiffMilliSec <= judgeWidthTime[2]) {
                judge = 3; // Good
            }
            gameController.setScore(judge, isFast, absDiffMilliSec);
            gameController.setCombo(judge);
            SpawnEffect(judgeAreaPos[index], judge);
            SetLineColor(judge, index);
            if (judge != 0) {
                hold.judgeBegin = judge;
            } else {
                hold.gameObject.SetActive(false); // Miss
                gameController.setScore(judge, isFast, absDiffMilliSec); // ノーツ2個分なので
            }
        }
    }
    
    public void OnClick(int index)
    {
        if (!SongSelectController.isAuto) {
            GameObject noteObj = FindTargetNote(index, false);
            GameObject holdObj = FindTargetNote(index, true);
            if (noteObj is object && holdObj is object) {
                NoteLine note = noteObj.GetComponent<NoteLine>();
                NoteHold hold = holdObj.GetComponent<NoteHold>();
                if (note.secBegin < hold.secBegin) {
                    ClickNote(note, index);
                } else {
                    ClickHold(hold, index);
                }
            } else if (noteObj is object) {
                NoteLine note = noteObj.GetComponent<NoteLine>();
                ClickNote(note, index);
            } else if (holdObj is object) {
                NoteHold hold = holdObj.GetComponent<NoteHold>();
                ClickHold(hold, index);
            }
        }
    }

    // NoteLine.cs と NoteHold.cs でも利用
    public void SpawnEffect(Vector3 position, int judge)
    {
        if (judge != 0) {
            Instantiate(lineEffectPrefab, position, Quaternion.identity);
        } else {
            // TODO Miss のときのエフェクトを召喚
            Debug.Log("JudgeLine: Miss effect");
        }
    }

    // NoteHold.cs で利用
    public void SpawnHoldEffect(Vector3 position, bool noteType)
    {
        GameObject effect = Instantiate(holdEffectPrefab, position, Quaternion.identity);
        ParticleSystem.MainModule judgeEffect = effect.GetComponent<ParticleSystem>().main;
        if (!noteType) {
            // noteType 1
            judgeEffect.startColor = new Color(0.5f, 1.0f, 1.0f);
        } else {
            // noteType 2
            judgeEffect.startColor = new Color(1.0f, 0.5f, 1.0f);
        }
    }

    // ノーツを叩いた際の判定によりレーンの色を変える
    // レーンの色は JudgeCircle の組み合わせとは独立に設定する必要があるので注意
    public void SetLineColor(int judge, int index)
    {
        if (judge == 1) {
            judgeLineRenderers[index].color = Color.cyan;
        } else if (judge == 2) {
            judgeLineRenderers[index].color = Color.yellow;
        } else if (judge == 3) {
            judgeLineRenderers[index].color = Color.green;
        } else if (judge == 0) {
            judgeLineRenderers[index].color = Color.red;
        }

        if (judgeLineCoroutines[index] is object) {
            StopCoroutine(judgeLineCoroutines[index]);
        }
        judgeLineCoroutines[index] = judgeLineFadeOutCoroutine(index, judgeLineRenderers[index].color, 0.30f);
        StartCoroutine(judgeLineCoroutines[index]);
    }

    IEnumerator judgeLineFadeOutCoroutine(int index, Color c, float fadeTime)
    {
        Color startColor = c;
        float time = 0;
        float maxOpacity = c.a; // 画像の不透明度
        while (time < fadeTime) {
            time += Time.deltaTime;
            startColor.a = maxOpacity * Mathf.Max(1 - time / fadeTime, 0);
            judgeLineRenderers[index].color = startColor;
            yield return null;
        }
    }
}
