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
                        SplineRange all = new SplineRange(0, spline.Count); 
                        spline.SetTangentMode(all, TangentMode.AutoSmooth);
                        spline.SetAutoSmoothTension(all, m_SplineTension);
                        spline = m_splineContainer.AddSpline();
                        // spline.TangentMode(Continuous);
                        knots = new List<BezierKnot>();
                    }
                    // knots.Add(new BezierKnot(new float3(x, y, z)));
                    spline.Add(new BezierKnot(new float3(x, y, z)), TangentMode.AutoSmooth);
                }
                else if (firstEmpty)
                {
                    firstEmpty = false;
                    spline.Knots = knots;
                    SplineRange all = new SplineRange(0, spline.Count); 
                    spline.SetTangentMode(all, TangentMode.AutoSmooth);
                    spline.SetAutoSmoothTension(all, m_SplineTension);
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
