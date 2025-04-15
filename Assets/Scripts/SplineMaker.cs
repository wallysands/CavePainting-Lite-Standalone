using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineMaker : MonoBehaviour
{
    [SerializeField] private Transform m_ArtworkParentTransform;
    [SerializeField] public TextAsset testAssetData;
    public SplineContainer m_splineContainer;
    public SpatialGrid m_spatialGrid;

    // Start is called before the first frame update
    void Start()
    {
        ReadCSV(testAssetData);
        m_splineContainer.transform.SetParent(m_ArtworkParentTransform, false);
        m_spatialGrid.Init();
    }

    void ReadCSV(TextAsset textAsset)
    {
        string[] data = textAsset.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        Spline spline = m_splineContainer.AddSpline();
        List<BezierKnot> knots = new List<BezierKnot>();
        float m_SplineTension = 1/3f;

        int rowNum = 0;
        bool firstEmpty = true;

        foreach (string row in data)
        {
            // Ignore assumed header, should instead test the string itself instead of assuming header
            if (rowNum != 0)
            {
                string[] rowValues = row.Split(new string[] { "," }, StringSplitOptions.None);

                // Test that values exist in the row
                if (rowValues[0].Length > 0)
                {
                    
                    float integrationTime = float.Parse(rowValues[0]);
                    float x = float.Parse(rowValues[1]);
                    float y = float.Parse(rowValues[2]);
                    float z = float.Parse(rowValues[3]);

                    // Check if starting a new vector
                    if (integrationTime == 0 && rowNum != 1)
                    {
                        spline.Knots = knots;
                        SplineRange all = new SplineRange(0, spline.Count); // is not working to smooth, revisit
                        spline.SetTangentMode(all, TangentMode.AutoSmooth);
                        spline.SetAutoSmoothTension(all, m_SplineTension);

                        // spline.Add(knots[0]);

                        // for (int i = 1; i < knots.Count-1; i++)
                        // {
                            
                        //     // var T = normalize(P[i+1] ​− P[i−1]​) * length(P - P[i-1]) / 3
                        //     // var T = (knots[i+1].Position - knots[i-1].Position) / 3;
                        //     var TIn = Vector3.Normalize(knots[i+1].Position - knots[i-1].Position) * Vector3.Distance(knots[i].Position,knots[i-1].Position) / 3;//normalize(P[i+1] ​− P[i−1]​) * length(P - P[i-1]) / 3;
                        //     var TOut = Vector3.Normalize(knots[i+1].Position - knots[i-1].Position) * Vector3.Distance(knots[i].Position,knots[i-1].Position) / 3;//normalize(P[i+1] ​− P[i−1]​) * length(P - P[i+1]) / 3;
                        //     spline.Add(new BezierKnot(knots[i].Position, TIn, TOut));
                        // }
                        // spline.Add(knots[^1]);
                        spline = m_splineContainer.AddSpline();
                        // spline.TangentMode(Continuous);
                        knots = new List<BezierKnot>();
                    }
                    knots.Add(new BezierKnot(new float3(x, y, z)));
                    // spline.Add(new BezierKnot(new float3(x, y, z)), TangentMode.AutoSmooth);
                }
                else if (firstEmpty)
                {
                    firstEmpty = false;
                    spline.Knots = knots;
                    // SplineRange all = new SplineRange(0, spline.Count); // is not working to smooth, revisit
                    // spline.SetTangentMode(all, TangentMode.AutoSmooth);
                    // spline.SetAutoSmoothTension(all, m_SplineTension);

                    // spline.Add(knots[0]);

                    // spline.Add(knots[^1]);
                }
            }  

            rowNum ++;

        }
        // m_splineContainer.GetComponent<MeshRenderer>();//.RecalculateNormals();
        // m_splineContainer.GetComponent<MeshRenderer>().RecalculateBounds();
        m_splineContainer.GetComponent<SplineExtrude>().Rebuild();
        
    }
}
