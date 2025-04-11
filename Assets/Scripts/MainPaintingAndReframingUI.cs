using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using System.Linq;

namespace IVLab.MinVR3
{

    public class MainPaintingAndReframingUI : MonoBehaviour//, IVREventListener
    {
        public Color brushColor {
            get { return m_BrushColor; }
            set { SetBrushColor(value); }
        }

        public void SetBrushColor(Color c)
        {
            m_BrushColor = c;
            m_BrushCursorMeshRenderer.sharedMaterial.color = c;
        }

        public void SetBrushColor(Vector4 c)
        {
            SetBrushColor(new Color(c[0], c[1], c[2], c[3]));
        }

        // void OnEnable()
        // {
        //     StartListening();
        // }

        // void OnDisable()
        // {
        //     StopListening();
        // }
        

        private void Reset()
        {
            m_ArtworkParentTransform = null;
            m_BrushCursorTransform = null;
            m_HandCursorTransform = null;
        }

        private void Start()
        {
            Debug.Assert(m_ArtworkParentTransform != null);
            Debug.Assert(m_BrushCursorTransform != null);
            Debug.Assert(m_BrushCursorMeshRenderer != null);
            Debug.Assert(m_HandCursorTransform != null);
            Debug.Assert(m_PaintMaterial != null);

            m_NumStrokes = 0;

            // Debug.Log("Entering");
            // m_NewArt = new List<Transform>();

            m_SplineColoredContainer.transform.SetParent(m_ArtworkParentTransform, false);
        }


        // PAINTING STATE CALLBACKS

        public void Painting_OnEnter()
        {
            // // create a new GameObject to hold the new paint stroke
            // m_CurrentStrokeObj = new GameObject("Stroke " + m_NumStrokes);
            // m_CurrentStrokeObj.transform.SetParent(m_ArtworkParentTransform, false);

            // // normals can get weird when using two-sided rendering, and Unity's standard shaders do not support it.
            // // but we would like to see both sides of the ribbons we paint.  so, the solution is to create two meshes
            // // one to draw the "front" side of the ribbon and one to draw the "back" side of the ribbon.  the only
            // // change between front and back is swapping the vertex ordering of each triangle.
            // GameObject frontMeshObj = new GameObject("FrontMesh", typeof(MeshFilter), typeof(MeshRenderer));
            // frontMeshObj.transform.SetParent(m_CurrentStrokeObj.transform, false);
            // MeshRenderer frontMeshRenderer = frontMeshObj.GetComponent<MeshRenderer>();
            // frontMeshRenderer.sharedMaterial = m_PaintMaterial;       // set shared base material
            // Material customizedMaterial = frontMeshRenderer.material; // clones base material
            // customizedMaterial.color = m_BrushColor;                  // customize the clone
            // frontMeshRenderer.sharedMaterial = customizedMaterial;    // set shared to customized

            // m_CurrentStrokeFrontMesh = frontMeshRenderer.GetComponent<MeshFilter>().mesh;
            // m_CurrentStrokeFrontMesh.MarkDynamic();
            // m_CurrentStrokeFrontVertices = new List<Vector3>();
            // m_CurrentStrokeFrontIndices = new List<int>();

            // GameObject backMeshObj = new GameObject("BackMesh", typeof(MeshFilter), typeof(MeshRenderer));
            // backMeshObj.transform.SetParent(m_CurrentStrokeObj.transform, false);
            // MeshRenderer backMeshRenderer = backMeshObj.GetComponent<MeshRenderer>();
            // backMeshRenderer.sharedMaterial = customizedMaterial;
            // m_CurrentStrokeBackMesh = backMeshRenderer.GetComponent<MeshFilter>().mesh;
            // m_CurrentStrokeBackMesh.MarkDynamic();
            // m_CurrentStrokeBackVertices = new List<Vector3>();
            // m_CurrentStrokeBackIndices = new List<int>();


            // Tube Geometry Start
            m_CurrentStrokeObj = new GameObject("Tube Stroke " + m_NumStrokes, typeof(TubeGeometry));
            m_CurrentStrokeObj.transform.SetParent(m_ArtworkParentTransform.transform, false);

            TubeGeometry tube = m_CurrentStrokeObj.GetComponent<TubeGeometry>();
            Debug.Assert(m_BrushCursorTransform.position != null);
            Debug.Assert(m_BrushCursorTransform.rotation != null);
            Debug.Assert(m_BrushColor != null);
            GameObject frontMeshObj = new GameObject("FrontMesh", typeof(MeshFilter), typeof(MeshRenderer));
            frontMeshObj.transform.SetParent(m_CurrentStrokeObj.transform, false);
            MeshRenderer frontMeshRenderer = frontMeshObj.GetComponent<MeshRenderer>();
            frontMeshRenderer.sharedMaterial = m_PaintMaterial;       // set shared base material
            Material customizedMaterial = frontMeshRenderer.material; // clones base material
            customizedMaterial.color = m_BrushColor;                  // customize the clone
            frontMeshRenderer.sharedMaterial = customizedMaterial;    // set shared to customized
            tube.SetMaterial(customizedMaterial);
            tube.SetNumFaces(8);
            tube.SetWrapTwice(true);
            
            tube.Init(m_BrushCursorTransform.position, m_BrushCursorTransform.rotation, 0f, 0f, m_BrushColor);
            m_strokeTransforms = new List<Vector3>();
            m_strokeSimilarities = new float[m_SplineContainer.Splines.Count];
            for (int i = 0; i<m_strokeSimilarities.Length; i++)
            {
                m_strokeSimilarities[i] = 0;
            }
        }

