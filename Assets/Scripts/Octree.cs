using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Octree : MonoBehaviour
{
    public void Clear()
    {
        
    }
    void Start()
    {
        // m_ContainerBounds = SplineUtility.GetBounds(m_SplineContainer);
        foreach (Spline spline in m_SplineContainer.Splines)
        {
            m_SplineBounds.Add(SplineUtility.GetBounds(spline));
            // sort into different cubes.
        }
    }
    public SplineContainer m_SplineContainer;
    private Bounds m_ContainerBounds;
    private List<Bounds> m_SplineBounds;
}
