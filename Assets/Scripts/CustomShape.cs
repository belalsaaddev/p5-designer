using System.Collections.Generic;
using UnityEngine;

public class CustomShape : Shape
{
    public CustomShape(List<Vector2> points) : base()
    {
        for (int i = 0; i < points.Count; i++)
        {
            AddLocalPoint(points[i]);
        }
        IsClosed = false;
    }

    public override string GetCode(string varName = "")
    {
        bool noVarName = varName == string.Empty;
        // Line if 2 points
        if (PointCount == 2)
        {
            Vector2 pointA = noVarName ? GetGlobalPoint(0) : GetLocalPoint(0);
            Vector2 pointB = noVarName ? GetGlobalPoint(1) : GetLocalPoint(1);
            if (noVarName)
            {
                return $"line({pointA.x}, {-pointA.y}," +
                    $" {pointB.x}, {-pointB.y});";
            }
            else
            {
                return $"line({varName}X + {pointA.x}, {varName}Y + {-pointA.y}," +
                    $" {varName}X + {pointB.x}, {varName}Y + {-pointB.y});";
            }
        }

        // Triangle if 3 points
        if(PointCount == 3 && IsClosed)
        {
            Vector2 pointA = noVarName ? GetGlobalPoint(0) : GetLocalPoint(0);
            Vector2 pointB = noVarName ? GetGlobalPoint(1) : GetLocalPoint(1);
            Vector2 pointC = noVarName ? GetGlobalPoint(2) : GetLocalPoint(2);
            if (noVarName)
                return $"triangle({pointA.x}, {-pointA.y}," +
                    $" {pointB.x}, {-pointB.y}," +
                    $" {pointC.x}, {-pointC.y});";
            else
            {
                return $"triangle({varName}X + {pointA.x}, {varName}Y + {-pointA.y}," +
                    $" {varName}X + {pointB.x}, {varName}Y + {-pointB.y}," +
                    $" {varName}X + {pointC.x}, {varName}Y + {-pointC.y});";
            }
        }

        // Custom Shape if more than 3 points
        string code = "";
        code += "beginShape();\n";
        for (int i = 0; i < PointCount; i++)
        {
            Vector2 point = GetGlobalPoint(i);
            if (noVarName)
                code += $"\tvertex({point.x}, {-point.y});";
            else
            {
                point = GetLocalPoint(i);
                code += $"\tvertex({varName}X + {point.x}, {varName}Y + {-point.y});";
            }

            code += '\n';
        }
        code += "\tendShape(" + (IsClosed ? "CLOSE" : "") + ");";
        return code;
    }
    public override string ToString()
    {
        if (PointCount == 2) return "Line";
        if (PointCount == 3 && IsClosed) return "Triangle";
        return "Custom Shape";
    }
}
