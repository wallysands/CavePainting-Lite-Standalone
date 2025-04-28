using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;

public class MenuFunctions : MonoBehaviour
{
    public DataMapper dataMapper;

    public FloatingMenu presetMenu;
    public List<TextAsset> colorMapPresets;


    public void Start()
    {
        presetMenu.menuItems.Clear();
        for (int i = 0; i < colorMapPresets.Count; i++)
        {
            Debug.Log(colorMapPresets[i].name);
            presetMenu.menuItems.Add(colorMapPresets[i].name);
        }
        presetMenu.RebuildMenu();
    }

    public void OnColorMenuItemSelected(int itemId)
    {
        // -1 = clear data binding
        //  0 = bind to variable #0
        //  1 = bind to variable #1
        //  etc.
        dataMapper.SetColorDataBinding(itemId - 1);
    }

    public void OnSizeMenuItemSelected(int itemId)
    {
        // -1 = clear data binding
        //  0 = bind to variable #0
        //  1 = bind to variable #1
        //  etc.
        dataMapper.SetSizeDataBinding(itemId - 1);
    }


    public void OnBrushModeMenuItemSelected(int itemId)
    {

    }

    public void OnPresetMenuItemSelected(int itemId)
    {
        ColorMap cm = dataMapper.GetComponent<ColorMap>();
        cm.SetFromXMLFile(colorMapPresets[itemId]);
        dataMapper.ApplyDataMappingsToStrokes();
    }
}
