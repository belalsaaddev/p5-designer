using UnityEngine;

public class Ellipse : Shape
{
    // Center is always first point
    public override Vector2 Center
    {
        get
        {
            return GetGlobalPoint(0);
        }
        set
        {
            SetLocalPoint(0, value);
        }
    }
    public Ellipse(Vector2 center, Vector2 size) : base()
    {
        AddLocalPoint(center);
        Scale = size;
    }

    public override string GetCode(string varName = "")
    {
        // If varName is empty, then no variable
        if (varName == string.Empty)
            return $"ellipse({Center.x}, {-Center.y}, {Scale.x}, {Scale.y});";
        else
        {
            Vector2 localCenter = GetLocalPoint(0);
            return $"ellipse({varName}X + {localCenter.x}, {varName}Y + {-localCenter.y}, {Scale.x}, {Scale.y});";
        }
    }
    public override string ToString()
    {
        return "Ellipse";
    }
}
