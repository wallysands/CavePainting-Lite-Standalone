using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;

public class Morphing : MonoBehaviour
{
    public void Init(TubeGeometry startingTube, Spline endingSpline, Transform parentTransform, Color c, Vector3 brushScale, bool settle, float animationSpeed = 3)
    {
        m_Settle = settle;
        m_startingTube = startingTube;
        m_startingVertices = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices;
        m_startingNormals = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.normals;
        m_startingColors = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.colors;

        // m_endingSpline = endingSpline;
        m_brushColor = c;
        m_ArtworkParentTransform = parentTransform;
        m_brushScale = brushScale;

        // convert end spline into a tube
        m_endingTube = CreateTube(endingSpline, m_startingTube);
        m_endingVertices = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.vertices;
        m_endingNormals = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.normals;
        m_endingColors = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.colors;
        m_originalVertices = m_endingVertices;
        m_originalBrushColor = m_endingColors;


        // If the tube would flip when morphing, reorganize vertices so it doesn't
        if (Vector3.Distance(m_startingVertices[0], m_endingVertices[0]) >= Vector3.Distance(m_startingVertices[^1], m_endingVertices[0])) {
            int faces = m_startingTube.GetNumFaces() + 1;

            m_startingVertices = m_startingVertices.Reverse().ToArray();
            m_startingNormals = m_startingNormals.Reverse().ToArray();
            m_startingColors = m_startingColors.Reverse().ToArray();


            Vector3[] tempVerts = new Vector3[m_startingVertices.Length];
            Vector3[] tempNorms = new Vector3[m_startingNormals.Length];
            Color[] tempCols = new Color[m_startingColors.Length];

            Debug.Log("FLIPPING VERTS " + m_endingVertices.Length + " " + (int)(1.9));


            for (int i = 0; i < m_startingVertices.Length; i++) {
                //=FLOOR(A1/($C$3+1)) * ($C$3+1)  + MOD($C$3 - A1,($C$3 + 1))
                tempVerts[i] = m_startingVertices[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingNormals.Length; i++) {
                tempNorms[i] = m_startingNormals[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingColors.Length; i++) {
                tempCols[i] = m_startingColors[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            m_startingVertices = tempVerts;
            m_startingNormals = tempNorms;
            m_startingColors = tempCols;
            Mesh startingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;

            startingMesh.vertices = m_startingVertices;
            startingMesh.normals = m_startingNormals;
            startingMesh.colors = m_startingColors;

            m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh = startingMesh;
            
            // startingMesh.vertices = m_endingVertices;
            // startingMesh.normals = m_endingNormals;
            // startingMesh.colors = m_endingColors;
        }

        m_MorphingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        m_MorphComplete = false;

        // Calculate Original Width
        m_OriginalWidth = Vector3.Distance(m_endingVertices[0], m_endingVertices[1]);

        // m_Mesh.SetVertices(m_Vertices);
        // m_Mesh.SetNormals(m_Normals);
        // m_Mesh.SetColors(m_Colors);
        // m_Mesh.SetUVs(0, m_TexCoords);
        // m_Mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);  

        // For lazy data binding mode
        m_stationaryVertices = m_startingVertices;
        m_stationaryNormals = m_startingNormals;
        m_stationaryColors = m_startingColors;
        m_originalStationaryVertices = m_stationaryVertices;
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

    public void NewMorph()
    {
        Destroy(m_startingTube.gameObject.GetComponent<MeshCollider>());
        // Mesh startingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        // startingMesh.vertices = m_MorphingMesh.vertices;
        // startingMesh.normals = m_MorphingMesh.normals;
        // startingMesh.colors = m_MorphingMesh.colors;
        // m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh = startingMesh;
        // If the tube would flip when morphing, reorganize vertices so it doesn't. Bandaid fix, because I can't find why stationary tubes are flipping when a width mapping is applied
        if (!m_Settle && Vector3.Distance(m_startingVertices[0], m_stationaryVertices[0]) >= Vector3.Distance(m_startingVertices[^1], m_stationaryVertices[0])) {
            int faces = m_startingTube.GetNumFaces() + 1;

            m_startingVertices = m_startingVertices.Reverse().ToArray();
            m_startingNormals = m_startingNormals.Reverse().ToArray();
            m_startingColors = m_startingColors.Reverse().ToArray();


            Vector3[] tempVerts = new Vector3[m_startingVertices.Length];
            Vector3[] tempNorms = new Vector3[m_startingNormals.Length];
            Color[] tempCols = new Color[m_startingColors.Length];

            Debug.Log("FLIPPING VERTS " + m_endingVertices.Length + " " + (int)(1.9));


            for (int i = 0; i < m_startingVertices.Length; i++) {
                //=FLOOR(A1/($C$3+1)) * ($C$3+1)  + MOD($C$3 - A1,($C$3 + 1))
                tempVerts[i] = m_startingVertices[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingNormals.Length; i++) {
                tempNorms[i] = m_startingNormals[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingColors.Length; i++) {
                tempCols[i] = m_startingColors[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            m_startingVertices = tempVerts;
            m_startingNormals = tempNorms;
            m_startingColors = tempCols;
            Mesh startingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;

            startingMesh.vertices = m_startingVertices;
            startingMesh.normals = m_startingNormals;
            startingMesh.colors = m_startingColors;

            m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh = startingMesh;
            
            // startingMesh.vertices = m_endingVertices;
            // startingMesh.normals = m_endingNormals;
            // startingMesh.colors = m_endingColors;
        }
        else if (m_Settle && Vector3.Distance(m_startingVertices[0], m_endingVertices[0]) >= Vector3.Distance(m_startingVertices[^1], m_endingVertices[0])) {
            int faces = m_startingTube.GetNumFaces() + 1;

            m_startingVertices = m_startingVertices.Reverse().ToArray();
            m_startingNormals = m_startingNormals.Reverse().ToArray();
            m_startingColors = m_startingColors.Reverse().ToArray();


            Vector3[] tempVerts = new Vector3[m_startingVertices.Length];
            Vector3[] tempNorms = new Vector3[m_startingNormals.Length];
            Color[] tempCols = new Color[m_startingColors.Length];

            Debug.Log("FLIPPING VERTS " + m_endingVertices.Length + " " + (int)(1.9));


            for (int i = 0; i < m_startingVertices.Length; i++) {
                //=FLOOR(A1/($C$3+1)) * ($C$3+1)  + MOD($C$3 - A1,($C$3 + 1))
                tempVerts[i] = m_startingVertices[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingNormals.Length; i++) {
                tempNorms[i] = m_startingNormals[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            for (int i = 0; i < m_startingColors.Length; i++) {
                tempCols[i] = m_startingColors[(int)(i / (faces)) * faces + ((faces - 1 - i) % faces)];
            }
            m_startingVertices = tempVerts;
            m_startingNormals = tempNorms;
            m_startingColors = tempCols;
            Mesh startingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;

            startingMesh.vertices = m_startingVertices;
            startingMesh.normals = m_startingNormals;
            startingMesh.colors = m_startingColors;

            m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh = startingMesh;
            
            // startingMesh.vertices = m_endingVertices;
            // startingMesh.normals = m_endingNormals;
            // startingMesh.colors = m_endingColors;
        }

        m_MorphingMesh = m_startingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh;
        alpha = 0;
        m_MorphComplete = false;
    }

    public void SetEndingColors(Color[] colorsPerSample)
    {
        m_endingColors = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.colors;
        m_stationaryColors = m_endingTube.GetComponent<MeshRenderer>().GetComponent<MeshFilter>().mesh.colors;
    }

    public void ResetColors()
    {
        m_endingColors = m_originalBrushColor;
        m_stationaryColors = m_originalBrushColor;
    }

    public void ResetWidths()
    {
        m_endingVertices = m_originalVertices;
        m_stationaryVertices = m_originalStationaryVertices;
    }

    void Update()
    {

        if ((m_startingTube != null) && (!m_MorphComplete)) {
            // lerp between vertex points on the start tube and end tube
            alpha += Time.deltaTime / m_animationSpeed;
            // Debug.Log("TIME PASSED: " + Time.deltaTime);

            // if morph is complete, lock in the ending vertices and return
            if (alpha >= 1) // || Time.deltaTime / m_animationSpeed > 0.1)
            {
                if (m_Settle)
                {
                    m_MorphingMesh.SetVertices(m_endingVertices);
                    m_MorphingMesh.SetNormals(m_endingNormals);
                    m_MorphingMesh.SetColors(m_endingColors);
                    MeshCollider mc = m_startingTube.gameObject.AddComponent<MeshCollider>();
                    m_MorphComplete = true;
                    m_startingVertices = m_endingVertices;
                    m_startingNormals = m_endingNormals;
                    m_startingColors = m_endingColors;
                }
                else
                {
                    m_MorphingMesh.SetVertices(m_stationaryVertices);
                    m_MorphingMesh.SetNormals(m_stationaryNormals);
                    m_MorphingMesh.SetColors(m_stationaryColors);
                    MeshCollider mc = m_startingTube.gameObject.AddComponent<MeshCollider>();
                    m_MorphComplete = true;
                    m_startingVertices = m_stationaryVertices;
                    m_startingNormals = m_stationaryNormals;
                    m_startingColors = m_stationaryColors;
                }
            } else {
                
                Vector3[] movingVerts = m_MorphingMesh.vertices;
                Vector3[] movingNorms = m_MorphingMesh.normals;
                Color[] movingCols = m_MorphingMesh.colors;
                if (m_Settle)
                {
                    for (int i = 0; i < movingVerts.Length; i++) {
                        // this works only if the start and end tubes have same number of verts and faces                 
                        movingVerts[i] = Vector3.Lerp(m_startingVertices[i], m_endingVertices[i], alpha);
                        movingNorms[i] = Vector3.Lerp(m_startingNormals[i], m_endingNormals[i], alpha);
                        movingCols[i] = Color.Lerp(m_startingColors[i], m_endingColors[i], alpha);
                    }
                }
                else
                {
                    for (int i = 0; i < movingVerts.Length; i++) {
                        // this works only if the start and end tubes have same number of verts and faces                 
                        movingVerts[i] = Vector3.Lerp(m_startingVertices[i], m_stationaryVertices[i], alpha);
                        movingNorms[i] = Vector3.Lerp(m_startingNormals[i], m_stationaryNormals[i], alpha);
                        movingCols[i] = Color.Lerp(m_startingColors[i], m_stationaryColors[i], alpha);
                    }
                }

                m_MorphingMesh.SetVertices(movingVerts);
                m_MorphingMesh.SetNormals(movingNorms);
                m_MorphingMesh.SetColors(movingCols);

            }
        }
    }

    public float GetOriginalWidth()
    {
        return m_OriginalWidth;
    }

    public Color GetOriginalColor()
    {
        // Only one brush color can be used when drawing so it should all be the same color
        return m_originalBrushColor[0];
    }

    // private Spline m_endingSpline;
    [SerializeField] private float m_animationSpeed = 3.0f;
    [SerializeField] private int segmentsAlongSpline = 50;

    private Transform m_ArtworkParentTransform;
    private GameObject m_CurrentStrokeObj;
    private Mesh m_MorphingMesh;

    private TubeGeometry m_startingTube;
    private Vector3[] m_startingVertices;
    private Vector3[] m_startingNormals;
    private Color[] m_startingColors;

    private TubeGeometry m_endingTube;
    public Vector3[] m_endingVertices;
    private Vector3[] m_endingNormals;
    private Color[] m_endingColors;
    public Vector3[] m_stationaryVertices;
    private Vector3[] m_stationaryNormals;
    private Color[] m_stationaryColors;

    private float alpha = 0;
    private bool m_MorphComplete;
    private Color m_brushColor;
    private Vector3 m_LastUp = new Vector3(0,1,0);
    private Vector3 m_brushScale;

    // Original Values
    private Color[] m_originalBrushColor;
    private Vector3[] m_originalVertices;
    private Vector3[] m_originalStationaryVertices;
    private float m_OriginalWidth;
    public bool m_Settle;
}