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
    public FloatingToggleButtons brushModeMenu;
    public MainPaintingAndReframingUI mainPaintingAndReframingUI;
    public FloatingMenu presetMenu;
    public List<TextAsset> colorMapPresets;
    public Artwork artwork;

    public void Start()
    {
        // build preset menu based on colormaps
        // presetMenu.menuItems.Clear();
        for (int i = 0; i < colorMapPresets.Count; i++)
        {
            Debug.Log(colorMapPresets[i].name);
            presetMenu.menuItems.Add(colorMapPresets[i].name);
        }
        presetMenu.RebuildMenu();

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
        mainPaintingAndReframingUI.SetStrokeType(itemId);
        if (itemId == 0 || itemId == 1)
        {
            colorBindingMenu.gameObject.SetActive(true);
            sizeBindingMenu.gameObject.SetActive(true);
        }
        else
        {
            colorBindingMenu.gameObject.SetActive(false);
            sizeBindingMenu.gameObject.SetActive(false);
        }
    }

    public void OnPresetMenuItemSelected(int itemId)
    {
        int i = itemId - 4; // offset to skip manually created buttons
        if (i >= 0)
        {
            ColorMap cm = dataMapper.GetComponent<ColorMap>();
            cm.SetFromXMLFile(colorMapPresets[i]);
            dataMapper.ApplyDataMappingsToStrokes();
        }
        else if (i == -4) // Reset Artwork
        {
            artwork.Clear();
            dataMapper.ClearColorDataBinding();
            dataMapper.ClearSizeDataBinding();
        }
        else if (i == -3) // Reset Width Min/Max
        {
            dataMapper.ResetWidthParams();
        }
        else if (i == -2) // Infer Stroke Width
        {
            if (! sizeBindingMenu.menuItems[0].pressed)
            {
                Debug.Log("INFERING WIDTH");
                dataMapper.InferUserMinMaxWidth();
            }
        }
        else if (i == -1) // Infer Stroke Color Map
        {
            if (! colorBindingMenu.menuItems[0].pressed)
            {
                Debug.Log("INFERING COLOR");
                dataMapper.InferUserColorMap();
            }
        }
    }
}
