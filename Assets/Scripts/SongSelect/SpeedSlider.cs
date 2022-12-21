using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    Slider speedSlider;
    [SerializeField] Text speedText;
    
    public static float speed = 1.2f; // 別シーンからでも参照できるようにstatic

    // Start is called before the first frame update
    void Start()
    {
        speedSlider = GetComponent<Slider>();
        speedSlider.value = speed; 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnChange()
    {
        speed = Mathf.Round(speedSlider.value * 10) / 10;
        speedText.text = speed.ToString("f1");
    }

    public void OnClickButtonPlus()
    {
        if (speed < speedSlider.maxValue) {
            speed += 0.1f;
            speedSlider.value = speed; 
            speedText.text = speed.ToString("f1");
        }
    }

    public void OnClickButtonMinus()
    {
        if (speed > speedSlider.minValue) {
            speed -= 0.1f;
            speedSlider.value = speed; 
            speedText.text = speed.ToString("f1");
        }
    }
}
