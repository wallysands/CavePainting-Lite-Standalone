using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

public class SplineFieldMaker : MonoBehaviour
{
    [SerializeField] private Transform m_ArtworkParentTransform;
    [SerializeField] public TextAsset textAssetData;
    public SplineContainer m_splineContainer;
    public SpatialGrid m_spatialGrid;
    public List<Dictionary<string, List<float>>> m_splineFeaturesList = new List<Dictionary<string, List<float>>>();
    private string[] featureHeaders;
    public Dictionary<string,float> m_maxValues = new Dictionary<string, float>();
    public Dictionary<string,float> m_minValues = new Dictionary<string, float>();

    // Start is called before the first frame update
    void Start()
    {
        ReadCSV(textAssetData);
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
        int splineIndex = 0;
        int integrationTimeIndex = 0;
        String timeString = "IntegrationTime";

        foreach (string row in data)
        {
            // Ignore assumed header, should instead test the string itself instead of assuming header
            string[] rowValues = row.Split(new string[] { "," }, StringSplitOptions.None);
            if (rowNum != 0)
            {
                // Test that values exist in the row
                if (rowValues[0].Length > 0)
                {
                    // Build Splines
                    float integrationTime = float.Parse(rowValues[integrationTimeIndex]);
                    float x = float.Parse(rowValues[^3]);
                    float y = float.Parse(rowValues[^2]);
                    float z = float.Parse(rowValues[^1]);

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
                        splineIndex ++;
                        m_splineFeaturesList.Add(new Dictionary<string, List<float>>());
                    }
                    knots.Add(new BezierKnot(new float3(x, y, z)));
                    // spline.Add(new BezierKnot(new float3(x, y, z)), TangentMode.AutoSmooth);

                    // Data Processing
                    for (int i = 0; i < rowValues.Length; i++)
                    {
                        if (m_splineFeaturesList[splineIndex].ContainsKey(featureHeaders[i]))
                        {
                            m_splineFeaturesList[splineIndex][featureHeaders[i]].Add(float.Parse(rowValues[i]));
                        }
                        else
                        {
                            List<float> tmp = new List<float>();
                            tmp.Add(float.Parse(rowValues[i]));
                            m_splineFeaturesList[splineIndex].Add(featureHeaders[i], tmp);
                        }
                        

                        // Check if values are larger or smaller than max and min values respectively for the feature
                        if (float.Parse(rowValues[i]) > m_maxValues[featureHeaders[i]])
                        {
                            m_maxValues[featureHeaders[i]] = float.Parse(rowValues[i]);
                        }
                        else if (float.Parse(rowValues[i]) < m_minValues[featureHeaders[i]])
                        {
                            m_minValues[featureHeaders[i]] = float.Parse(rowValues[i]);
                        }
                    }

                    // Manual added features
                    float speed = (new Vector3(x, y, z)).sqrMagnitude;
                    if (m_splineFeaturesList[splineIndex].ContainsKey("Speed"))
                    {
                        m_splineFeaturesList[splineIndex]["Speed"].Add(speed);
                    }
                    else
                    {
                        List<float> tmp = new List<float>();
                        tmp.Add(speed);
                        m_splineFeaturesList[splineIndex].Add("Speed", tmp);
                    }
                    // Check if values are larger or smaller than max and min values respectively for the feature
                    if ((speed) > m_maxValues["Speed"])
                    {
                        m_maxValues["Speed"] = speed;
                    }
                    else if (speed < m_minValues["Speed"])
                    {
                        m_minValues["Speed"] = speed;
                    }

                }
                else if (firstEmpty)
                {
                    firstEmpty = false;
                    spline.Knots = knots;
                    SplineRange all = new SplineRange(0, spline.Count); 
                    spline.SetTangentMode(all, TangentMode.AutoSmooth);
                    spline.SetAutoSmoothTension(all, m_SplineTension);
                }
            }
            else
            {
                featureHeaders = rowValues;
                for (int i=0; i < featureHeaders.Length; i++)
                {
                    featureHeaders[i] = featureHeaders[i].Trim("\"".ToCharArray());
                    m_maxValues.Add(featureHeaders[i], float.MinValue);
                    m_minValues.Add(featureHeaders[i], float.MaxValue);
                    if (String.Equals(featureHeaders[i],"IntegrationTime"))
                    {
                        integrationTimeIndex = i;
                    }
                }

                // Manually Added Variables
                m_maxValues.Add("Speed", float.MinValue);
                m_minValues.Add("Speed", float.MaxValue);

                m_splineFeaturesList.Add(new Dictionary<string, List<float>>());
            }

            rowNum ++;

        }
        // m_splineContainer.GetComponent<MeshRenderer>();//.RecalculateNormals();
        // m_splineContainer.GetComponent<MeshRenderer>().RecalculateBounds();
        m_splineContainer.GetComponent<SplineExtrude>().Rebuild();        
    }
}
