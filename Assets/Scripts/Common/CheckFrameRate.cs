using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckFrameRate : MonoBehaviour
{
    [SerializeField] Text fpsText;

    int frameCount;
    float elapsedTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 0.5f)
        {
            float fps = 1.0f * frameCount / elapsedTime;
            fpsText.text = fps.ToString("F2");

            frameCount = 0;
            elapsedTime = 0f;
        }
    }
}
