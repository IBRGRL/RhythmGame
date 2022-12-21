using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTouchManager : MonoBehaviour
{
    [SerializeField] JudgeCircle judgeCircle;

    // タッチしている間は判定円の色を変える処理を行う
    [SerializeField] SpriteRenderer circleLeftRenderer;
    [SerializeField] SpriteRenderer circleRightRenderer;

    private int onTouchLeftBefore;
    private int onTouchRightBefore;

    [SerializeField] GameController gameController = default;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        onTouchLeftBefore = onTouchRightBefore = 0;    
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameController.isPaused) {
            int onTouchLeft = 0;
            int onTouchRight = 0;

            // Editor では Touch を取得できないため実機でしか正常に判定できないので注意
            for (int i = 0; i < Input.touchCount; i++) {
                // タッチ数の分だけタッチ情報を確認する
                Touch touch = Input.GetTouch(i);
                Vector2 touchPos = cam.ScreenToWorldPoint(touch.position);
                foreach (RaycastHit2D hit in Physics2D.RaycastAll(touchPos, Vector2.zero)) {
                    // オブジェクトが見つかったときの処理
                    if (hit) {
                        // 見つかったオブジェクトが判定円なら
                        if (hit.collider.gameObject.CompareTag("CircleLeft")) {
                            onTouchLeft++;
                            if (onTouchLeft > onTouchLeftBefore) {
                                judgeCircle.OnClick(0);
                            }
                        }
                        if (hit.collider.gameObject.CompareTag("CircleRight")) {
                            onTouchRight++;
                            if (onTouchRight > onTouchRightBefore) {
                                judgeCircle.OnClick(1);
                            }
                        }
                    }
                }
            }

            if (onTouchLeft > 0) {
                circleLeftRenderer.color = new Color(0.75f, 0.75f, 0.75f);
            } else {
                circleLeftRenderer.color = Color.white;
            }
            if (onTouchRight > 0) {
                circleRightRenderer.color = new Color(0.75f, 0.75f, 0.75f);
            } else {
                circleRightRenderer.color = Color.white;
            }
            onTouchLeftBefore = onTouchLeft;
            onTouchRightBefore = onTouchRight;
        }
    }
}
