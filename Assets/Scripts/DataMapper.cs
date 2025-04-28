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
                List<string> featureNames = strokeData.getFeatureNames();

                if (m_SizeDataBindingVariableId != VariableId_None)
                {
                    Debug.Log("Size bound to " + featureNames[m_SizeDataBindingVariableId]);
                    strokeData.AdjustTubeWidth(featureNames[m_SizeDataBindingVariableId], m_MinSize, m_MaxSize);
                }
                else
                {
                    // TODO: Reset to original width
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
                        float dataVal = strokeData.GetDataValueAlongSpline(colorFeatureName, t.GetFracAlongLine(i));
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
        }
    }


    public int GetColorDataBindingVariableId()
    {
        return m_ColorDataBindingVariableId;
    }

    public void SetColorDataBinding(int variableId)
    {
        if (m_ColorDataBindingVariableId != variableId)
        {
            m_ColorDataBindingVariableId = variableId;
            ApplyDataMappingsToStrokes();
        }
    }

    public void ClearColorDataBinding()
    {
        if (m_ColorDataBindingVariableId != VariableId_None)
        {
            m_ColorDataBindingVariableId = VariableId_None;
            ApplyDataMappingsToStrokes();
        }
    }



    public int GetSizeDataBindingVariableId()
    {
        return m_SizeDataBindingVariableId;
    }

    public void SetSizeDataBinding(int variableId)
    {
        if (m_SizeDataBindingVariableId != variableId)
        {
            m_SizeDataBindingVariableId = variableId;
            ApplyDataMappingsToStrokes();
        }
    }

    public void ClearSizeDataBinding()
    {
        if (m_SizeDataBindingVariableId != VariableId_None)
        {
            m_SizeDataBindingVariableId = VariableId_None;
            ApplyDataMappingsToStrokes();
        }
    }


    public const int VariableId_None = -1;

    [SerializeField] private int m_ColorDataBindingVariableId;
    [SerializeField] private ColorMap m_ColorMap;

    [SerializeField] private int m_SizeDataBindingVariableId;
    [SerializeField] private float m_MinSize = -1;
    [SerializeField] private float m_MaxSize = -1;

    [SerializeField] private GameObject m_ArtworkRoot;
}
