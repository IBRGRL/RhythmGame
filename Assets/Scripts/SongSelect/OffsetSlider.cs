using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffsetSlider : MonoBehaviour
{
    Slider offsetSlider;
    [SerializeField] Text offsetText;
    
    public static float offset = 0; // 別シーンからでも参照できるようにstatic

    // Start is called before the first frame update
    void Start()
    {
        offsetSlider = GetComponent<Slider>();
        offsetSlider.value = offset; 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnChange()
    {
        offset = Mathf.Round(offsetSlider.value * 100) / 100;
        offsetText.text = offset.ToString("f2");
    }

    public void OnClickButtonPlus()
    {
        if (offset < offsetSlider.maxValue) {
            offset += 0.01f;
            offsetSlider.value = offset; 
            offsetText.text = offset.ToString("f2");
        }
    }

    public void OnClickButtonMinus()
    {
        if (offset > offsetSlider.minValue) {
            offset -= 0.01f;
            offsetSlider.value = offset; 
            offsetText.text = offset.ToString("f2");
        }
    }
}
