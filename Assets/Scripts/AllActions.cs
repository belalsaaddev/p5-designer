using System.Collections.Generic;
using UnityEngine;

// This file contains all the different types of actions that can be taken in the design editor,
// and the code to undo and redo those actions.

public interface IAction
{
    public void Undo();
    public void Redo();
}

// Shape actions
public class AddShapeAction : IAction
{
    protected Shape shape;
    protected int shapeIndex;
    protected GameObject renderer;
    public AddShapeAction(Shape shape, int shapeIndex, GameObject renderer)
    {
        this.shape = shape;
        this.shapeIndex = shapeIndex;
        this.renderer = renderer;
    }
    public virtual void Undo()
    {
        CanvasManager.RemoveLayer(shapeIndex);
    }
    public virtual void Redo()
    {
        renderer = ShapeRenderer.InstantiateRenderer(shape);
        int currentLayer = CanvasManager.CurrentLayerIndex;
        CanvasManager.AddShapeLayer(shape, shapeIndex);
        if (currentLayer >= shapeIndex)
        {
            CanvasManager.ChangeLayer(currentLayer + 1);
        }

        DesignEditor.instance.AddRenderer(renderer.GetComponent<ShapeRenderer>());
    }

    public override string ToString()
    {
        return $"AddShapeAction: {shape.ToString()} at index {shapeIndex}";
    }
}
public class RemoveShapeAction : AddShapeAction
{
    public RemoveShapeAction(Shape shape, int shapeIndex, GameObject renderer) : base(shape, shapeIndex, renderer)
    {

    }
    public override void Undo()
    {
        base.Redo();
    }
    public override void Redo()
    {
        base.Undo();
    }
    public override string ToString()
    {
        return $"RemoveShapeAction: {shape.ToString()} at index {shapeIndex}";
    }
}
public class ScaleShapeAction : IAction
{
    protected Shape shape;
    Vector2 from;
    Vector2 to;
    public ScaleShapeAction(Shape shape, Vector2 from, Vector2 to)
    {
        this.shape = shape;
        this.from = from;
        this.to = to;
    }
    public void Undo()
    {
        shape.SetScale(from);
    }
    public void Redo()
    {
        shape.SetScale(to);
    }
    public override string ToString()
    {
        return $"ScaleShapeAction: {shape.ToString()} from {from} to {to}";
    }
}
public class MoveShapeAction : IAction
{
    Shape shape;
    List<Vector2> from;
    List<Vector2> to;
    public MoveShapeAction(Shape shape, List<Vector2> from, List<Vector2> to)
    {
        this.shape = shape;
        this.from = from;
        this.to = to;
    }
    public void Undo()
    {
        for (int i = 0; i < from.Count; i++)
        {
            shape.SetLocalPoint(i, from[i]);
        }
    }
    public void Redo()
    {
        for (int i = 0; i < to.Count; i++)
        {
            shape.SetLocalPoint(i, to[i]);
        }
    }
    public override string ToString()
    {
        return $"MoveShapeAction: {shape.ToString()} from {from} to {to}";
    }
}
public class MoveAndScaleShapeAction : IAction
{
    Shape shape;
    ScaleShapeAction scaleAction;
    MoveShapeAction moveAction;
    public MoveAndScaleShapeAction(Shape shape, ScaleShapeAction scaleAction, MoveShapeAction moveAction)
    {
        this.shape = shape;
        this.scaleAction = scaleAction;
        this.moveAction = moveAction;
    }
    public MoveAndScaleShapeAction(Shape shape, List<Vector2> moveFrom, List<Vector2> moveTo, Vector2 scaleFrom, Vector2 scaleTo)
    {
        this.shape = shape;
        scaleAction = new ScaleShapeAction(shape, scaleFrom, scaleTo);
        moveAction = new MoveShapeAction(shape, moveFrom, moveTo);
    }
    public void Undo()
    {
        scaleAction.Undo();
        moveAction.Undo();
    }
    public void Redo()
    {
        scaleAction.Redo();
        moveAction.Redo();
    }

    public override string ToString()
    {
        return $"MoveAndScaleShapeAction: {shape.ToString()}";
    }
}
public class ShapeFillAction : IAction
{
    public Shape shape;
    public Color from;
    public Color to;

    public ShapeFillAction(Shape shape, Color from, Color to)
    {
        this.shape = shape;
        this.from = from;
        this.to = to;
    }
    public void Undo()
    {
        shape.Fill(from);
    }
    public void Redo()
    {
        shape.Fill(to);
    }
    public override string ToString()
    {
        return $"ShapeFillAction: {shape.ToString()} from {from} to {to}";
    }
}
public class ShapeNoFillAction : IAction
{
    public Shape shape;
    public bool newNoFill;
    public Color shapeColor;

