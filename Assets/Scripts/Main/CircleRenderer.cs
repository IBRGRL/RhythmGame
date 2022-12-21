using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    private int segments;
    public float radius;

    private float lineWidth;
    private float edgeWidth;

    [SerializeField] LineRenderer line;
    [SerializeField] LineRenderer edge;
       
    // Start is called before the first frame update
    void Start()
    {
        segments = 60; // とりあえず 360 / 6
        lineWidth = 0.2f; // 線の幅
        edgeWidth = 0.3f; // 縁も含めた幅
        
        line.positionCount = edge.positionCount = segments + 1;
        line.startWidth = line.endWidth = lineWidth;
        edge.startWidth = edge.endWidth = edgeWidth;
        line.useWorldSpace = edge.useWorldSpace = false;
    }
   
    // Update is called once per frame
    void Update()
    {
        CreatePoints();
    }

    void CreatePoints()
    {
        float x;
        float y;
        float z = 0f;
       
        float angle = 0f;
       
        for (int i = 0; i < (segments + 1); i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle);
            y = Mathf.Cos(Mathf.Deg2Rad * angle);
            line.SetPosition(i, new Vector3(x, y, z) * radius);
            edge.SetPosition(i, new Vector3(x, y, z) * radius);
            angle += (360f / segments);
        }
    }
}
