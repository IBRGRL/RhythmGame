using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundImage : MonoBehaviour
{
    private Image image;
    private Sprite sprite;

    void Awake()
    {
        string songTitle = SongSelectController.songTitle; // Song Select Sceneから曲名を読み込む 
        sprite = Resources.Load<Sprite>("Artworks/" + songTitle);
        image = this.GetComponent<Image>();
        image.sprite = sprite;    
    }
}
