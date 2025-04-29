using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataMapper : MonoBehaviour
{
    private void Reset()
    {
        m_ColorMap = GetComponent<ColorMap>();
        m_ArtworkRoot = GameObject.Find("Artwork");
    }

    public void ApplyDataMappingsToStrokes()
    {
        Debug.Log("Apply Data Mapping: ColorID=" + m_ColorDataBindingVariableId + " SizeID=" + m_SizeDataBindingVariableId);

        // get all the TubeGeometries we want to edit
        TubeGeometry[] tubes = m_ArtworkRoot.GetComponentsInChildren<TubeGeometry>();


        foreach (TubeGeometry t in tubes)
        {
            // get the data associated with the tube
            StrokeData strokeData = t.transform.GetComponentInChildren<StrokeData>();
            if (strokeData != null)
            {
                Debug.Log("Got Stroke Data");
                // List<string> featureNames = strokeData.getFeatureNames();

                // if (m_SizeDataBindingVariableId != VariableId_None)
                // {
                //     Debug.Log("Size bound to " + featureNames[m_SizeDataBindingVariableId]);
                //     strokeData.AdjustTubeWidth(featureNames[m_SizeDataBindingVariableId], m_MinSize, m_MaxSize, m_InverseWidthMaps);
                // }
                // else
                // {
                //     strokeData.ResetWidths();
                // }

                // // fill in the array of colors based on underlying data

                // Color[] colors = new Color[t.GetNumSamples()];
                // if (m_ColorDataBindingVariableId != VariableId_None)
                // {
                //     string colorFeatureName = featureNames[m_ColorDataBindingVariableId];
                //     Debug.Log("Color bound to " + featureNames[m_ColorDataBindingVariableId]);

                //     for (int i = 0; i < t.GetNumSamples(); i++)
                //     {
                //         float dataVal = strokeData.GetDataValueAlongSpline(colorFeatureName, t.GetFracAlongLine(i), m_InverseColorMaps);
                //         colors[i] = m_ColorMap.LookupColor(dataVal);
                //     }
                //     strokeData.SetColors(colors);
                // }
                // else
                // {
                //     // for (int i = 0; i < t.GetNumSamples(); i++)
                //     // {
                //     //     // TODO: Reset to original color
                //     //     colors[i] = Color.white;
                        
                //     // }
                //     strokeData.ResetColors();
                // }
                
                // // t.SetColorsPerSample(colors);
                // strokeData.TriggerMorph();
                ApplyDataMappingsToStroke(t, strokeData);
            }
        }
    }

    public void ApplyDataMappingsToStroke(TubeGeometry t, StrokeData strokeData)
    {
        List<string> featureNames = strokeData.getFeatureNames();

        if (m_SizeDataBindingVariableId != VariableId_None)
        {
            Debug.Log("Size bound to " + featureNames[m_SizeDataBindingVariableId]);
            strokeData.AdjustTubeWidth(featureNames[m_SizeDataBindingVariableId], m_MinSize, m_MaxSize, m_InverseWidthMaps);
        }
        else
        {
            strokeData.ResetWidths();
        }

        // fill in the array of colors based on underlying data

        Color[] colors = new Color[t.GetNumSamples()];
        if (m_ColorDataBindingVariableId != VariableId_None)
        {
            string colorFeatureName = featureNames[m_ColorDataBindingVariableId];
            Debug.Log("Color bound to " + featureNames[m_ColorDataBindingVariableId]);

            for (int i = 0; i < t.GetNumSamples(); i++)
            {
                float dataVal = strokeData.GetDataValueAlongSpline(colorFeatureName, t.GetFracAlongLine(i), m_InverseColorMaps);
                colors[i] = m_ColorMap.LookupColor(dataVal);
            }
            strokeData.SetColors(colors);
        }
        else
        {
            // for (int i = 0; i < t.GetNumSamples(); i++)
            // {
            //     // TODO: Reset to original color
            //     colors[i] = Color.white;
                
            // }
            strokeData.ResetColors();
        }
        
        // t.SetColorsPerSample(colors);
        strokeData.TriggerMorph();
    }

    public void InferUserMinMaxWidth()
    {
        TubeGeometry[] tubes = m_ArtworkRoot.GetComponentsInChildren<TubeGeometry>();
        float minWidth = -1;
        float minVal = 0;
        float maxWidth = -1;
        float maxVal = 0;
        foreach (TubeGeometry t in tubes)
        {
            StrokeData strokeData = t.transform.GetComponentInChildren<StrokeData>();
            if (strokeData != null)
            {
                List<string> featureNames = strokeData.getFeatureNames();
                string widthBindingFeature = featureNames[m_SizeDataBindingVariableId];
                (float drawnWidth, float bindingValue) = strokeData.GetStrokeInfoWidth(widthBindingFeature);
                if (minWidth > drawnWidth || minWidth == -1)
                {
                    minWidth = drawnWidth;
                    minVal = bindingValue;
                }
                else if (maxWidth < drawnWidth)
                {
                    maxWidth = drawnWidth;
                    maxVal = bindingValue;
                }
            }
        }
        Debug.Log("Min Value: " + minVal + " Max Value: " + maxVal);
        
        if (minVal > maxVal)
        {
            m_InverseWidthMaps = true;
            float tmp = minVal;
            minVal = maxVal;
            maxVal = tmp;
        }
        if (maxVal - minVal < 0.01)
        {
            maxVal += 0.01f;
        }
        m_MaxSize = maxWidth;
        m_MinSize = minWidth;
        ApplyDataMappingsToStrokes();
    }

    public void InferUserColorMap()
    {
        TubeGeometry[] tubes = m_ArtworkRoot.GetComponentsInChildren<TubeGeometry>();
        m_ColorMap.controlPts.Clear();
        foreach (TubeGeometry t in tubes)
        {
            StrokeData strokeData = t.transform.GetComponentInChildren<StrokeData>();
            if (strokeData != null)
            {
                List<string> featureNames = strokeData.getFeatureNames();
                string colorBindingFeature = featureNames[m_ColorDataBindingVariableId];
                (Color color, float averageBoundValue) = strokeData.GetStrokeInfoColor(colorBindingFeature);
                m_ColorMap.AddControlPt(averageBoundValue, color);
            }
        }
        
        ApplyDataMappingsToStrokes();
    }

    public int GetColorDataBindingVariableId()
    {
        return m_ColorDataBindingVariableId;
    }

    public void SetColorDataBinding(int variableId)
    {
        // if (m_ColorDataBindingVariableId != variableId)
        // {
            m_ColorDataBindingVariableId = variableId;
            ApplyDataMappingsToStrokes();
        // }
    }

    public void ClearColorDataBinding()
    {
        // if (m_ColorDataBindingVariableId != VariableId_None)
        // {
            m_ColorDataBindingVariableId = VariableId_None;
            ApplyDataMappingsToStrokes();

        // }
    }



    public int GetSizeDataBindingVariableId()
    {
        return m_SizeDataBindingVariableId;
    }

    public void SetSizeDataBinding(int variableId)
    {
        // if (m_SizeDataBindingVariableId != variableId)
        // {
            m_SizeDataBindingVariableId = variableId;

            ApplyDataMappingsToStrokes();
        // }
    }

    public void ClearSizeDataBinding()
    {
        // if (m_SizeDataBindingVariableId != VariableId_None)
        // {
            m_SizeDataBindingVariableId = VariableId_None;
            ResetWidthParams();
            ApplyDataMappingsToStrokes();
        // }
    }

    public void ResetWidthParams()
    {
        m_MinSize = 0.005f;
        m_MaxSize = 0.3f;
    }


    public const int VariableId_None = -1;

    [SerializeField] private int m_ColorDataBindingVariableId;
    [SerializeField] private ColorMap m_ColorMap;

    [SerializeField] private int m_SizeDataBindingVariableId;
    [SerializeField] private float m_MinSize = -1;
    [SerializeField] private float m_MaxSize = -1;

    [SerializeField] private GameObject m_ArtworkRoot;
    [SerializeField] private bool m_InverseColorMaps = false;
    [SerializeField] private bool m_InverseWidthMaps = false;
}
