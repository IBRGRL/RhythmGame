using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpeed : MonoBehaviour
{
    // speed をプロパティにより管理
    public float speed {get; set;}

    void Awake()
    {
        speed = SpeedSlider.speed * 13; // Song Select Scene で設定した値を読み込む 
    }
}
