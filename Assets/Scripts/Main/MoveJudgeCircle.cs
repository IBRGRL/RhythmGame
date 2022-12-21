using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveJudgeCircle : MonoBehaviour
{
    // アニメーションを記述したjsonファイルは時間について昇順に書くこと
    
    [SerializeField] Transform circleLeftTransform;
    [SerializeField] Transform circleRightTransform;
    [SerializeField] Transform connectLineTransform;
    [SerializeField] SpriteRenderer connectLineRenderer;

    [System.Serializable]
    public class CircleMove
    {
        public int from;      // 変化前の値
        public int to;        // 変化後の値
        public int[] atime;   // [開始時間, 終了時間]
        public float[] trans; // cubic-bezier
        public int[] repeat;  // [リピート回数, 時間間隔, 往復かどうか]
    }
    
    [System.Serializable]
    public class CircleAnimation
    {
        public CircleMove[] left;
        public CircleMove[] right;
    }

    CircleAnimation circleAnimation = default;
    private int leftNum, rightNum;

    private float xmin, xmax, circleHeight, lineThickness;
    private int userOffset;
 
    private int leftIndex, rightIndex;

    private bool leftInitFlag, rightInitFlag;
    private int milliSecBeginLeft, milliSecEndLeft, posFromLeft, posToLeft, repeatCountLeft;
    private float mX1L, mY1L, mX2L, mY2L, mX1R, mY1R, mX2R, mY2R;
    private int milliSecBeginRight, milliSecEndRight, posFromRight, posToRight, repeatCountRight;

    /**
     * https://github.com/gre/bezier-easing
     * BezierEasing - use bezier curve for transition easing function
     * by Gaëtan Renaudeau 2014 - 2015 – MIT License
     */
    
    // These values are established by empiricism with tests (tradeoff: performance VS precision)
    const int NEWTON_ITERATIONS = 4;
    const float NEWTON_MIN_SLOPE = 0.001f;
    const float SUBDIVISION_PRECISION = 0.0000001f;
    const int SUBDIVISION_MAX_ITERATIONS = 10;
    
    const int kSplineTableSize = 11;
    const float kSampleStepSize = 1.0f / (kSplineTableSize - 1.0f);
    
    void Awake()
    {
        // アニメーションデータの読み込み
        string songTitle = SongSelectController.songTitle; // Song Select Sceneから曲名を読み込む 
        string difficulty = SongSelectController.difficulty.ToString(); // 難易度を読み込む
        TextAsset jsonTextFile = Resources.Load<TextAsset>("Charts/" + songTitle + "/a" + difficulty);
        string data = jsonTextFile.text;
        circleAnimation = JsonUtility.FromJson<CircleAnimation>(data);

        leftNum = circleAnimation.left.Length;
        rightNum = circleAnimation.right.Length;
    }

    // Start is called before the first frame update
    void Start()
    {
        xmin = circleLeftTransform.position.x;  // 左の円の初期位置を左端とする
        xmax = circleRightTransform.position.x; // 右の円の初期位置を右端とする
        circleHeight = circleLeftTransform.position.y;
        lineThickness = connectLineRenderer.size.y;

        leftIndex = rightIndex = 0;
        leftInitFlag = rightInitFlag = true;

        userOffset = (int)(OffsetSlider.offset * 1000); // Song Select Scene で設定した値をmsに変換して読み込む 
    }

    // Update is called once per frame
    void Update()
    {
        // アニメーションの実行 
        int currentMilliSec = (int)(Time.timeSinceLevelLoad * 1000);
        bool anyMoveFlag = false;

        if (leftIndex < leftNum) { 
            if (leftInitFlag) {
                leftInitFlag = false;
                milliSecBeginLeft = circleAnimation.left[leftIndex].atime[0] + userOffset;
                milliSecEndLeft = circleAnimation.left[leftIndex].atime[1] + userOffset;
                posFromLeft = circleAnimation.left[leftIndex].from;
                posToLeft = circleAnimation.left[leftIndex].to;
                repeatCountLeft = 1;
                if (circleAnimation.left[leftIndex].trans is object) {
                    mX1L = circleAnimation.left[leftIndex].trans[0];
                    mY1L = circleAnimation.left[leftIndex].trans[1];
                    mX2L = circleAnimation.left[leftIndex].trans[2];
                    mY2L = circleAnimation.left[leftIndex].trans[3];
                } else {
                    mX1L = mY1L = mX2L = mY2L = 0;
                }
            }
            if (currentMilliSec >= milliSecBeginLeft) {
                MoveCircle(true, currentMilliSec, milliSecBeginLeft, milliSecEndLeft, posFromLeft, posToLeft, mX1L, mY1L, mX2L, mY2L);
                anyMoveFlag = true;
                if (circleAnimation.left[leftIndex].repeat is object) {
                    if (currentMilliSec >= milliSecEndLeft && repeatCountLeft < circleAnimation.left[leftIndex].repeat[0]) {
                        {
                            int tmp = milliSecBeginLeft;
                            milliSecBeginLeft = milliSecEndLeft + circleAnimation.left[leftIndex].repeat[1];
                            milliSecEndLeft = milliSecEndLeft + milliSecBeginLeft - tmp;
                        }
                        if (circleAnimation.left[leftIndex].repeat[2] != 0) { // 往復の場合
                            int tmp = posFromLeft;
                            posFromLeft = posToLeft;
                            posToLeft = tmp;
                        }
                        repeatCountLeft++;
                    }
                }
            }
            if (currentMilliSec >= milliSecEndLeft) {
                leftIndex++;
                leftInitFlag = true;
            }
        }

        if (rightIndex < rightNum) { 
            if (rightInitFlag) {
                rightInitFlag = false;
                milliSecBeginRight = circleAnimation.right[rightIndex].atime[0] + userOffset;
                milliSecEndRight = circleAnimation.right[rightIndex].atime[1] + userOffset;
                posFromRight = circleAnimation.right[rightIndex].from;
                posToRight = circleAnimation.right[rightIndex].to;
                repeatCountRight = 1;
                if (circleAnimation.right[rightIndex].trans is object) {
                    mX1R = circleAnimation.right[rightIndex].trans[0];
                    mY1R = circleAnimation.right[rightIndex].trans[1];
                    mX2R = circleAnimation.right[rightIndex].trans[2];
                    mY2R = circleAnimation.right[rightIndex].trans[3];
                } else {
                    mX1R = mY1R = mX2R = mY2R = 0;
                }
            }
            if (currentMilliSec >= milliSecBeginRight) {
                MoveCircle(false, currentMilliSec, milliSecBeginRight, milliSecEndRight, posFromRight, posToRight, mX1R, mY1R, mX2R, mY2R);
                anyMoveFlag = true;
                if (circleAnimation.right[rightIndex].repeat is object) {
                    if (currentMilliSec >= milliSecEndRight && repeatCountRight < circleAnimation.right[rightIndex].repeat[0]) {
                        {
                            int tmp = milliSecBeginRight;
                            milliSecBeginRight = milliSecEndRight + circleAnimation.right[rightIndex].repeat[1];
                            milliSecEndRight = milliSecEndRight + milliSecBeginRight - tmp;
                        }
                        if (circleAnimation.right[rightIndex].repeat[2] != 0) { // 往復の場合
                            int tmp = posFromRight;
                            posFromRight = posToRight;
                            posToRight = tmp;
                        }
                        repeatCountRight++;
                    }
                }
            }
            if (currentMilliSec >= milliSecEndRight) {
                rightIndex++;
                rightInitFlag = true;
            }
        }
        
        if (anyMoveFlag) {
            // connectLine の長さ変更
            float midPosition = (circleLeftTransform.position.x + circleRightTransform.position.x) / 2;
            float lineSize = circleRightTransform.position.x - circleLeftTransform.position.x;
            connectLineTransform.position = new Vector3(midPosition, circleHeight, 0);
            connectLineRenderer.size = new Vector2(lineSize, lineThickness);
        }
    }

    void MoveCircle(bool isLeft, int currentMilliSec, int milliSecBegin, int milliSecEnd, int posFrom, int posTo, float mX1, float mY1, float mX2, float mY2)
    {
        float animationProgress = (float)(currentMilliSec - milliSecBegin) / (milliSecEnd - milliSecBegin);
        float posBegin = xmin + (xmax - xmin) * posFrom / 100;
        float posEnd = xmin + (xmax - xmin) * posTo / 100; 
        animationProgress = Mathf.Min(animationProgress, 1.0f); // 行き過ぎ防止
        float transition = BezierEasing(animationProgress, mX1, mY1, mX2, mY2);
        float currentPos = posBegin + (posEnd - posBegin) * transition;
        if (isLeft) {
            circleLeftTransform.position = new Vector3(currentPos, circleHeight, 0);
        } else {
            circleRightTransform.position = new Vector3(currentPos, circleHeight, 0);
        }
    }

    float BezierEasing(float aX, float mX1, float mY1, float mX2, float mY2)
    {
        if (mX1 == mY1 && mX2 == mY2) {
            return aX;
        }

        // Precompute samples table
        float[] sampleValues = new float[kSplineTableSize];
        for (int i = 0; i < kSplineTableSize; ++i) {
            sampleValues[i] = calcBezier(i * kSampleStepSize, mX1, mX2);
        }

        // get T for X
        float aT = 0;
        float intervalStart = 0;
        int currentSample = 1;
        int lastSample = kSplineTableSize - 1;
    
        for (; currentSample != lastSample && sampleValues[currentSample] <= aX; ++currentSample) {
            intervalStart += kSampleStepSize;
        }
        --currentSample;
    
        // Interpolate to provide an initial guess for t
        float dist = (aX - sampleValues[currentSample]) / (sampleValues[currentSample + 1] - sampleValues[currentSample]);
        float guessForT = intervalStart + dist * kSampleStepSize;
    
        float initialSlope = getSlope(guessForT, mX1, mX2);
        if (initialSlope >= NEWTON_MIN_SLOPE) {
            aT = newtonRaphsonIterate(aX, guessForT, mX1, mX2);
        } else if (initialSlope == 0) {
            aT = guessForT;
        } else {
            aT = binarySubdivide(aX, intervalStart, intervalStart + kSampleStepSize, mX1, mX2);
        }

        // Because JavaScript number are imprecise, we should guarantee the extremes are right.
        if (aX == 0 || aX == 1.0f) {
            return aX;
        }
        return calcBezier(aT, mY1, mY2);
    }

    float A(float aA1, float aA2) { return 1.0f - 3.0f * aA2 + 3.0f * aA1; }
    float B(float aA1, float aA2) { return 3.0f * aA2 - 6.0f * aA1; }
    float C(float aA1)            { return 3.0f * aA1; }
    
    // Returns x(t) given t, x1, and x2, or y(t) given t, y1, and y2.
    float calcBezier(float aT, float aA1, float aA2) { return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT; }
    
    // Returns dx/dt given t, x1, and x2, or dy/dt given t, y1, and y2.
    float getSlope(float aT, float aA1, float aA2) { return 3.0f * A(aA1, aA2) * aT * aT + 2.0f * B(aA1, aA2) * aT + C(aA1); }

    float binarySubdivide(float aX, float aA, float aB, float mX1, float mX2) {
        float currentX = 0, currentT = 0, i = 0;
        do {
            currentT = aA + (aB - aA) / 2.0f;
            currentX = calcBezier(currentT, mX1, mX2) - aX;
            if (currentX > 0) {
              aB = currentT;
            } else {
              aA = currentT;
            }
        } while (Mathf.Abs(currentX) > SUBDIVISION_PRECISION && ++i < SUBDIVISION_MAX_ITERATIONS);
        return currentT;
    }
    
    float newtonRaphsonIterate(float aX, float aGuessT, float mX1, float mX2) {
        for (int i = 0; i < NEWTON_ITERATIONS; ++i) {
            float currentSlope = getSlope(aGuessT, mX1, mX2);
            if (currentSlope == 0) {
                return aGuessT;
            }
            float currentX = calcBezier(aGuessT, mX1, mX2) - aX;
            aGuessT -= currentX / currentSlope;
        }
        return aGuessT;
    }
}