        public void Painting_OnUpdate()
        {
            // // find the points at the left edge and right edge of the brush bristles.  In the raw CavePainting brush
            // // model, these vertices are at (-0.5, 0, 0) and (0.5, 0, 0)
            // Vector3 leftBrushPtWorld = m_BrushCursorTransform.LocalPointToWorldSpace(new Vector3(-0.5f, 0, 0));
            // Vector3 rightBrushPtWorld = m_BrushCursorTransform.LocalPointToWorldSpace(new Vector3(0.5f, 0, 0));

            // // convert these into the local space of the stroke, which has already been added to the artwork parent
            // Vector3 leftBrushPtArtwork = m_CurrentStrokeObj.transform.WorldPointToLocalSpace(leftBrushPtWorld);
            // Vector3 rightBrushPtArtwork = m_CurrentStrokeObj.transform.WorldPointToLocalSpace(rightBrushPtWorld);


            // // ADD TO THE FRONT MESH

            // // push two new vertices for these points to back of the vertex list
            // m_CurrentStrokeFrontVertices.Add(leftBrushPtArtwork);
            // m_CurrentStrokeFrontVertices.Add(rightBrushPtArtwork);

            // // add two triangles to the stroke mesh to connect the last left/right points to the current ones
            // if (m_CurrentStrokeFrontVertices.Count >= 4) {
            //     // construct two traingles with the last four vertices added
            //     int v0 = m_CurrentStrokeFrontVertices.Count - 4; // last left
            //     int v1 = m_CurrentStrokeFrontVertices.Count - 3; // last right
            //     int v2 = m_CurrentStrokeFrontVertices.Count - 2; // cur left
            //     int v3 = m_CurrentStrokeFrontVertices.Count - 1; // cur right

            //     // tri #1 (note: Unity uses clockwise ordering)
            //     m_CurrentStrokeFrontIndices.Add(v0);
            //     m_CurrentStrokeFrontIndices.Add(v2);
            //     m_CurrentStrokeFrontIndices.Add(v3);

            //     // tri #2
            //     m_CurrentStrokeFrontIndices.Add(v0);
            //     m_CurrentStrokeFrontIndices.Add(v3);
            //     m_CurrentStrokeFrontIndices.Add(v1);

            //     // update the mesh
            //     m_CurrentStrokeFrontMesh.Clear();
            //     m_CurrentStrokeFrontMesh.SetVertices(m_CurrentStrokeFrontVertices);
            //     m_CurrentStrokeFrontMesh.SetIndices(m_CurrentStrokeFrontIndices, MeshTopology.Triangles, 0);
            //     m_CurrentStrokeFrontMesh.RecalculateNormals();
            // }


            // // ADD TO THE BACK MESH (only difference is the ordering of the vertices in the two triangles)

            // // push two new vertices for these points to back of the vertex list
            // m_CurrentStrokeBackVertices.Add(leftBrushPtArtwork);
            // m_CurrentStrokeBackVertices.Add(rightBrushPtArtwork);

            // // add two triangles to the stroke mesh to connect the last left/right points to the current ones
            // if (m_CurrentStrokeBackVertices.Count >= 4) {

            //     // construct two traingles with the last four vertices added
            //     int v0 = m_CurrentStrokeBackVertices.Count - 4; // last left
            //     int v1 = m_CurrentStrokeBackVertices.Count - 3; // last right
            //     int v2 = m_CurrentStrokeBackVertices.Count - 2; // cur left
            //     int v3 = m_CurrentStrokeBackVertices.Count - 1; // cur right

            //     // tri #1 (note: Unity uses clockwise ordering)
            //     m_CurrentStrokeBackIndices.Add(v0);
            //     m_CurrentStrokeBackIndices.Add(v3);
            //     m_CurrentStrokeBackIndices.Add(v2);

            //     // tri #2
            //     m_CurrentStrokeBackIndices.Add(v0);
            //     m_CurrentStrokeBackIndices.Add(v1);
            //     m_CurrentStrokeBackIndices.Add(v3);

            //     // update the mesh
            //     m_CurrentStrokeBackMesh.Clear();
            //     m_CurrentStrokeBackMesh.SetVertices(m_CurrentStrokeBackVertices);
            //     m_CurrentStrokeBackMesh.SetIndices(m_CurrentStrokeBackIndices, MeshTopology.Triangles, 0);
            //     m_CurrentStrokeBackMesh.RecalculateNormals();
            // }


            // Tube Geomtry start
            Vector3 brushScale = m_BrushCursorTransform.localScale;

            TubeGeometry tube = m_CurrentStrokeObj.GetComponent<TubeGeometry>();
            Debug.Assert(tube != null);
            tube.AddSample(m_BrushCursorTransform.position, m_BrushCursorTransform.rotation, brushScale.x, brushScale.y, m_BrushColor);
            m_strokeTransforms.Add(m_CurrentStrokeObj.transform.WorldPointToLocalSpace(m_BrushCursorTransform.position));
            float[] pointSimilarities = FindSplineSimilarities(m_BrushCursorTransform.LocalPointToWorldSpace(new Vector3 (0,0,0)), m_BrushCursorTransform.rotation);
            for (int i = 0; i < m_strokeSimilarities.Length; i++)
            {
                m_strokeSimilarities[i] += pointSimilarities[i]; 
            }
            // m_BrushCursorTransform.LocalPointToWorldSpace(
        }


