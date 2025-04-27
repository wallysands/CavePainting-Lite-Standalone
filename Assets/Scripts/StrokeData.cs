using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

public class StrokeData : MonoBehaviour
{

//"AsH3","CH4","GaMe3","H2","Pres","Temp","V:0","V:1","V:2","IntegrationTime","Vorticity:0","Vorticity:1","Vorticity:2","Rotation","AngularVelocity","Normals:0","Normals:1","Normals:2","Points:0","Points:1","Points:2"
    public Dictionary<string, List<float>> m_strokeData = new Dictionary<string, List<float>>();
    public List<float> m_speed = new List<float>();
    private GameObject m_currentStrokeObj;
    private float m_speedScalar = 0.05f;
    private TubeGeometry m_morphStroke;
    private Dictionary<string,float> m_maxValues;
    private Dictionary<string,float> m_minValues;
    private Mesh m_strokeMesh;
    private Morphing m_morph;


    public void Init(Dictionary<string, List<float>> strokeData, GameObject strokeObj, int startKnotIndex, int endKnotIndex, Dictionary<string,float> maxValues, Dictionary<string,float> minValues, Morphing morph)
    {
        m_currentStrokeObj = strokeObj;
        m_morphStroke = GameObject.Find("Morph " + strokeObj.name).GetComponent<TubeGeometry>();
        m_strokeMesh = m_morphStroke.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        m_maxValues = maxValues;
        m_minValues = minValues;
        m_morph = morph;

        foreach(string key in strokeData.Keys)
        {
            m_strokeData.Add(key, strokeData[key].GetRange(math.min(startKnotIndex,endKnotIndex), math.abs(endKnotIndex - startKnotIndex)));
            // Normalize these values
            for (int i = 0; i < m_strokeData[key].Count(); i++) m_strokeData[key][i] = 2 * (m_strokeData[key][i] - m_minValues[key]) / (m_maxValues[key] - m_minValues[key]) - 1;
        }
        if (m_strokeData.ContainsKey("V:0"))
        {
            DetermineSpeed();
            Debug.Log(m_morphStroke);
            // foreach(float v in m_strokeData["Speed"]) Debug.Log(v);
            AdjustTubeWidth("Speed");
        }
    }

    public List<string> getFeatureNames()
    {
        return m_strokeData.Keys.ToList();
    }

    public void DetermineSpeed()
    {
        for (int i = 0; i < m_strokeData["V:0"].Count(); i++)
        {
            float speed = (new Vector3(m_strokeData["V:0"][i], m_strokeData["V:1"][i], m_strokeData["V:2"][i])).sqrMagnitude;
            m_speed.Add(speed);
        }
        m_strokeData.Add("Speed", m_speed);
    }

    public void AdjustTubeWidth(string feat)
    {
        // m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;

        List<Vector3> strokeVertices = m_morph.m_endingVertices.ToList();
        Debug.Log(strokeVertices[0]);
        List<float> featToScaleOn = m_strokeData[feat];
        int numFaces = m_morphStroke.GetNumFaces();
        for (int i = 0; i < strokeVertices.Count(); i+=(numFaces+1))
        {
            // how far along the stroke are we
            float t = i / (float)(strokeVertices.Count() - numFaces);
            // index from the data for that % along the stroke
            float dataIndexFloat = (t * featToScaleOn.Count());
            int dataIndex = (int)dataIndexFloat;
            float dataLerpAmount = dataIndexFloat - dataIndex;
            float dataScaleValue = featToScaleOn[dataIndex];
            if (dataIndex < featToScaleOn.Count() - 1)
            {
                dataScaleValue = Mathf.Lerp(featToScaleOn[dataIndex], featToScaleOn[dataIndex + 1], dataLerpAmount);
            }
            dataScaleValue *= m_speedScalar;
            // calculate center of tube
            Vector3 center = new Vector3(0,0,0);
            foreach (Vector3 v in m_morphStroke.GetVertices().ToList().GetRange(i, numFaces)) center += (v / numFaces);

            // adjust vertices
            float a = 0;
            // float dataScaleValue = featToScaleOn[dataIndex];
            for (int j = 0; j < numFaces + 1; j++)
            {
                Vector3 thisVert = center + m_morphStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Cos(a) + m_morphStroke.GetNormals()[i+j] * dataScaleValue * Mathf.Sin(a);
                if (i == 0 && j == 0) Debug.Log(thisVert + " " + dataScaleValue + " " + featToScaleOn[dataIndex] +  " " + m_strokeData["V:0"][dataIndex] + " " + m_strokeData["V:1"][dataIndex] + " " + m_strokeData["V:2"][dataIndex]);
                strokeVertices[i + j] = thisVert;
            }
        }
        // Debug.Log(strokeVertices[0]);
        m_morph.m_endingVertices = strokeVertices.ToArray();
    }
}
