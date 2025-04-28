using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;

public class MenuFunctions : MonoBehaviour
{
    public DataMapper dataMapper;
    public SplineFieldMaker splineFieldMaker;
    public FloatingToggleButtons colorBindingMenu;
    public FloatingToggleButtons sizeBindingMenu;
    public FloatingMenu presetMenu;
    public List<TextAsset> colorMapPresets;


    public void Start()
    {
        // // build preset menu based on colormaps
        // presetMenu.menuItems.Clear();
        // for (int i = 0; i < colorMapPresets.Count; i++)
        // {
        //     Debug.Log(colorMapPresets[i].name);
        //     presetMenu.menuItems.Add(colorMapPresets[i].name);
        // }
        // presetMenu.RebuildMenu();

        // build data menus
        string[] varNames = splineFieldMaker.featureHeaders;
        foreach (string name in varNames) {
            colorBindingMenu.menuItems.Add(new FloatingToggleButtons.MenuItem(name, false));
            sizeBindingMenu.menuItems.Add(new FloatingToggleButtons.MenuItem(name, false));
        }
        colorBindingMenu.RebuildMenu();
        sizeBindingMenu.RebuildMenu();
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
