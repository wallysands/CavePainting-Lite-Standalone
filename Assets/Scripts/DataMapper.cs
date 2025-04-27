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
        // get all the TubeGeometries we want to edit
        TubeGeometry[] tubes = m_ArtworkRoot.GetComponentsInChildren<TubeGeometry>();

        Debug.Log("Apply " + tubes.Length);

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


    [SerializeField] private ColorMap m_ColorMap;
    [SerializeField] private GameObject m_ArtworkRoot;
}
