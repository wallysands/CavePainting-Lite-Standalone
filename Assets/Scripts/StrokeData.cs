using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

public class StrokeData : MonoBehaviour
{

//"AsH3","CH4","GaMe3","H2","Pres","Temp","V:0","V:1","V:2","IntegrationTime","Vorticity:0","Vorticity:1","Vorticity:2","Rotation","AngularVelocity","Normals:0","Normals:1","Normals:2","Points:0","Points:1","Points:2"
    public Dictionary<string, List<float>> m_strokeData = new Dictionary<string, List<float>>();
    private GameObject m_currentStrokeObj;
    private TubeGeometry m_morphStroke;
    private TubeGeometry m_stationaryStroke;
    private Dictionary<string,float> m_maxValues;
    private Dictionary<string,float> m_minValues;
    private Mesh m_strokeMesh;
    private Morphing m_morph;


    public void Init(Dictionary<string, List<float>> strokeData, GameObject strokeObj, int startKnotIndex, int endKnotIndex, Dictionary<string,float> maxValues, Dictionary<string,float> minValues, Morphing morph)
    {
        m_currentStrokeObj = strokeObj;
        m_morphStroke = GameObject.Find("Morph " + strokeObj.name).GetComponent<TubeGeometry>();
        m_stationaryStroke = strokeObj.GetComponent<TubeGeometry>();
        m_strokeMesh = m_morphStroke.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        m_maxValues = maxValues;
        m_minValues = minValues;
        m_morph = morph;

        foreach(string key in strokeData.Keys)
        {
            m_strokeData.Add(key, strokeData[key].GetRange(math.min(startKnotIndex,endKnotIndex), math.abs(endKnotIndex - startKnotIndex)));
            // Normalize these values
            for (int i = 0; i < m_strokeData[key].Count(); i++) m_strokeData[key][i] = (m_strokeData[key][i] - m_minValues[key]) / (m_maxValues[key] - m_minValues[key]);
        }
        if (m_strokeData.ContainsKey("V:0"))
        {
            // DetermineSpeed();
            // Debug.Log(m_morphStroke);
            // foreach(float v in m_strokeData["Speed"]) Debug.Log(v);
            // AdjustTubeWidth("Speed");
        }
    }

    public List<string> getFeatureNames()
    {
        return m_strokeData.Keys.ToList();
    }

    public float GetDataValueAlongSpline(string feat, float fracAlongSpline, bool inverseMappings)
    {
        // Determine if high values should make the tube smaller or larger (0 = high values => larger)
        int inverse = 0;
        if (inverseMappings) inverse = 1;
        
        Debug.Assert(m_strokeData.ContainsKey(feat));
        List<float> featureValsAtKnots = m_strokeData[feat];
        // Debug.Log("Along Spline: " + fracAlongSpline);

        // index from the data for that % along the stroke
        float dataIndexFloat = (fracAlongSpline * (featureValsAtKnots.Count()-1));
        int dataIndex = (int)dataIndexFloat;
        float dataLerpAmount = dataIndexFloat - dataIndex;
        // Debug.Log("Data Index: " + dataIndex);
        float dataValue = Mathf.Abs(inverse - featureValsAtKnots[dataIndex]);
        if (dataIndex < featureValsAtKnots.Count() - 1)
        {
            dataValue = Mathf.Lerp(dataValue, Mathf.Abs(inverse - featureValsAtKnots[dataIndex + 1]), dataLerpAmount);
        }
        return dataValue;
    }

