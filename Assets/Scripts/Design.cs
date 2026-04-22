using System.Collections.Generic;
using UnityEngine;
public class Design
{
    public readonly string Name = "";
    // Code friendly name of the design, with no spaces
    public string VariableName => Name.Replace(" ", "");
    public readonly string path;
    public Design parent;
    
    public struct ShapeLayer : ILayer
    {
        public Shape shape;
        
        public ShapeLayer(Shape shape)
        {
            this.shape = shape;
        }
    }
    public struct DesignLayer : ILayer
    {
        public Design design;
        public Vector2 position => design.position;
        public DesignLayer(Design design)
        {
            this.design = design;
        }
        public DesignLayer(string path, Design parent)
        {
            design = CanvasManager.LoadDesign(path);
            if(design != null) design.parent = parent;
        }
    }
    public List<ILayer> layers;

    private Vector2 position = Vector2.zero;
    // Local position of the design relative to its parent.
    public Vector2 LocalPosition
    {
        get => position;
        set
        {
            position = value;
        }
    }
    // Global position of the design, calculated by adding the local position to the parent's global position.
    public Vector2 Position
    {
        get => position + (parent != null ? parent.Position : Vector2.zero);
    }
    // Center of the design, calculated as the average of the centers of all layers.
    public Vector2 Center
    {
        get
        {
            Vector2 sum = Vector2.zero;
            foreach (ILayer layer in layers)
            {
                if (layer is ShapeLayer shapeLayer)
                {
                    sum += shapeLayer.shape.Center;
                }
                else if (layer is DesignLayer designLayer)
                {
                    sum += designLayer.design.Center;
                }
            }
            return sum / layers.Count;
        }
    }

    public Design(string name)
    {
        layers = new List<ILayer>();
        Name = name;
        path = PathFromName(name);
    }
    public Design(DesignData data, string Path)
    {
        Name = data.Name;
        path = Path;
        layers = new List<ILayer>();
        // Loading layers based on their type and the data they contain
        for (int i = 0; i < data.layers.Length; i++)
        {
            if (data.layers[i].layerType == "shape")
            {
                layers.Add(new ShapeLayer(Shape.ShapeFromData(data.layers[i].shapeData, this)));
            }
            else if (data.layers[i].layerType == "design")
            {
                DesignLayer designLayer = new DesignLayer(data.layers[i].path, this);
                if (designLayer.design == null) continue;
                designLayer.design.position = data.layers[i].position;
                layers.Add(designLayer);
            }
        }
    }
    public string ExtractJson()
    {
        return JsonUtility.ToJson(new DesignData(this));
    }
    public string GetCode(bool makePositionVariable)
    {
        string code = "\t// " + Name;
        if (makePositionVariable)
        {
            if(parent == null)
            {
                code += $"\n\tvar {VariableName}X = {position.x};";
                code += $"\n\tvar {VariableName}Y = {-position.y};";
            }
            else
            {
                code += $"\n\tvar {VariableName}X = {(position.x == 0 ? "" : (position.x) + " + ") + parent.VariableName + "X"};";
                code += $"\n\tvar {VariableName}Y = {(position.y == 0 ? "" : (-position.y) + " + ") + parent.VariableName + "Y"};";
            }
        }

        float strokeWeight = -1f;
        bool noStroke = false;
        Color strokeColour = Color.clear;
        Color fillColour = Color.clear;
        bool noFill = false;


        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] is ShapeLayer shapeLayer)
            {
                Shape shape = shapeLayer.shape;
                // ---- Formatting shape ---- //
                // Stroke color is forced if the shape has a stroke and the previous shape had no stroke,
                // to prevent the new stroke color from being ignored due to the noStroke() call
                bool forceStrokeColour = false;
                // Stroke weight
                if (!shape.NoStroke && shape.StrokeWeight != strokeWeight)
                {
                    strokeWeight = shape.StrokeWeight;
                    code += $"\n\tstrokeWeight({strokeWeight});";
                }
                // No Stroke
                if (shape.NoStroke != noStroke)
                {
                    noStroke = shape.NoStroke;
                    if (noStroke) code += "\n\tnoStroke();";
                    else forceStrokeColour = true;
                }
                // Stroke color
                if (forceStrokeColour || (!shape.NoStroke && shape.StrokeColor != strokeColour))
                {
                    strokeColour = shape.StrokeColor;
                    bool colourChannelsAreSame = strokeColour.r == strokeColour.b && strokeColour.r == strokeColour.g;
                    bool noTransparency = strokeColour.a == 1;
                    // Using the correct stroke() function overload
                    if (colourChannelsAreSame && noTransparency)
                    {
                        code += $"\n\tstroke({(int)(strokeColour.r * 255)});";
                    }
                    else if (noTransparency)
                    {
                        code += $"\n\tstroke({(int)(strokeColour.r * 255)}, {(int)(strokeColour.g * 255)}, {(int)(strokeColour.b * 255)});";
                    }
                    else
                    {
                        code += $"\n\tstroke({(int)(strokeColour.r * 255)}, {(int)(strokeColour.g * 255)}, {(int)(strokeColour.b * 255)}, {(int)(strokeColour.a * 255)});";
                    }
                }

                // Fill
                if (shape is not CustomShape || shape.PointCount > 2)
                {
                    // We write fill code if the color changed,
                    // or if the previous shape had no fill
                    bool writeFillCode = false;
                    if (shape.NoFill != noFill)
                    {
                        noFill = shape.NoFill;
                        if (shape.NoFill)
                        {
                            code += "\n\tnoFill();";
                        }
                        else
                        {
                            writeFillCode = true;
                        }
                    }
                    else if (!shape.NoFill && shape.FillColor != fillColour)
                    {
                        writeFillCode = true;
                    }

                    if (writeFillCode)
                    {
                        fillColour = shape.FillColor;
                        bool colourChannelsAreSame = fillColour.r == fillColour.b && fillColour.r == fillColour.g;
                        bool noTransparency = fillColour.a == 1;
                        // Using the correct fill() function overload
                        if (colourChannelsAreSame && noTransparency)
                        {
                            code += $"\n\tfill({(int)(fillColour.r * 255)});";
                        }
                        else if (noTransparency)
                        {
                            code += $"\n\tfill({(int)(fillColour.r * 255)}, {(int)(fillColour.g * 255)}, {(int)(fillColour.b * 255)});";
                        }
                        else
                        {
                            code += $"\n\tfill({(int)(fillColour.r * 255)}, {(int)(fillColour.g * 255)}, {(int)(fillColour.b * 255)}, {(int)(fillColour.a * 255)});";
                        }
                    }
                }
                code += "\n\t" + shape.GetCode(makePositionVariable ? VariableName : "");
            }
            else if (layers[i] is DesignLayer designLayer)
            {
                code += "\n" + designLayer.design.GetCode(makePositionVariable);
            }
        }
        return code + "\n";
    }
    public const string FOLDER_PATH = "/Designs/";
    // returns the path of a design file based on its name
    public static string PathFromName(string name)
    {
        return FOLDER_PATH + name + ".json";
    }

    // Returns true if the given object is a Design with the same path as this design
    public override bool Equals(object obj)
    {
        Design design = obj as Design;
        return path == design.path;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
