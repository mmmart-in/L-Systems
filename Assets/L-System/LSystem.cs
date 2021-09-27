using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    [SerializeField] private float length;
    [SerializeField] private float angle;
    [SerializeField] private List<char> sentence = new List<char>();
    [SerializeField] private List<char> nextGenSentence = new List<char>();
    [SerializeField] private int generations;

    Vector3 startPosition;
    private float radianAngle;
    //char conversions.? 


    //need some sort of library of char translations
    private float/*??*/ ConvertCharToAngle(char c)
    {
        //expand sentence
        //easiest place to expand the sentence would be inside the switch case, i think

        float angleValue = 0f;
        switch (c)
        {
            case 'F': angleValue = 0f;
                nextGenSentence.Add('F');
                nextGenSentence.Add('R');
                nextGenSentence.Add('F');
                break;
            case 'R': angleValue = -angle;
                break;
            case 'L': angleValue = angle;
                break;

            default: angleValue = 0f;
                break;
        }
        return angleValue;
    }

    private void Start()
    {
        startPosition = transform.position;  
        //Generate();
    }
    private void DrawLine(Vector3 pointA, Vector3 pointB)
    {
        //LR creation
        GameObject go = new GameObject();
        LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
        go.transform.position = pointA;
     
        //LR positions
        lineRenderer.SetPosition(0, pointA);
        lineRenderer.SetPosition(1, pointB);
        //Color
        lineRenderer.startColor = Color.white; 
        lineRenderer.endColor = Color.black;

    }
    private void OnDrawGizmos()
    {
        
    }
    //StartPos för varje LR blir fel
    //
    public void ButtonGenerate()
    {
        Debug.Log("ButtonGenerate");
        Generate();
    }
    private void Generate()
    {
        Debug.Log("Generate:.");
        Vector3 pointA = startPosition;
        Vector3 pointB = Vector3.zero;

            for (int i = 0; i < sentence.Count; i++)
            {
                Debug.Log("For..." + i);
                //Set points
                float angleValue = ConvertCharToAngle(sentence[i]);
                radianAngle = Mathf.Deg2Rad * ConvertCharToAngle(sentence[i]);

                Vector3 aWithLength = pointA + (Vector3.up * length);

                pointB.x = aWithLength.x * Mathf.Cos(angleValue) - aWithLength.y * Mathf.Sin(angleValue);
                pointB.y = aWithLength.y * Mathf.Cos(angleValue) + aWithLength.x * Mathf.Sin(angleValue);


                //Debug.Log("pointB before length multiplication" + pointB);







                Debug.Log("PointA is :" + pointA + "in iter : " + i);
                Debug.Log("PointB is: " + pointB);
                //DrawLine
                DrawLine(pointA, pointB);

                //Get start point for next lap
                pointA = pointB;
            }

            sentence = nextGenSentence;
         

    }


}
