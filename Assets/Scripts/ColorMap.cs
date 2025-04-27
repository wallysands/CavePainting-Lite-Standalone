/* CSci-5609 Support Code created by Prof. Dan Keefe, Fall 2023 */

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Linq;

/// <summary>
/// A color map class that supports:
/// * colors defined at multiple control points that do not need to be evenly spaced
/// * interpolation in Lab space
/// * initializing the map from:
///   - Paraview .xml format (used on sciviscolor.org)
///   - a regular 2D texture with a gradient running left to right
///   - a list of control points defined directly in the Unity editor
/// </summary>
public class ColorMap : MonoBehaviour
{
    /// <summary>
    /// small internal class to store control point data/color pairs
    /// </summary>
    [Serializable]
    public class ControlPt
    {
        public ControlPt(float d, Color c)
        {
            dataVal = d;
            col = c;
        }
        public float dataVal;
        public Color col;
    }

    /// <summary>
    /// One of these methods is used to initialize the colormap during Start()
    /// </summary>
    public enum InitializationMethod
    {
        FromVisAsset,
        FromTexture2D,
        FromXML,
        FromControlPoints
    };


    [Header("Initialization (set one of these OR add control points manually)")]

    [Tooltip("[Optional] If set, the ColorMap will load control points from this Texture2D during Start()." +
        "In Unity's settings for the Texture, 'Read/Write Enabled' must be set to on.")]
    public Texture2D texture;

    [Tooltip("[Optional] If set, the ColorMap will load control points from this XML file in your Assets " +
        "folder during Start() -- uses the Paraview ColorMap XML format used on sciviscolor.org.")]
    public TextAsset xmlFile;



    [Header("[Optional] Override Data Range")]

    [Tooltip("Turn on to override the range of the control map, treating the dataVal at the first control " +
        "point as if it were the custom min set below and the dataVal as the last control point as if it " +
        "were the custom max set below.")]
    public bool useCustomMinMax;

    [Tooltip("Min value for the data range -- used only when useCustomMinMax is turned on.")]
    public float customMinDataValue;

    [Tooltip("Max value for the data range -- used only when useCustomMinMax is turned on.")]
    public float customMaxDataValue;



    [Header("[Optional] Override Color for Out-of-Range Data")]

    [Tooltip("By default the colormap will 'stretch' to handle data values that are less than or " +
        "greater than the min and max values specified by the control points, returning the first color " +
        "for data <= the min data value in the control points and the last color for anything >= the max. " +
        "Turning this on will instead return a special color to signify the data are outside the color " +
        "map's range.")]
    public bool useSpecialColorForOutOfRange;

    [Tooltip("Color to apply to data that fall outside the range of the colormap's control points -- used " +
        "only when useSpecialColorForOutOfRange is turned on.")]
    public Color outOfRangeColor;



    [Header("Current Mapping")]

    [Tooltip("The current colormap as defined by control points.  Note, if any of the initialization " +
        "options above are set, then those take precedence, and these values will be overwritten on " +
        "Start().")]
    public List<ControlPt> controlPts;




    /// <summary>
    /// (Re)sets default values for variables when the script is added to a GameObject or when Reset is selected from the
    /// script's [...] menu in the editor.  So, Reset() is used in a way similar to a constructor.  
    /// </summary>
    private void Reset()
    {
        texture = null;
        xmlFile = null;
        useCustomMinMax = false;
        customMinDataValue = 0;
        customMaxDataValue = 1;
        useSpecialColorForOutOfRange = false;
        outOfRangeColor = Color.black;
        controlPts = null;
    }


    /// <summary>
    /// Called automatically by Unity. The ColorMap can optionally be initialized here by loading control points from
    /// a visasset or other resource.
    /// </summary>
    private void Start()
    {
        if (texture != null)
        {
            SetFromTexture2D(texture);
        }
        else if (xmlFile != null)
        {
            SetFromXMLFile(xmlFile);
        }
    }

    /// <summary>
    /// When custom data min/max values are used, this implies a change in the whole
    /// colormap, i.e., every control should have its dataVal adjusted so that the
    /// first color maps to the custom data min and the last color maps to the custom
    /// data max, and all the points between should maintain their relative positioning.
    /// This would destroy the original colormap and be a bit of a challenge to manage.
    /// However, we can get the same effect as changing colormap's data range by
    /// leaving the colormap alone and instead adjusting the dataVal that we look up.
    /// This function is used to accomplish that.  If useCustomMinMax is turned on,
    /// then maps the dataVal's position relative to the custom min/max values to a
    /// corresponding position relative to the min/max values of the original colormap,
    /// i.e., the dataVal for the first control point and the dataVal for the last
    /// control point.  If useCustomMinMax is turned off, the function simpy returns
    /// the dataVal unchanged.
    /// </summary>
    public float GetDataValueWithCustomMinMax(float dataVal)
    {
        if (useCustomMinMax)
        {
            float dataMinVal = customMinDataValue;
            float dataMaxVal = customMaxDataValue;
            float dataVal01 = (dataVal - dataMinVal) / (dataMaxVal - dataMinVal);
            float colorMinVal = controlPts[0].dataVal;
            float colorMaxVal = controlPts[controlPts.Count - 1].dataVal;
            float colorVal = colorMinVal + dataVal01 * (colorMaxVal - colorMinVal);
            return colorVal;
        }
        else
        {
            return dataVal;
        }
    }

