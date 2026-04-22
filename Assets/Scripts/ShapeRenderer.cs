using UnityEngine;

public class ShapeRenderer : MonoBehaviour
{
    public bool isDrawing = false;
    //If the shape is being drawn, we want it to appear ontop of all other shapes
    public int ShapeIndex => isDrawing ? 9999 : CanvasManager.GetShapeIndex(shape);
    public Shape shape;
    protected SpriteRenderer spriteRenderer;
    protected LineRenderer lineRenderer;
    protected bool isSelected = false;
    protected virtual void Start()
    {
        CanvasManager.OnLayerChanged += OnLayerChanged;
        CanvasManager.OnLayerRemoved += OnLayerRemoved;
        ActionLogger.OnUndo += OnUndo;
        ActionLogger.OnRedo += OnRedo;
    }
    protected virtual void OnLayerChanged()
    {
        // If this renderer's shape is the current shape, we want to select it
        Shape currentShape = CanvasManager.CurrentShape;
        bool shouldBeSelected = currentShape != null && currentShape == shape;
        if (isSelected && shouldBeSelected) return;
        if(shouldBeSelected)
        {
            Select();
        }
        else
        {
            if (isSelected) Deselect();
        }
    }
    protected virtual void Select()
    {
        isSelected = true;
        UIManager.instance.OnHandleHeld += OnHandleHeld;
        UIManager.instance.OnHandleReleased += OnHandleReleased;
    }
    protected virtual void Deselect()
    {
        isSelected = false;
        UIManager.instance.OnHandleHeld -= OnHandleHeld;
        UIManager.instance.OnHandleReleased -= OnHandleReleased;
    }
    protected virtual void OnHandleHeld(HandleUI handle)
    {
        
    }
    protected virtual void OnHandleReleased(HandleUI handle)
    {
        UpdateAllHandlePositions();
    }
    protected virtual void UpdateAllHandlePositions()
    {
        if (!isSelected) return;
    }
    protected void OnLayerRemoved(int index, ILayer layer)
    {
        // If the index is -1, that means that this shape was not found in the design,
        // hence we destroy its renderer
        if(ShapeIndex == -1)
        {
            DestroyRenderer();
            return;
        }
    }
    protected virtual void OnUndo()
    {
        UpdateAllHandlePositions();
    }
    protected virtual void OnRedo()
    {
        UpdateAllHandlePositions();
        OnLayerChanged();
    }
    // This method should be overridden by subclasses to render the shape
    public virtual void Render()
    {
        // Setting sprite renderer properties for fill
        if (spriteRenderer)
        {
            if(shape.NoFill) spriteRenderer.color = new Color(0, 0, 0, 0);
            else spriteRenderer.color = shape.FillColor;

            spriteRenderer.sortingOrder = ShapeIndex * 2;
        }
        
        //Setting line renderer properties for stroke
        if(lineRenderer)
        {
            lineRenderer.enabled = !shape.NoStroke;
            if (!shape.NoStroke)
            {
                lineRenderer.startColor = shape.StrokeColor;
                lineRenderer.endColor = shape.StrokeColor;
                lineRenderer.sortingOrder = ShapeIndex * 2 + 1;
                lineRenderer.startWidth = shape.StrokeWeight;
                lineRenderer.endWidth = shape.StrokeWeight;
            }
        }
    }
    public virtual void Setup()
    {
        // This method should be overridden by subclasses to set up the renderer
    }

    // Creates a renderer for a given shape
    public static GameObject InstantiateRenderer(Shape shape)
    {
        GameObject rendererObj = new GameObject();
        ShapeRenderer renderer = null;
        if (shape is Rectangle)
        {
            rendererObj.name = "Rectangle";
            RectangleRenderer rectRenderer = rendererObj.AddComponent<RectangleRenderer>();
            rectRenderer.shape = shape;
            renderer = rectRenderer;
        }
        else if(shape is Ellipse)
        {
            rendererObj.name = "Ellipse";
            EllipseRenderer ellipseRenderer = rendererObj.AddComponent<EllipseRenderer>();
            ellipseRenderer.shape = shape;
            renderer = ellipseRenderer;
        }

        if (shape is CustomShape)
        {
            rendererObj.name = "Custom Shape";
            CustomShapeRenderer csRenderer = rendererObj.AddComponent<CustomShapeRenderer>();
            csRenderer.shape = shape;
            renderer = csRenderer;
        }
        else
        {
            renderer.spriteRenderer = rendererObj.AddComponent<SpriteRenderer>();
        }

        renderer.lineRenderer = rendererObj.AddComponent<LineRenderer>();
        renderer.lineRenderer.material = DesignEditor.instance.strokeMaterial;
        
        renderer.Setup();
        return rendererObj;
    }
    public void DestroyRenderer()
    {
        if(isSelected) Deselect();
        DesignEditor.instance.RemoveRenderer(this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        CanvasManager.OnLayerChanged -= OnLayerChanged;
        CanvasManager.OnLayerRemoved -= OnLayerRemoved;
        ActionLogger.OnUndo -= OnUndo;
        ActionLogger.OnRedo -= OnRedo;
    }
}
