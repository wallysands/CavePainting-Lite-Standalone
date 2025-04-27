using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Mathematics;

public class StrokeData : MonoBehaviour
{

//"AsH3","CH4","GaMe3","H2","Pres","Temp","V:0","V:1","V:2","IntegrationTime","Vorticity:0","Vorticity:1","Vorticity:2","Rotation","AngularVelocity","Normals:0","Normals:1","Normals:2","Points:0","Points:1","Points:2"
    public Dictionary<string, List<float>> m_strokeData = new Dictionary<string, List<float>>();

    public void Init(Dictionary<string, List<float>> strokeData, int startKnotIndex, int endKnotIndex)
    {
        foreach(string key in strokeData.Keys)
        {
            m_strokeData.Add(key, strokeData[key].GetRange(math.min(startKnotIndex,endKnotIndex), math.abs(endKnotIndex - startKnotIndex)));
        }

    }
    public List<string> getFeatureNames()
    {
        return m_strokeData.Keys.ToList();
    }
}