        public void Painting_OnExit()
        {
            m_NumStrokes++;
            // List<Vector3> currCenters = new List<Vector3>();

            // // Get center of the line.
            // for (int i = 0; i < m_CurrentStrokeBackVertices.Count; i+=2)
            // {
            //     if (m_CurrentStrokeBackVertices.Count > i+1)
            //     {
            //         Vector3 v0 = m_CurrentStrokeBackVertices[i];
            //         Vector3 v1 = m_CurrentStrokeBackVertices[i+1];
            //         currCenters.Add((v0 + v1) / 2);
                    
            //         // Debug.Log("VERTICES LEFT LAST: " + v0 + " RIGHT LAST: " + v1);
            //     }
            // }
            // currCenters = currCenters.Distinct().ToList();

            // Debug.Log("CENTERS LAST: " + currCenters[^1] + " FIRST: " + currCenters[0]);

            
            if (m_strokeTransforms.Count > 0)
            {
                // Spline spline = FindClosestSpline(m_strokeTransforms, out int splineIndex, out Spline drawnSpline);//, out float splineStart, out float splineEnd);
                int splineIndex = FindClosestSplineIndex();
                Spline drawnSpline = new Spline();
                foreach (Vector3 t in m_strokeTransforms)
                {
                    drawnSpline.Add(new BezierKnot(t), TangentMode.AutoSmooth);
                }
                // m_SplineColoredContainer.AddSpline(drawnSpline);
                Spline spline = m_SplineContainer.Splines[splineIndex];
                
                // Debug.Log("INDEX CHECK " +splineIndex);
                Spline nearestSplineSegment = FindSplineSegmentUsingLength(spline, splineIndex, drawnSpline);

                GameObject go = new GameObject("Morph " + m_NumStrokes, typeof(Morphing));
                Morphing morph = go.GetComponent<Morphing>();
                morph.Init(m_CurrentStrokeObj.GetComponent<TubeGeometry>(), nearestSplineSegment, m_ArtworkParentTransform, m_BrushColor, m_BrushCursorTransform.localScale);
            }


            // Tube Geometry

            TubeGeometry tube = m_CurrentStrokeObj.GetComponent<TubeGeometry>();
            Debug.Assert(tube != null);
            tube.Complete(m_BrushCursorTransform.position, m_BrushCursorTransform.rotation, 0f, 0f, m_BrushColor);
            for (int i = 0; i<m_strokeSimilarities.Length; i++)
            {
                m_strokeSimilarities[i] = 0;
            }
        }

