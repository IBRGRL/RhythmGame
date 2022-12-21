using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeCircle : MonoBehaviour
{
    // スコアやコンボの実装
    [SerializeField] GameController gameController = default;

    // 子オブジェクトの円
    [SerializeField] GameObject[] circles = new GameObject[2];
    private Transform[] circleTransforms = new Transform[2];

    // 判定エフェクトの表示
    [SerializeField] GameObject circleEffectPrefab = default;

    // 判定幅の取得に利用
    [SerializeField] JudgeLine judgeLine;

    // 判定線の色を変えるのに使用
    [SerializeField] GameObject[] circleEffects = new GameObject[2];
    private Transform[] circleEffectTransforms = new Transform[2];
    private SpriteRenderer[] judgeCircleRenderers = new SpriteRenderer[2]; 
    private Animator[] judgeCircleAnimators = new Animator[2];

    string[] noteTag = new string[2] {"NoteLeft", "NoteRight"};

    int[] judgeWidthTime = new int[4]; // JudgeLineと同じ判定幅を用いる

    // keep a copy of the executing script
    private IEnumerator[] judgeCircleCoroutines = new IEnumerator[2];
 
    private int[] judgeCircleExpandHashs = new int[2];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; i++) {
            judgeWidthTime[i] = judgeLine.judgeWidthTime[i];
        }

        for (int i = 0; i < 2; i++) {
            circleTransforms[i] = circles[i].transform;
            circleEffectTransforms[i] = circleEffects[i].transform;

            judgeCircleRenderers[i] = circleEffects[i].GetComponent<SpriteRenderer>();
            judgeCircleRenderers[i].color = Color.clear;
            judgeCircleAnimators[i] = circleEffects[i].GetComponent<Animator>();
        }

        judgeCircleExpandHashs[0] = Animator.StringToHash("Base Layer.CircleEffectLeft@expand");
        judgeCircleExpandHashs[1] = Animator.StringToHash("Base Layer.CircleEffectRight@expand");
    }

    // Update is called once per frame
    void Update()
    {

    }

    GameObject FindTargetNote(int index)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(noteTag[index]);
        GameObject closest = null;
        float min = Mathf.Infinity;
        foreach (GameObject go in gos)
        {
            CircleRenderer note = go.transform.Find("Note").gameObject.GetComponent<CircleRenderer>();
            if (note.radius < min)
            {
                closest = go;
                min = note.radius;
            }
        }
        return closest;
    }

    public void OnClick(int index)
    {
        if (!SongSelectController.isAuto) {
            GameObject noteObj = FindTargetNote(index);
            if (noteObj is object) {
                NoteCircle note = noteObj.GetComponent<NoteCircle>();
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
                    SpawnEffect(circleTransforms[index].position, judge, index);
                    SetCircleColor(judge, index);
                }
            }
        }
    }

    // NoteCircle.cs でも利用
    public void SpawnEffect(Vector3 position, int judge, int index)
    {
        if (judge != 0) {
            GameObject circleEffect = Instantiate(circleEffectPrefab, position, Quaternion.identity);
            circleEffect.transform.parent = circleTransforms[index]; // 親子関係を設定してエフェクトが判定円に追従するようにする
            circleEffect.transform.localScale = Vector3.one;
        } else {
            // TODO Miss のときのエフェクトを召喚
            Debug.Log("JudgeCircle: Miss effect");
        }
    }

    // ノーツを叩いた際の判定によりレーンの色を変える
    // レーンの色は JudgeLine の組み合わせとは独立に設定する必要があるので注意
    public void SetCircleColor(int judge, int index)
    {
        // エフェクトの発生位置を合わせる
        circleEffectTransforms[index].position = circleTransforms[index].position;
          
        if (judge == 1) {
            judgeCircleRenderers[index].color = Color.cyan;
        } else if (judge == 2) {
            judgeCircleRenderers[index].color = Color.yellow;
        } else if (judge == 3) {
            judgeCircleRenderers[index].color = Color.green;
        } else if (judge == 0) {
            judgeCircleRenderers[index].color = Color.red;
        }

        if (judgeCircleCoroutines[index] is object) {
            StopCoroutine(judgeCircleCoroutines[index]);
        }
        judgeCircleCoroutines[index] = judgeCircleFadeOutCoroutine(index, judgeCircleRenderers[index].color, 0.10f, 0.10f);
        StartCoroutine(judgeCircleCoroutines[index]);

        // effect が拡大するアニメーション
        judgeCircleAnimators[index].Play(judgeCircleExpandHashs[index], 0, 0.0f);
    }

    IEnumerator judgeCircleFadeOutCoroutine(int index, Color c, float waitTime, float fadeTime)
    {
        yield return new WaitForSeconds(waitTime);
        Color startColor = c;
        float time = 0;
        float maxOpacity = c.a; // 画像の不透明度
        while (time < fadeTime) {
            time += Time.deltaTime;
            startColor.a = maxOpacity * Mathf.Max(1 - time / fadeTime, 0);
            judgeCircleRenderers[index].color = startColor;
            yield return null;
        }
    }
}
