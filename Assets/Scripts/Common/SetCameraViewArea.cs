using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraViewArea : MonoBehaviour {
    // プレイ画面で画面外の黒い部分にもタッチ判定を持たせるためのスクリプト
    // プレイ画面以外では AspectKeeper.cs を使ったほうがいいかもしれません

    [SerializeField] private Camera targetCamera; // 対象とするカメラ
    [SerializeField] private Vector2 aspectVec; // 目的解像度

    void Awake()
    {
        var screenAspect = Screen.width / (float)Screen.height; // 画面のアスペクト比
        var targetAspect = aspectVec.x / aspectVec.y; // 目的のアスペクト比
        var magRate = targetAspect / screenAspect; // 目的アスペクト比にするための倍率
        if (magRate > 1)
        {
            // 画面が縦長の時にカメラサイズを調整する
            targetCamera.orthographicSize *= magRate;
        }
        targetCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
    }
}
