using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SpatialGrid : MonoBehaviour
{

    public void Init()
    {
        // m_ContainerBounds = SplineUtility.GetBounds(m_SplineContainer);
        for (int i = 0; i < m_SplineContainer.Splines.Count; i++)
        {
            foreach (BezierKnot knot in m_SplineContainer.Splines[i].Knots)
            {
                Vector3 roundedPosition = math.round(knot.Position/m_cubeSize); // no round 6167 // round 245
                if (m_Dict.ContainsKey(roundedPosition))
                {
                    m_Dict[roundedPosition].Add(i);
                }
                else
                {
                    m_Dict.Add(roundedPosition, new List<int>(i));
                }
            }
        }
        Debug.Log("NUMBER OF KEYS: " + m_Dict.Keys.Count);
    }

    public Dictionary<Vector3, List<int>> GetDict()
    {
        return m_Dict;
    }

    public SplineContainer m_SplineContainer;
    public float m_cubeSize;
    private Dictionary<Vector3, List<int>> m_Dict = new Dictionary<Vector3, List<int>>();
}
