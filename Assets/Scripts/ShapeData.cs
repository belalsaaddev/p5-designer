using UnityEngine;
using Vector2 = UnityEngine.Vector2;
[System.Serializable]
public class ShapeData
{
    public string type;
    public Vector2[] points;
    public Vector2 size;
    public float strokeWeight;
    public Color strokeColor;
    public bool noFill;
    public Color fillColor;
    public bool isClosed;
    public ShapeData(Shape shape)
    {
        type = shape.ToString();
        points = shape.LocalPoints.ToArray();
        size = shape.Scale;
        strokeWeight = shape.StrokeWeight;
        strokeColor = shape.StrokeColor;
        noFill = shape.NoFill;
        fillColor = shape.FillColor;
        isClosed = shape.IsClosed;
    }
}