        public Spline FindClosestSpline(List<Vector3> centers, out int bestIndex, out Spline drawnSpline)//, out float splineStartIndex, out float splineEndIndex)
        {
            float splineStartIndex = 0;
            float splineEndIndex = 1;

            // Spline conversion is currently slightly shifted, possible due to float precision. 
            drawnSpline = new Spline();
            List<BezierKnot> knots = new List<BezierKnot>();
            foreach (Vector3 t in m_strokeTransforms)
            {
                drawnSpline.Add(new BezierKnot(t), TangentMode.AutoSmooth);
                // drawnSpline.Add(new BezierKnot(c));
                //knots.Add(new BezierKnot(new float3(x, y, z)));
            }
            // drawnSpline.Knots = knots;
            // m_SplineColoredContainer.AddSpline(drawnSpline);
            // Debug.Log("DRAWING SPLINE: " + drawnSpline.EvaluatePosition(1));

            Spline bestSpline = null;
            float bestSimilaritySpline = float.MaxValue;
            bestIndex = 0;
            for (int index = 0; index < m_SplineContainer.Splines.Count; index++)
            {
                Spline spline = m_SplineContainer.Splines[index];
                float splineSimilarity = 0;

                float currStartIndex = 0;
                float currEndIndex = sampleCount - 1;

                // Loop through points evenly spaced along the drawn spline
                for (int i = 0; i < sampleCount; i++)
                {
                    float t1 = i / (float)(sampleCount - 1); // Normalized parameter along spline
                    Vector3 p1 = drawnSpline.EvaluatePosition(t1);
                    Vector3 d1 = drawnSpline.EvaluateTangent(t1);

                    ///// COMPARISON FROM EACH POINT TO ANYWHERE ALONG THE SPLINE, needs d2 calculation still
                    // var distance = SplineUtility.GetNearestPoint(spline, p1, out var p2, out float _);
                    
                    // float similarity = w_dist * Mathf.Pow(distance, 2) + Mathf.Abs(w_dir * Vector3.Dot(d1, d2)); 

                    // splineSimilarity += similarity;
                    ///// END COMPARISON


                    ///// COMPARISON FROM EACH POINT TO ANY POINT ON THE SPLINE


                    float bestSimilarityPoint = float.MaxValue;
                    // Loop through points evenly spaced along the current spline comparison
                    for (int j = 0; j < sampleCount; j++)
                    {
                        float t2 = j / (float)(sampleCount - 1); // Normalized parameter along spline
                        Vector3 p2 = spline.EvaluatePosition(t2);
                        Vector3 d2 = spline.EvaluateTangent(t2);

                        float distance = Vector3.Distance(p1, p2);



                        float similarity = w_dist * Mathf.Pow(distance, 2) + Mathf.Abs(w_dir * Vector3.Dot(d1, d2)); 
                        if (similarity < bestSimilarityPoint)
                        {
                            bestSimilarityPoint = similarity;
                            if (i == 0)
                            {
                                currStartIndex = t2;
                            }
                            else if (i == sampleCount - 1)
                            {
                                currEndIndex = t2;
                            }
                            //bestPoint = i;
                        }
                    }

                    splineSimilarity += bestSimilarityPoint;
                    ///// END COMPARISON


                    ///// COMPARISON AT EACH SECTION TO SECTION
                    // Vector3 p2 = spline.EvaluatePosition(t1);
                    // Vector3 d2 = spline.EvaluateTangent(t1);

                    // float distance = Vector3.Distance(p1, p2);



                    // float similarity = w_dist * Mathf.Pow(distance, 2) + Mathf.Abs(w_dir * Vector3.Dot(d1, d2)); 

                    // splineSimilarity += similarity;
                    ///// END COMPARISON


                }
                if (splineSimilarity < bestSimilaritySpline)
                {
                    bestSimilaritySpline = splineSimilarity;
                    bestSpline = spline;
                    bestIndex = index;
                    splineStartIndex = currStartIndex;
                    splineEndIndex = currEndIndex;
                }
            }
            return bestSpline;
        }