    /// <summary>
    /// The main function to use at runtime.  This looks up a color in the colormap by
    /// datavalue, using interpolation in Lab space to find colors between control points.
    /// If useCustomMinMax is turned on, then the colormap is interpreted as ranging
    /// between customMinDataValue and customMaxDataValue rather than the data values
    /// associated with the first and last control points. If dataVal lies outside of the
    /// min and max dataValues for the map, then the color at the closest control
    /// point is returned, unless useSpecialColorForDataOutsideRange is set.  In that case
    /// colorForOutsideDataRange is returned.
    /// </summary>
    public Color LookupColor(float dataVal)
    {
        if (controlPts.Count == 0)
        {
            Debug.LogWarning("ColorMap::lookupColor called for an empty color map!");
            return outOfRangeColor;
        }
        else if (controlPts.Count == 1)
        {
            return controlPts[0].col;
        }
        else
        {

            //
            // appldata value is adjusted before
            // the lookup n the dataVal needs to be adjusted
            // so that the colormap 
            dataVal = GetDataValueWithCustomMinMax(dataVal);
            float minVal = controlPts[0].dataVal;
            float maxVal = controlPts[controlPts.Count - 1].dataVal;

            // check bounds
            if (dataVal >= maxVal)
            {
                if (useSpecialColorForOutOfRange)
                {
                    return outOfRangeColor;
                }
                else
                {
                    return controlPts[controlPts.Count - 1].col;
                }
            }
            else if (dataVal <= minVal)
            {
                if (useSpecialColorForOutOfRange)
                {
                    return outOfRangeColor;
                }
                else
                {
                    return controlPts[0].col;
                }
            }
            else
            {  // value within bounds

                // make i = upper control pt and (i-1) = lower control point
                int i = 1;
                while (controlPts[i].dataVal < dataVal)
                {
                    i++;
                }

                // find the amount to interpolate between the two control points
                float v1 = controlPts[i - 1].dataVal;
                float v2 = controlPts[i].dataVal;
                float alpha = (dataVal - v1) / (v2 - v1);

                // use lab space to interpolate between the colors at the two control points
                Color c1 = controlPts[i - 1].col;
                Color c2 = controlPts[i].col;

                List<float> rgb1 = Lab2Rgb.color2list(c1);
                List<float> rgb2 = Lab2Rgb.color2list(c2);
                List<float> lab1 = Lab2Rgb.rgb2lab(rgb1);
                List<float> lab2 = Lab2Rgb.rgb2lab(rgb2);

                List<float> labFinal = new List<float> {
                        lab1[0] * (1.0f - alpha) + lab2[0] * alpha,
                        lab1[1] * (1.0f - alpha) + lab2[1] * alpha,
                        lab1[2] * (1.0f - alpha) + lab2[2] * alpha
                    };

                List<float> rgbFinal = Lab2Rgb.lab2rgb(labFinal);

                return Lab2Rgb.list2color(rgbFinal);
            }
        }
    }


    /// <summary>
    /// Called automatically in Start() if InitializationMethod == FromTexture2D.  If called at runtime, this will
    /// replace the current color map with a new one.
    /// </summary>
    public void SetFromTexture2D(Texture2D texture, int numSamples = 11)
    {
        controlPts = new List<ControlPt>();
        for (int i = 0; i < numSamples; i++)
        {
            float val = (float)i / (numSamples - 1);
            AddControlPt(val, texture.GetPixelBilinear(val, 0.5f));
        }
    }

    /// <summary>
    /// Called automatically in Start() if InitializationMethod == FromXMLFile.  If called at runtime, this will
    /// replace the current color map with a new one.
    /// </summary>
    public void SetFromXMLFile(TextAsset xmlFile)
    {
        SetFromXML(xmlFile.text);
    }

    /// <summary>
    /// Creates a ColorMap from control points stored in ParaView's XML ColorMap format.
    /// This is the format used for the maps published on sciviscolor.org
    /// </summary>
    public void SetFromXML(string xmlText)
    {
        controlPts = new List<ControlPt>();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);

        XmlNode colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMaps/ColorMap");
        if (colormapNode == null)
            colormapNode = doc.DocumentElement.SelectSingleNode("/ColorMap");

