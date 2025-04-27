using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

public class SpatialGrid : MonoBehaviour
{

    public void Init()
    {
        // m_ContainerBounds = SplineUtility.GetBounds(m_SplineContainer);
        for (int i = 0; i < m_SplineContainer.Splines.Count; i++)
        {
            foreach (BezierKnot knot in m_SplineContainer.Splines[i].Knots)
            {
                Vector3 roundedPosition = GetRoundedVector(knot.Position); // no round 6167 // round 245
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

    public List<int> GetNearbySplines(Vector3 pos)
    {
        Vector3 roundedPosition = GetRoundedVector(pos);
        List<int> splineIndices = new List<int>();
        for (float i = -1 * m_cubeSize; i <= 1 * m_cubeSize; i += m_cubeSize)
        {
            for (float j = -1 * m_cubeSize; j <= 1 * m_cubeSize; j += m_cubeSize)
            {
                for (float k = -1 * m_cubeSize; k <= 1 * m_cubeSize; k += m_cubeSize)
                {
                    Vector3 checkPos = roundedPosition + Vector3.right * i + Vector3.up * j + Vector3.forward * k;
                    // Debug.Log("CHECKING POS " + checkPos);
                    if (m_Dict.ContainsKey(checkPos))
                    {
                        foreach (int splineIndex in m_Dict[checkPos])
                        {
                            splineIndices.Add(splineIndex);
                        }
                    }
                }
            }
        }
        return splineIndices.Distinct().ToList();
    }

    public List<int> GetNearbySplines(List<Vector3> pos)
    {
        List<int> splineIndices = new List<int>();
        foreach (Vector3 v in pos)
        {
            Vector3 roundedPosition = GetRoundedVector(v);
            for (float i = -1 * m_cubeSize; i <= 1 * m_cubeSize; i += m_cubeSize)
            {
                for (float j = -1 * m_cubeSize; j <= 1 * m_cubeSize; j += m_cubeSize)
                {
                    for (float k = -1 * m_cubeSize; k <= 1 * m_cubeSize; k += m_cubeSize)
                    {
                        Vector3 checkPos = roundedPosition + Vector3.right * i + Vector3.up * j + Vector3.forward * k;
                        // Debug.Log("CHECKING POS " + checkPos);
                        if (m_Dict.ContainsKey(checkPos))
                        {
                            foreach (int splineIndex in m_Dict[checkPos])
                            {
                                splineIndices.Add(splineIndex);
                            }
                        }
                    }
                }
            }
        }
        return splineIndices.Distinct().ToList();
    }

    private Vector3 GetRoundedVector(Vector3 currPos)
    {
        return math.round(currPos/m_cubeSize);
    }

    public SplineContainer m_SplineContainer;
    public float m_cubeSize;
    private Dictionary<Vector3, List<int>> m_Dict = new Dictionary<Vector3, List<int>>();
}