        public int FindClosestSplineIndex()
        {
            int index = -1;
            float value = float.MaxValue;
            for (int i = 0; i < m_strokeSimilarities.Length; i++)
            {
                if (m_strokeSimilarities[i] < value)
                {
                    index = i;
                    value = m_strokeSimilarities[i];
                }
            }
            // Debug.Log("FINDING SPLINE INDEX: " + index);
            return index;
        }

        public float[] FindSplineSimilarities(Vector3 currPos, Quaternion currQuat)
        {
            int splineCount = m_SplineContainer.Splines.Count;
            float[] similarities = new float[splineCount];
            int testind = -1;
            Vector3[] test = new Vector3[2];
            // Vector3 currTan = (currQuat * Vector3.forward); // doesn't make sense due to rotation of hand does not mean the tangent of line.
            for (int i = 0; i < splineCount; i++)
            {
                similarities[i] = float.MaxValue;

                for (int j = 0; j < sampleCount; j++)
                {
                    float alongSpline = j / (float)(sampleCount - 1);
                    Vector3 splineTan = m_SplineContainer.transform.LocalPointToWorldSpace(m_SplineContainer.Splines[i].EvaluateTangent(alongSpline)); // need to adjust to world version
                    Vector3 splinePos = m_SplineContainer.transform.LocalPointToWorldSpace(m_SplineContainer.Splines[i].EvaluatePosition(alongSpline));

                    float distance = Vector3.Distance(currPos, splinePos);
                    // float distance = (currPos - splinePos).sqrMagnitude;
                    // float sim = w_dist * Mathf.Pow(distance, 2) + Mathf.Abs(w_dir * Vector3.Dot(currTan.normalized, splineTan.normalized)); 
                    float sim = w_dist * distance;// + Mathf.Abs(w_dir * Vector3.Dot(currTan.normalized, splineTan.normalized)); 

                    if (sim < similarities[i])
                    {
                        similarities[i] = sim;
                        test[0] = currPos;
                        test[1] = splinePos;
                        testind = i;
                    }
                }
            }
            // Debug.Log("Current: " + currPos);
            // Debug.Log("Closest: " + test[1] + " With index: " + testind);
            return similarities;
        }

        public Spline FindSplineSegment(Spline spline, int splineIndex, Spline drawnSpline)//Vector3 splineStart, Vector3 splineEnd)
        {
            int startIndex = -1;
            int endIndex = -1;
            float bestStartDist = float.MaxValue;
            float bestEndDist = float.MaxValue;
            Spline newSpline = new Spline();

            for (int i = 0; i < spline.Knots.Count(); i++)
            {
                var k = spline.Knots.ElementAt(i).Position;
                var t = (spline.Knots.ElementAt(i).TangentIn + spline.Knots.ElementAt(i).TangentOut) / 2;
                var startDist = Vector3.Distance(k, drawnSpline[0].Position);
                var endDist = Vector3.Distance(k, drawnSpline[^1].Position);

                // compare quaternions for direction?
                var startRot = (Quaternion)drawnSpline[0].Rotation * Vector3.forward;
                var endRot = (Quaternion)drawnSpline[^1].Rotation * Vector3.forward;



                float startSimilarity = w_dist * Mathf.Pow(startDist, 2) + Mathf.Abs(w_dir * Vector3.Dot(t, startRot)); 
                float endSimilarity = w_dist * Mathf.Pow(endDist, 2) + Mathf.Abs(w_dir * Vector3.Dot(t,endRot)); 


                // If the split position is between two control points, we split here
                if (startSimilarity < bestStartDist)
                {
                    bestStartDist = startSimilarity;
                    startIndex = i;
                }
                if (endSimilarity < bestEndDist)
                {
                    bestEndDist = endSimilarity;
                    endIndex = i;
                }
            }

            if (startIndex == float.MaxValue || endIndex == float.MaxValue)
            {
                Debug.Log("Error with split");
                Debug.Log("SPLIT INDEX1: " + startIndex + " SPLIT INDEX2: " +endIndex);

                return newSpline;
            }
            else if (startIndex == endIndex)
            {
                if (endIndex < spline.Knots.Count() - 1)
                {
                    endIndex += 1;
                }
                else
                {
                    startIndex -= 1;
                }
                Debug.Log("Split at same location");
            }
            int counter = 0;

            for (int i = Mathf.Min(startIndex, endIndex); i <= Mathf.Max(startIndex, endIndex); i++)
            {
                newSpline.Add(spline.Knots.ElementAt(i));
                counter ++;
            }

            m_SplineColoredContainer.AddSpline(newSpline);
            m_SplineColoredContainer.GetComponent<SplineExtrude>().Rebuild();
            return newSpline;
        }

