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

        foreach (TubeGeometry t in tubes) {
            // get the center position of each sample along the tube
            List<Vector3> positions = t.GetSamplePositions();

            // create an array of colors, one per sample
            Color[] colors = new Color[positions.Count];

            // fill in the array of colors based on underlying data
            for (int i = 0; i < positions.Count; i++) {
                // TODO: replace the next line with looking data value at positions[i] and converting it to a 0..1 range
                float dataVal = (float)i / (float)(positions.Count - 1);
                colors[i] = m_ColorMap.LookupColor(dataVal);
            }
            t.SetColorsPerSample(colors);
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
    [SerializeField] private float m_MinSize;
    [SerializeField] private float m_MaxSize;

    [SerializeField] private GameObject m_ArtworkRoot;
}