    public void AdjustTubeWidth(string feat, float minValue, float maxValue, bool inverseMappings)
    {
        // Determine if high values should make the tube smaller or larger (0 = high values => larger)
        int inverse = 0;
        if (inverseMappings) inverse = 1;

        if (minValue == -1)
        {
            Debug.Log("MIN VALUE DEFAULT");
            minValue = float.MinValue;
        }
        if (maxValue == -1)
        {
            maxValue = float.MaxValue;
        }

        List<Vector3> strokeVertices = m_morph.m_endingVertices.ToList();
        List<Vector3> stationaryStrokeVertices = m_morph.m_stationaryVertices.ToList();
        List<float> featToScaleOn = m_strokeData[feat];
        int numFaces = m_morphStroke.GetNumFaces();
        for (int i = 0; i < strokeVertices.Count(); i+=(numFaces+1))
        {
            // how far along the stroke are we
            float t = i / (float)(strokeVertices.Count() - numFaces);
            // index from the data for that % along the stroke
            float dataIndexFloat = (t * (featToScaleOn.Count()-1));
            int dataIndex = (int)dataIndexFloat;
            float dataLerpAmount = dataIndexFloat - dataIndex;
            // float dataScaleValue = Mathf.Max(Mathf.Min(Mathf.Abs(inverse - featToScaleOn[dataIndex]), maxValue), minValue);
            float dataScaleValue = Mathf.Lerp(minValue, maxValue, Mathf.Abs(inverse - featToScaleOn[dataIndex]));
            if (dataIndex < featToScaleOn.Count() - 1)
            {
                // dataScaleValue = Mathf.Lerp(dataScaleValue, Mathf.Max(Mathf.Min(Mathf.Abs(inverse - featToScaleOn[dataIndex+1]), maxValue), minValue), dataLerpAmount);
                dataScaleValue = Mathf.Lerp(dataScaleValue, Mathf.Lerp(minValue, maxValue, Mathf.Abs(inverse - featToScaleOn[dataIndex+1])), dataLerpAmount);
            }
            // calculate center of tube
            Vector3 center = new Vector3(0,0,0);
            foreach (Vector3 v in m_morphStroke.GetVertices().ToList().GetRange(i, numFaces)) center += (v / numFaces);
            Vector3 stationaryCenter = new Vector3(0,0,0);
            foreach (Vector3 v in m_stationaryStroke.GetVertices().ToList().GetRange(i, numFaces)) stationaryCenter += (v / numFaces);

            // adjust vertices
            float a = 0;
            // float dataScaleValue = featToScaleOn[dataIndex];
            for (int j = 0; j < numFaces + 1; j++)
            {
                Vector3 thisVert = center + m_morphStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Cos(a) + m_morphStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Sin(a);
                Vector3 thisStationaryVert = stationaryCenter + m_stationaryStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Cos(a) + m_stationaryStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Sin(a);
                // if (i == 0 && j == 0) Debug.Log(thisVert + " " + dataScaleValue + " " + featToScaleOn[dataIndex] +  " " + m_strokeData["V:0"][dataIndex] + " " + m_strokeData["V:1"][dataIndex] + " " + m_strokeData["V:2"][dataIndex]);
                strokeVertices[i + j] = thisVert;
                stationaryStrokeVertices[i + j] = thisStationaryVert;
            }
        }
        // Debug.Log(strokeVertices[0]);
        m_morph.m_endingVertices = strokeVertices.ToArray();
        m_morph.m_stationaryVertices = stationaryStrokeVertices.ToArray();
    }

    // Returns original drawn width; average data value of input feature
    public (float, float) GetStrokeInfoWidth(string bindingFeature)
    {
        float width = m_morph.GetOriginalWidth();
        float averageValue = 0;
        foreach (float v in m_strokeData[bindingFeature]) averageValue += v / m_strokeData[bindingFeature].Count();
        return (width, averageValue);
    }

    // Returns original drawn color; average data value of input feature
    public (Color, float) GetStrokeInfoColor(string bindingFeature)
    {
        Color color = m_morph.GetOriginalColor();
        float averageValue = 0;
        foreach (float v in m_strokeData[bindingFeature]) averageValue += v / m_strokeData[bindingFeature].Count();
        return (color, averageValue);
    }

    public void TriggerMorph()
    {
        m_morph.NewMorph();
    }

    public void SetColors(Color[] colorsPerSample)
    {
        m_morphStroke.SetColorsPerSample(colorsPerSample);
        m_stationaryStroke.SetColorsPerSample(colorsPerSample);
        m_morph.SetEndingColors(colorsPerSample);
    }

    public void ResetColors()
    {
        m_morph.ResetColors();
    }

    public void ResetWidths()
    {
        m_morph.ResetWidths();
    }
}