        public Spline FindSplineSegmentUsingLength(Spline spline, int splineIndex, Spline drawnSpline)//Vector3 splineStart, Vector3 splineEnd)
        {
            int startIndex = -1;
            int endIndex = -1;
            float bestStartDist = float.MaxValue;
            float bestEndDist = float.MaxValue;
            Spline newSpline = new Spline();

            var startDrawnTan = (Quaternion)drawnSpline[0].Rotation * Vector3.forward;
            var endDrawnTan = (Quaternion)drawnSpline[^1].Rotation * Vector3.forward;

            float bestSimilarity = float.MaxValue;
            float[] precomputedKnotLengths = new float[spline.Knots.Count()];
            for (int i = 0; i < spline.Knots.Count(); i++)
            {
                precomputedKnotLengths[i] = spline.GetCurveLength(i);
            }

            for (int i = 0; i < spline.Knots.Count(); i++)
            {
                var startTargetKnot = spline.Knots.ElementAt(i);
                var startTargetPos = startTargetKnot.Position;
                var startTargetTan = (startTargetKnot.TangentIn + startTargetKnot.TangentOut) / 2;
                var startDist = Vector3.Distance(startTargetPos, drawnSpline[0].Position);
                float startSimilarity = w_dist * Mathf.Pow(startDist, 2) + Mathf.Abs(w_dir * Vector3.Dot(startDrawnTan, startTargetTan)); 
                for (int j = 0; j < spline.Knots.Count(); j++)
                {
                    if (i != j)
                    {
                        var endTargetKnot = spline.Knots.ElementAt(j);
                        var endTargetPos = endTargetKnot.Position;
                        var endTargetTan = (endTargetKnot.TangentIn + endTargetKnot.TangentOut) / 2;
                        var endDist = Vector3.Distance(endTargetPos, drawnSpline[^1].Position);
                        float endSimilarity = w_dist * Mathf.Pow(endDist, 2) + Mathf.Abs(w_dir * Vector3.Dot(endDrawnTan,endTargetTan));
                        // var targetLength = new SplineSlice<Spline>(spline, new SplineRange(Mathf.Min(i,j), Mathf.Abs(i - j))).GetLength();
                        var targetLength = precomputedKnotLengths[Mathf.Min(i, j)..Mathf.Max(i,j)].Sum(); 
                        float totalSimilarity = startSimilarity + endSimilarity + Mathf.Abs(drawnSpline.GetLength() - targetLength);
                        if (totalSimilarity < bestSimilarity)
                        {
                            bestSimilarity = totalSimilarity;
                            startIndex = i;
                            endIndex = j;
                            Debug.Log("TARGET LENGTH: " + targetLength + " DRAWN LENGTH: " + drawnSpline.GetLength());
                        }
                        
                    }
                }
            }

            if (startIndex == float.MaxValue || endIndex == float.MaxValue)
            {
                Debug.Log("Error with split");
                Debug.Log("SPLIT INDEX1: " + startIndex + " SPLIT INDEX2: " +endIndex);

                return newSpline;
            }
            int counter = 0;

            for (int i = Mathf.Min(startIndex, endIndex); i <= Mathf.Max(startIndex, endIndex); i++)
            {
                newSpline.Add(spline.Knots.ElementAt(i));
                counter ++;
            }

            //m_SplineColoredContainer.AddSpline(newSpline);
            //m_SplineColoredContainer.GetComponent<SplineExtrude>().Rebuild();
            return newSpline;
        }

        // TRANS-ROT-ARTWORK STATE CALLBACKS

        public void TransRotArtwork_OnEnter()
        {
            m_LastHandPos = m_HandCursorTransform.position;
            m_LastHandRot = m_HandCursorTransform.rotation;
        }

