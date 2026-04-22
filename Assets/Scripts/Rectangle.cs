using UnityEngine;

public class Rectangle : Shape
{
    // Top left point is always the first point
    public Vector2 TopLeft => GetGlobalPoint(0);
    public override Vector2 Center
    {
        get
        {
            return new Vector2(TopLeft.x + Scale.x/2, TopLeft.y + Scale.y/2);
        }
        set
        {
            SetLocalPoint(0, value - Scale / 2);
        }
    }
    public Rectangle(Vector2 topLeft, Vector2 size) : base()
    {
        AddLocalPoint(topLeft);
        Scale = size;
    }

    public override string GetCode(string varName = "")
    {
        // If varName is empty, we don't add the variable name
        if(varName == string.Empty)
            return $"rect({TopLeft.x}, {-TopLeft.y}, {Scale.x}, {-Scale.y});";
        else
        {
            Vector2 localTopLeft = GetLocalPoint(0);
            return $"rect({varName}X + {localTopLeft.x}, {varName}Y + {-localTopLeft.y}, {Scale.x}, {-Scale.y});";
        }
    }
    public override string ToString()
    {
        return "Rectangle";
    }
}
