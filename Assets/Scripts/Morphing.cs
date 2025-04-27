using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

public class Morphing : MonoBehaviour
{
    public void Init(TubeGeometry startingTube, Spline endingSpline, Transform parentTransform, Color c, Vector3 brushScale, float animationSpeed = 3)
    {
        m_startingTube = startingTube;
        m_startingVertices = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices;

        // m_endingSpline = endingSpline;
        m_brushColor = c;
        m_ArtworkParentTransform = parentTransform;
        m_brushScale = brushScale;

        // convert end spline into a tube
        m_endingTube = CreateTube(endingSpline, m_startingTube);
        m_endingVertices = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices;

        // If the tube would flip when morphing, reorganize vertices so it doesn't
        if (Vector3.Distance(m_startingVertices[0], m_endingVertices[0]) >= Vector3.Distance(m_startingVertices[^1], m_endingVertices[0])) {
            int faces = m_startingTube.GetNumFaces() + 1;
            m_endingVertices = m_endingVertices.Reverse().ToArray();
            Vector3[] temp = new Vector3[m_endingVertices.Length];
            Debug.Log("FLIPPING VERTS " + m_endingVertices.Length + " " + (int)(1.9));
            for (int i = 0; i < m_endingVertices.Length; i++) {
                //=FLOOR(A1/($C$3+1)) * ($C$3+1)  + MOD($C$3 - A1,($C$3 + 1))
                temp[i] = m_endingVertices[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            m_endingVertices = temp;
            m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices = m_endingVertices;
        }

        m_MorphingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        m_MorphComplete = false;

        // m_Mesh.SetVertices(m_Vertices);
        // m_Mesh.SetNormals(m_Normals);
        // m_Mesh.SetColors(m_Colors);
        // m_Mesh.SetUVs(0, m_TexCoords);
        // m_Mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);  
    }

    private TubeGeometry CreateTube(Spline spline, TubeGeometry matchTube)
    {
        GameObject m_CurrentStrokeObj = new GameObject("Morph " + m_startingTube.name, typeof(TubeGeometry));
        m_CurrentStrokeObj.transform.SetParent(m_ArtworkParentTransform.transform, false);

        TubeGeometry tube = m_CurrentStrokeObj.GetComponent<TubeGeometry>();
        tube.SetMaterial(matchTube.GetMaterial());
        tube.SetNumFaces(matchTube.GetNumFaces());
        tube.SetWrapTwice(matchTube.GetWrapTwice());

        Quaternion d = Quaternion.LookRotation(m_CurrentStrokeObj.transform.TransformDirection(spline.EvaluateTangent(0)));
        
        tube.Init(m_CurrentStrokeObj.transform.TransformPoint(spline.EvaluatePosition(0)), d, 0f, 0f, m_brushColor);

        // segmentsAlongSpline = matchTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices.Length / matchTube.GetNumFaces();
        segmentsAlongSpline = matchTube.GetVertices().Count / ( matchTube.GetNumFaces() + 1) ;
        // Debug.Log("NUM SEGMENTS " + segmentsAlongSpline);
        // Debug.Log("NUM VERTICES START " + matchTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices.Length);
        // Debug.Log("Num Faces Start " + matchTube.GetNumFaces());

        for (int i = 0; i < segmentsAlongSpline; i++)
        {
            float t = i / (float)(segmentsAlongSpline - 1); 
            Vector3 look = m_CurrentStrokeObj.transform.TransformDirection(spline.EvaluateTangent(t)).normalized; // z
            Vector3 up = new Vector3(0,1,0); // y = initial guess
            if (Mathf.Abs(Vector3.Dot(look, up)) > 0.8f) {
                // up vector is closely aligned with look, so we had a bad initial guess.
                up = new Vector3(1,0,0); // update the guess
            }
            Vector3 perp = Vector3.Cross(up, look); // x
            up = Vector3.Cross(look, perp);

            if (Vector3.Dot(up, m_LastUp) < 0) {
                up = -up;
            }
            m_LastUp = up;

            d = Quaternion.LookRotation(look, up);

            tube.AddSample(m_CurrentStrokeObj.transform.TransformPoint(spline.EvaluatePosition(t)), d, m_brushScale.x, m_brushScale.y, m_brushColor);
        }

        tube.Complete(m_CurrentStrokeObj.transform.TransformPoint(spline.EvaluatePosition(1)), d, 0f, 0f, m_brushColor);
        tube.GetComponent<MeshRenderer>().enabled = false;

        return tube;
    }

    void Update()
    {

        if ((m_startingTube != null) && (!m_MorphComplete)) {
            // lerp between vertex points on the start tube and end tube
            alpha += Time.deltaTime / m_animationSpeed;

            // if morph is complete, lock in the ending vertices and return
            if (alpha >= 1) // || Time.deltaTime / m_animationSpeed > 0.1)
            {
                m_MorphingMesh.SetVertices(m_endingVertices);
                MeshCollider mc = m_startingTube.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = m_MorphingMesh;
                m_MorphComplete = true;
            } else {

                Vector3[] movingVerts = m_MorphingMesh.vertices;

                for (int i = 0; i < movingVerts.Length; i++) {
                    movingVerts[i] = Vector3.Lerp(m_startingVertices[i], m_endingVertices[i], alpha); // this works only if the start and end tubes have same number of verts and faces                 
                }

                // Debug.Log("Num starting Verts " + m_startingVertices.Length + " Num ending verts " +endingVerts.Length);
                // int truncatedEnd =  endingVerts.Length - endingVerts.Length % m_startingTube.GetNumFaces();
                // float ratio = (float)(truncatedEnd) / (float)(movingVerts.Length);
                // Debug.Log("RATIO " + ratio);
                // Debug.Log("DOES RATIO WORK? " +(int)(ratio * movingVerts.Length));

                // for (int i = 0; i < movingVerts.Length-m_startingTube.GetNumFaces()+1; i+= m_startingTube.GetNumFaces())
                // {
                //     int endingVertIndex = (int)(ratio * i) - (int)(ratio * i) % m_startingTube.GetNumFaces();
                //     for (int j = 0; j < m_startingTube.GetNumFaces(); j++)
                //     {
                //         if (endingVertIndex + j >= endingVerts.Length)
                //         {
                //             Debug.Log("ENDINDEX " + endingVertIndex);
                //             Debug.Log("J " + j);
                //         }
                //         movingVerts[i + j] = Vector3.Lerp(m_startingVertices[i + j], endingVerts[endingVertIndex + j], alpha); // this works only if the start and end tubes have same number of verts and faces
                //     }
                // }

                // startingVerts[0] = Vector3.Lerp(startingVerts[0], endingVerts[0], Time.deltaTime/m_animationSpeed);
                // Debug.Log("MOVED TO " + movingVerts[0]+ " AT TIME " + alpha);
                // m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.Clear();
                m_MorphingMesh.SetVertices(movingVerts);
                // mesh.SetNormals

                // m_Mesh.SetNormals(m_Normals);
                // m_Mesh.SetColors(m_Colors);
                // m_Mesh.SetUVs(0, m_TexCoords);
                // m_Mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);  
            }
        }
    }

    private TubeGeometry m_startingTube;
    // private Spline m_endingSpline;
    private float m_animationSpeed = 3.0f;
    private Color m_brushColor;
    private Transform m_ArtworkParentTransform;
    private TubeGeometry m_endingTube;
    private int segmentsAlongSpline = 100;
    private Vector3[] m_startingVertices;
    private Vector3[] m_endingVertices;
    private Mesh m_MorphingMesh;

    private float alpha = 0;
    private GameObject m_CurrentStrokeObj;
    private bool m_MorphComplete;

    private Vector3 m_LastUp = new Vector3(0,1,0);
    private Vector3 m_brushScale;
}