        public void TransRotArtwork_OnUpdate()
        {
            Vector3 handPosWorld = m_HandCursorTransform.position;
            Vector3 deltaPosWorld = handPosWorld - m_LastHandPos;

            Quaternion handRotWorld = m_HandCursorTransform.rotation;
            Quaternion deltaRotWorld = handRotWorld * Quaternion.Inverse(m_LastHandRot);

            m_ArtworkParentTransform.TranslateByWorldVector(deltaPosWorld);
            m_ArtworkParentTransform.RotateAroundWorldPoint(handPosWorld, deltaRotWorld);
            
            m_LastHandPos = handPosWorld;
            m_LastHandRot = handRotWorld;
        }

        public void TransRotArtwork_OnExit()
        {
        }


        // SCALE-ARTWORK STATE CALLBACKS

        public void ScaleArtwork_OnEnter()
        {
            m_LastBrushPos = m_BrushCursorTransform.position;
        }

        public void ScaleArtwork_OnUpdate()
        {
            Vector3 handPosWorld = m_HandCursorTransform.position;
            Vector3 brushPosWorld = m_BrushCursorTransform.position;
            Vector3 curSpan = handPosWorld - brushPosWorld;
            Vector3 lastSpan = m_LastHandPos - m_LastBrushPos;

            float deltaScale = curSpan.magnitude / lastSpan.magnitude;
            m_ArtworkParentTransform.ScaleAroundWorldPoint(handPosWorld, deltaScale);

            m_LastHandPos = handPosWorld;
            m_LastBrushPos = brushPosWorld;
        }

        // public void StartListening()
        // {
        //     VREngine.Instance.eventManager.AddEventListener(this);
        // }
        // public void StopListening()
        // {
        //     VREngine.Instance?.eventManager?.AddEventListener(this);
        // }

        // public void OnVREvent(VREvent vrEvent)
        // {
        //     if (vrEvent.Matches(m_BrushPosEvent))
        //     {
        //         m_BrushPosInWorld = vrEvent.GetData<Vector3>();
        //     }
        //     else if (vrEvent.Matches(m_BrushRotEvent))
        //     {
        //         m_BrushRotInWorld = vrEvent.GetData<Quaternion>();
        //     }
        // }

        [Tooltip("Parent Transform for any 3D geometry produced by painting.")]
        [SerializeField] private Transform m_ArtworkParentTransform;
        [Tooltip("The brush cursor mesh renderer.")]
        [SerializeField] private MeshRenderer m_BrushCursorMeshRenderer;
        [Tooltip("The transform of the brush cursor.")]
        [SerializeField] private Transform m_BrushCursorTransform;
        [Tooltip("The transform of the hand cursor.")]
        [SerializeField] private Transform m_HandCursorTransform;

        [Tooltip("The base material for the paint -- color is added to this.")]
        [SerializeField] private Material m_PaintMaterial;

        [Tooltip("The current brush color.")]
        [SerializeField] private Color m_BrushColor;


        // runtime only

        // for painting ribbon strokes
        private int m_NumStrokes;
        private GameObject m_CurrentStrokeObj;

        private Mesh m_CurrentStrokeFrontMesh;
        private List<Vector3> m_CurrentStrokeFrontVertices;
        private List<int> m_CurrentStrokeFrontIndices;

        private Mesh m_CurrentStrokeBackMesh;
        private List<Vector3> m_CurrentStrokeBackVertices;
        private List<int> m_CurrentStrokeBackIndices;

        // for other interactions
        private Vector3 m_LastHandPos;
        private Quaternion m_LastHandRot;
        private Vector3 m_LastBrushPos;

        // for stroke location testing
        private List<Vector3> m_CurrentStrokeLocations;

        [SerializeField] SplineContainer m_SplineContainer;

        [SerializeField] SplineContainer m_SplineColoredContainer;

        private List<Vector3> m_strokeTransforms;

        // Tube Geometry
        [SerializeField] private TubeGeometry m_TubeGeometry;
        private float w_dir = 1;
        private float w_dist = 10;
        private int sampleCount = 50;
        private float[] m_strokeSimilarities;
        
        // [Header("VR Events")]
        // [SerializeField] private VREventPrototypeVector3 m_BrushPosEvent;
        // [SerializeField] private VREventPrototypeQuaternion m_BrushRotEvent;
        // private Vector3 m_BrushPosInWorld;
        // private Quaternion m_BrushRotInWorld;
    }

} // namespace