        foreach (XmlNode pointNode in colormapNode.SelectNodes("Point"))
        {
            float x = float.Parse(pointNode.Attributes.GetNamedItem("x").Value);
            float r = float.Parse(pointNode.Attributes.GetNamedItem("r").Value);
            float g = float.Parse(pointNode.Attributes.GetNamedItem("g").Value);
            float b = float.Parse(pointNode.Attributes.GetNamedItem("b").Value);

            Color toAdd = new Color(r, g, b, 1.0f);
            AddControlPt(x, toAdd);
        }
    }



    /// <summary>
    /// Adds to the existing list of control points, unless a control point already exists with this
    /// data value.  In that case, the existing control point is updated with this color.
    /// </summary>
    public void AddControlPt(float dataVal, Color col)
    {
        // remove any control point already associated with this dataVal
        RemoveControlPt(dataVal, false);
        controlPts.Add(new ControlPt(dataVal, col));
        // re-sort the list by dataVal
        controlPts = controlPts.OrderBy(c => c.dataVal).ToList();
    }

    /// <summary>
    /// Returns true if a control point with the dataVal was found.  Otherwise, returns false and optionally
    /// prints a warning.
    /// </summary>
    bool RemoveControlPt(float dataVal, bool warnOnNotFound = true)
    {
        int i = 0;
        while (i < controlPts.Count)
        {
            if (controlPts[i].dataVal == dataVal)
            {
                controlPts.RemoveAt(i);
                return true;
            }
            i++;
        }
        if (warnOnNotFound)
        {
            Debug.LogWarning("ColorMap::removeControlPt no control point with data val = " + dataVal);
        }
        return false;
    }




    // ----- BEGIN EXTERNAL CODE FOR RGB-LAB CONVERSION -----
    // https://github.com/antimatter15/rgb-lab

    /*
    MIT License
    Copyright (c) 2014 Kevin Kwok <antimatter15@gmail.com>
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    // the following functions are based off of the pseudocode
    // found on www.easyrgb.com

    /// <summary>
    /// Conversions for RGB and CIE Lab color spaces and Unity color objects
    /// </summary>
    public class Lab2Rgb
    {
        public static List<float> color2list(Color color)
        {
            return new List<float> {
                color.r,
                color.g,
                color.b
            };
        }

        public static Color list2color(List<float> list)
        {
            return new Color(list[0], list[1], list[2], 1.0f);
        }

        public static List<float> lab2rgb(List<float> lab)
        {
            float y = (lab[0] + 16.0f) / 116.0f,
                x = lab[1] / 500.0f + y,
                z = y - lab[2] / 200.0f,
                r, g, b;

            x = 0.95047f * ((x * x * x > 0.008856f) ? x * x * x : (x - 16.0f / 116.0f) / 7.787f);
            y = 1.00000f * ((y * y * y > 0.008856f) ? y * y * y : (y - 16.0f / 116.0f) / 7.787f);
            z = 1.08883f * ((z * z * z > 0.008856f) ? z * z * z : (z - 16.0f / 116.0f) / 7.787f);

            r = x * 3.2406f + y * -1.5372f + z * -0.4986f;
            g = x * -0.96890f + y * 1.8758f + z * 0.0415f;
            b = x * 0.05570f + y * -0.2040f + z * 1.0570f;

            r = (r > 0.0031308f) ? (1.055f * Mathf.Pow(r, 1.0f / 2.4f) - 0.055f) : 12.92f * r;
            g = (g > 0.0031308f) ? (1.055f * Mathf.Pow(g, 1.0f / 2.4f) - 0.055f) : 12.92f * g;
            b = (b > 0.0031308f) ? (1.055f * Mathf.Pow(b, 1.0f / 2.4f) - 0.055f) : 12.92f * b;

            // Colors are already 0-1 in Unity
            return new List<float> {
                Mathf.Max(0.0f, Mathf.Min(1.0f, r)),
                Mathf.Max(0.0f, Mathf.Min(1.0f, g)),
                Mathf.Max(0.0f, Mathf.Min(1.0f, b))
            };
        }


        public static List<float> rgb2lab(List<float> rgb)
        {
            // Colors are already 0-1 in Unity
            // var r = rgb[0] / 255,
            //     g = rgb[1] / 255,
            //     b = rgb[2] / 255,
            float r = rgb[0],
                g = rgb[1],
                b = rgb[2],
                x, y, z;

            r = (r > 0.04045f) ? Mathf.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
            g = (g > 0.04045f) ? Mathf.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
            b = (b > 0.04045f) ? Mathf.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

            x = (r * 0.4124f + g * 0.3576f + b * 0.1805f) / 0.95047f;
            y = (r * 0.2126f + g * 0.7152f + b * 0.0722f) / 1.00000f;
            z = (r * 0.0193f + g * 0.1192f + b * 0.9505f) / 1.08883f;

            x = (x > 0.008856f) ? Mathf.Pow(x, 1.0f / 3.0f) : (7.787f * x) + 16.0f / 116.0f;
            y = (y > 0.008856f) ? Mathf.Pow(y, 1.0f / 3.0f) : (7.787f * y) + 16.0f / 116.0f;
            z = (z > 0.008856f) ? Mathf.Pow(z, 1.0f / 3.0f) : (7.787f * z) + 16.0f / 116.0f;

            return new List<float> {
                (116.0f * y) - 16.0f,
                500.0f * (x - y),
                200.0f * (y - z)
            };
        }
    }
    // ----- END EXTERNAL CODE FOR RGB-LAB CONVERSION -----
}