    public ShapeNoFillAction(Shape shape, bool newNoFill, Color shapeColor)
    {
        this.shape = shape;
        this.newNoFill = newNoFill;
        this.shapeColor = shapeColor;
    }
    public void Undo()
    {
        if (newNoFill)
        {
            shape.Fill(shapeColor);
        }
        else shape.NoFillShape();
    }
    public void Redo()
    {
        if (newNoFill)
        {
            shape.NoFillShape();
        }
        else shape.Fill(shapeColor);
    }
    public override string ToString()
    {
        return $"ShapeNoFillAction: {shape.ToString()} from {!shape.NoFill} to {shape.NoFill}";
    }
}
public class ShapeStrokeAction : IAction
{
    public Shape shape;
    public Color fromColor;
    public Color toColor;
    public float fromWeight;
    public float toWeight;

    public ShapeStrokeAction(Shape shape, Color fromColor, Color toColor, float fromWeight, float toWeight)
    {
        this.shape = shape;
        this.fromColor = fromColor;
        this.toColor = toColor;
        this.fromWeight = fromWeight;
        this.toWeight = toWeight;
    }
    public void Undo()
    {
        shape.Stroke(fromColor, fromWeight);
    }
    public void Redo()
    {
        shape.Stroke(toColor, toWeight);
    }
    public override string ToString()
    {
        return $"ShapeStrokeAction: {shape.ToString()} from color {fromColor} and weight {fromWeight}" +
            $" to color {toColor} and weight {toWeight}";
    }
}
public class ShapeClosedAction : IAction
{
    public Shape shape;
    public bool newIsClosed;
    public ShapeClosedAction(Shape shape, bool newIsClosed)
    {
        this.shape = shape;
        this.newIsClosed = newIsClosed;
    }
    public void Undo()
    {
        if (newIsClosed)
        {
            shape.OpenShape();
        }
        else shape.CloseShape();
    }
    public void Redo()
    {
        if (newIsClosed)
        {
            shape.CloseShape();
        }
        else shape.OpenShape();
    }
    public override string ToString()
    {
        return $"ShapeClosedAction: {shape.ToString()} from {!shape.IsClosed} to {shape.IsClosed}";
    }
}
public class ApplySettingsAction : IAction
{
    List<IAction> actions = new List<IAction>();
    public void AddSettingChangeAction(IAction action)
    {
        actions.Add(action);
    }
    public void Undo()
    {
        foreach (IAction action in actions)
        {
            action.Undo();
        }
    }
    public void Redo()
    {
        foreach (IAction action in actions)
        {
            action.Redo();
        }
    }
    public override string ToString()
    {
        return $"ApplySettingsAction: {actions.Count} changes";
    }
}

// Design actions
public class AddDesignAction : IAction
{
    Design design;
    int layerIndex;
    public AddDesignAction(Design design, int layerIndex)
    {
        this.design = design;
        this.layerIndex = layerIndex;
    }
    public void Undo()
    {
        CanvasManager.RemoveLayer(layerIndex);
    }
    public void Redo()
    {
        CanvasManager.ImportDesign(design, layerIndex);
    }
    public override string ToString()
    {
        return "AddDesignAction: " + design.Name + " at index " + layerIndex;
    }
}
public class RemoveDesignAction : IAction
{
    Design design;
    int layerIndex;
    public RemoveDesignAction(Design design, int layerIndex)
    {
        this.design = design;
        this.layerIndex = layerIndex;
    }
    public void Undo()
    {
        CanvasManager.ImportDesign(design, layerIndex);
    }
    public void Redo()
    {
        CanvasManager.RemoveLayer(layerIndex);
    }
    public override string ToString()
    {
        return "RemoveDesignAction: " + design.Name + " at index " + layerIndex;
    }
}
public class MoveDesignAction : IAction
{
    Design design;
    Vector2 from;
    Vector2 to;
    public MoveDesignAction(Design design, Vector2 from, Vector2 to)
    {
        this.design = design;
        this.from = from;
        this.to = to;
    }
    public void Undo()
    {
        design.LocalPosition = from;
    }
    public void Redo()
    {
        design.LocalPosition = to;
    }
    public override string ToString()
    {
        return $"MoveDesignAction: {design.Name} from {from} to {to}";
    }
}

// Other actions
public class AddLayerGroupAction : IAction
{
    int startIndex;
    ILayer[] layers;

    public AddLayerGroupAction(int startIndex, ILayer[] layers)
    {
        this.startIndex = startIndex;
        this.layers = layers;
    }

    public void Undo()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            CanvasManager.RemoveLayer(startIndex);
        }
    }
    public void Redo()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] is Design.ShapeLayer shapeLayer)
            {
                CanvasManager.AddShapeLayer(shapeLayer.shape, startIndex + i);
            }
            else if (layers[i] is Design.DesignLayer designLayer)
            {
                CanvasManager.ImportDesign(designLayer.design, startIndex + i);
            }
        }
    }
    public override string ToString()
    {
        return $"AddLayerGroupAction: {layers.Length} layers starting at index {startIndex}";
    }
}
public class MoveLayerAction : IAction
{
    protected int from;
    protected int to;

    public MoveLayerAction(int from, int to)
    {
        this.from = from;
        this.to = to;
    }

    public void Undo()
    {
        CanvasManager.MoveLayer(to, from);
    }
    public void Redo()
    {
        CanvasManager.MoveLayer(from, to);
    }
    public override string ToString()
    {
        return $"MoveLayerAction: from {from} to {to}";
    }
}
