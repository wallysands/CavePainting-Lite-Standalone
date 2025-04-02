using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Morphing : MonoBehaviour
{
    public void Init(TubeGeometry startingTube, Spline endingSpline, Transform parentTransform, Color c, float animationSpeed = 1)
    {
        m_startingTube = startingTube;
        // m_endingSpline = endingSpline;
        m_brushColor = c;
        m_ArtworkParentTransform = parentTransform;

        // convert end spline into a tube
        m_endingTube = CreateTube(endingSpline, m_startingTube);

    }

    private TubeGeometry CreateTube(Spline spline, TubeGeometry matchTube)
    {
        GameObject m_CurrentStrokeObj = new GameObject("Morph " + m_startingTube.name, typeof(TubeGeometry));
        m_CurrentStrokeObj.transform.SetParent(m_ArtworkParentTransform.transform, false);

        TubeGeometry tube = m_CurrentStrokeObj.GetComponent<TubeGeometry>();
        tube.SetMaterial(matchTube.GetMaterial());
        tube.SetNumFaces(matchTube.GetNumFaces());
        tube.SetWrapTwice(matchTube.GetWrapTwice());

        Quaternion d = Quaternion.LookRotation(spline.EvaluateTangent(0));
        
        tube.Init(spline.EvaluatePosition(0), d, 0.5f, 0.5f, m_brushColor);

        for (int i = 0; i < segmentsAlongSpline; i++)
        {
            float t = i / (float)(segmentsAlongSpline - 1); 
            d = Quaternion.LookRotation(spline.EvaluateTangent(t));
            tube.AddSample(spline.EvaluatePosition(t), d, 0.1f, 0.1f, m_brushColor);
        }

        tube.Complete(spline.EvaluatePosition(1), d, 0.5f, 0.5f, m_brushColor);

        return tube;
    }

    void Update()
    {

    }

    private TubeGeometry m_startingTube;
    // private Spline m_endingSpline;
    private float m_animationSpeed;
    private Color m_brushColor;
    private Transform m_ArtworkParentTransform;
    private TubeGeometry m_endingTube;
    private int segmentsAlongSpline = 100;
}