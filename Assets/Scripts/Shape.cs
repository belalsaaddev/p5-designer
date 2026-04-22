using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Shape
{
    private readonly List<Vector2> points;
    // Returns a list of all points relative to their parent's position
    public List<Vector2> LocalPoints
    {
        get
        {
            // Creating a new list instance so that the current one isn't modified
            List<Vector2> localPoints = new List<Vector2>();
            foreach (Vector2 point in points)
            {
                localPoints.Add(point);
            }
            return localPoints;
        }
    }
    public int PointCount => points.Count;
    public virtual Vector2 Center
    {
        get
        {
            Vector2 sum = Vector2.zero;
            for (int i = 0; i < PointCount; i++)
            {
                sum += GetGlobalPoint(i);
            }
            return sum / PointCount;
        }
        set
        {
            Vector2 difference = value - Center;
            for (int i = 0; i < points.Count; i++)
            {
                points[i] += difference;
            }
        }
    }
    public Vector2 Scale { get; protected set; } = Vector2.one;
    public float StrokeWeight { get; private set; }
    public Color StrokeColor { get; private set; }
    public Color FillColor { get; private set; }
    public bool IsClosed { get; protected set; } = true;
    public bool CanOpen => this is CustomShape;
    public bool NoFill { get; protected set; }
    public bool NoStroke => StrokeWeight == 0;
    public Design parent;
    public Shape()
    {
        points = new List<Vector2>();
        FillColor = Color.white;
        StrokeColor = Color.black;
        StrokeWeight = 0;
        IsClosed = true;
        NoFill = false;
    }
    public static Shape ShapeFromData(ShapeData data, Design parent)
    {
        Shape shape = null;
        switch(data.type)
        {
            case "Rectangle":
                Rectangle rect = new Rectangle(data.points[0], data.size);
                shape = rect;
                break;
            case "Ellipse":
                Ellipse ellipse = new Ellipse(data.points[0], data.size);
                shape = ellipse;
                break;
            default:
                CustomShape customShape = new CustomShape(data.points.ToList());
                shape = customShape;
                break;
        }

        shape.FillColor = data.fillColor;
        shape.StrokeColor = data.strokeColor;
        shape.StrokeWeight = data.strokeWeight;
        shape.IsClosed = data.isClosed;
        shape.NoFill = data.noFill;
        shape.parent = parent;

        return shape;
    }
    public void Fill(Color color)
    {
        FillColor = color;
        NoFill = false;
    }
    public void NoFillShape()
    {
        NoFill = true;
    }
    public void Stroke(Color color, float weight)
    {
        StrokeColor = color;
        StrokeWeight = weight;
    }
    public void NoStrokeShape()
    {
        StrokeWeight = 0;
    }
    public void CloseShape()
    {
        IsClosed = true;
    }
    public void OpenShape()
    {
        // Shapes can only be opened if they are a custom shape
        if (this is CustomShape) IsClosed = false;
    }

    public Vector2 GetLocalPoint(int index)
    {
        return points[index];
    }
    public void SetLocalPoint(int index, Vector2 value)
    {
        if (index >= 0 && index < points.Count)
        {
            points[index] = value;
        }
    }
    public void AddLocalPoint(Vector2 point)
    {
        points.Add(point);
    }

    public Vector2 GetGlobalPoint(int index)
    {
        return points[index] + parent.Position;
    }

    public void SetScale(Vector2 scale)
    {
        if (scale.x == 0 || scale.y == 0) return;
        Scale = scale;
    }
    public string ExtractJson()
    {
        return JsonUtility.ToJson(new ShapeData(this));
    }
    public virtual string GetCode(string anchorVarName = "")
    {
        return "//This Abstract Shape Class Has No Code";
    }
}
