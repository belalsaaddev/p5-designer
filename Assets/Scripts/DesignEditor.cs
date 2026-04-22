using PowerfistTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class DesignEditor : MonoBehaviour
{
    public static DesignEditor instance;
    public SpriteShape shapeProfile;
    public Material strokeMaterial;
    public Sprite rectangleSprite;
    public Sprite ellipseSprite;
    public bool IsDrawing { get; private set; }
    private readonly List<ShapeRenderer> renderers = new();
    Shape CurrentShape => CanvasManager.CurrentShape;
    // The current fill color. If we aren't on a design layer, it will return a random color.
    public Color CurrentFill { 
        get
        {
            if (CanvasManager.LayerCount == 0 || CurrentShape == null)
                return Random.ColorHSV();
            else
                return CurrentShape.FillColor;
        }
    }
    // The current stroke color. If we aren't on a design layer, it will return black.
    public Color CurrentStroke
    {
        get
        {
            if (CanvasManager.LayerCount == 0 || CurrentShape == null)
                return Color.black;
            else
                return CurrentShape.StrokeColor;
        }
    }
    // The current stroke weight. If we aren't on a design layer, it will return a default value.
    public float CurrentStrokeWeight
    {
        get
        {
            if (CanvasManager.LayerCount == 0 || CurrentNoFill || CurrentShape == null)
                return 2f;
            else
                return CurrentShape.StrokeWeight;
        }
    }
    // Whether the current shape has no fill. If we aren't on a design layer, it will return false.
    public bool CurrentNoFill
    {
        get
        {
            if (CanvasManager.LayerCount == 0 || CurrentShape == null) return false;
            else return CurrentShape.NoFill;
        }
    }
    Vector2 DEFAULT_SCALE => Vector2.one * 10;
    int GRID_SIZE => CanvasManager.GRID_SIZE;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        if(CanvasManager.CurrentDesign != null)
        {
            ImportDesign(CanvasManager.CurrentDesign);
        }
        CanvasManager.OnLayerAdded += OnLayerAdded;
    }

    void Update()
    {
        // Ctrl + Z for undo, Ctrl + Shift + Z for redo
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Z))
        {
            Redo();
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }

        // Ctrl + S for save
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            CanvasManager.Save();
            Debug.Log("Saving");
        }

        // Delete key to delete current shape
        if (CanvasManager.LayerCount > 0 && Input.GetKeyDown(KeyCode.Delete))
        {
            // Storing layer to log action after it is removed
            ILayer currentLayer = CanvasManager.CurrentDesign.layers[CanvasManager.CurrentLayerIndex];
            CanvasManager.RemoveLayer(CanvasManager.CurrentLayerIndex);

            // Logging action
            if (currentLayer is Design.ShapeLayer shapeLayer)
            {
                RemoveShapeAction action = new RemoveShapeAction(shapeLayer.shape,
                    CanvasManager.CurrentLayerIndex,
                    FindShapeRendererFromShape(shapeLayer.shape).gameObject);

                ActionLogger.LogAction(action);
            }
        }

        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].Render();
        }
    }
    // Creates a shape of the given type
    public void CreateShape(ShapeType shapeType)
    {
        if (IsDrawing) return;
        IsDrawing = true;
        UIManager.instance.HideToolBar();
        switch (shapeType)
        {
            case ShapeType.Rectangle:
                StartCoroutine(DrawRect());
                break;
            case ShapeType.Ellipse:
                StartCoroutine(DrawEllipse());
                break;
            case ShapeType.Triangle:
                UIManager.instance.ShowDrawShapeMsg("Select 3 points");
                StartCoroutine(DrawCustomShape(3));
                break;
            case ShapeType.Line:
                UIManager.instance.ShowDrawShapeMsg("Select 2 points");
                StartCoroutine(DrawCustomShape(2));
                break;
            case ShapeType.Custom:
                UIManager.instance.ShowDrawShapeMsg("Select points, then press escape or backspace, or click on the start point");
                StartCoroutine(DrawCustomShape());
                break;
        }
    }
    // Draws a rectangle by clicking and dragging
    IEnumerator DrawRect()
    {
        UIManager.instance.ShowDrawShapeMsg();
        // Waiting a bit to avoid immediately clicking and starting the rectangle on the same frame as the button click
        yield return new WaitForFixedUpdate();
        // Waiting until the user clicks to start drawing
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        UIManager.instance.HideDrawShapeMsg();

        Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = startPos.SnapToGrid(GRID_SIZE);
        // Creating the rectangle with a default scale
        Rectangle rect = new Rectangle(startPos, DEFAULT_SCALE);
        rect.parent = CanvasManager.CurrentDesign;
        // Setting fill
        if (CurrentNoFill) rect.NoFillShape();
        else rect.Fill(CurrentFill);
        // Setting stroke
        rect.Stroke(CurrentStroke, CurrentStrokeWeight);
        // Creating renderer for the shape
        RectangleRenderer rectRenderer =
            ShapeRenderer.InstantiateRenderer(rect).GetComponent<RectangleRenderer>();
        rectRenderer.isDrawing = true;

        yield return new WaitForFixedUpdate();
        // As user drags the mouse, update the scale of the rectangle
        while (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPos = currentPos.SnapToGrid(GRID_SIZE);
            rect.SetScale(currentPos - startPos);
            rectRenderer.Render();
            yield return new WaitForEndOfFrame();
        }

        AddShape(rect, rectRenderer);
        Debug.Log($"Created rect at {startPos} with scale {rect.Scale}");
        UIManager.instance.ShowToolBar();
        IsDrawing = false;
    }
    // Draws an ellipse by clicking and dragging
    IEnumerator DrawEllipse()
    {
        UIManager.instance.ShowDrawShapeMsg();
        // Waiting a bit to avoid immediately clicking and starting the ellipse on the same frame as the button click
        yield return new WaitForFixedUpdate();
        // Waiting until the user clicks to start drawing
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        UIManager.instance.HideDrawShapeMsg();

        Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = startPos.SnapToGrid(GRID_SIZE);
        // Creating the ellipse with a default scale
        Ellipse ellipse = new Ellipse(startPos, DEFAULT_SCALE);
        ellipse.parent = CanvasManager.CurrentDesign;
        // Setting fill
        if (CurrentNoFill) ellipse.NoFillShape();
        else ellipse.Fill(CurrentFill);
        // Setting stroke
        ellipse.Stroke(CurrentStroke, CurrentStrokeWeight);
        // Creating renderer for the shape
        EllipseRenderer ellipseRenderer =
            ShapeRenderer.InstantiateRenderer(ellipse).GetComponent<EllipseRenderer>();
        ellipseRenderer.isDrawing = true;

        yield return new WaitForFixedUpdate();
        // As user drags the mouse, update the scale of the ellipse
        while (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPos = currentPos.SnapToGrid(GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            // Making sure the scale is always positive
            if (difference.x < 0) difference.x*=-1;
            if (difference.y < 0) difference.y *= -1;
            // Multiplying by 2 to convert radius to diameter
            ellipse.SetScale(difference * 2f);
            ellipseRenderer.Render();
            yield return new WaitForEndOfFrame();
        }

        AddShape(ellipse, ellipseRenderer);
        Debug.Log($"Created ellipse at {startPos} with scale {ellipse.Scale}");
        UIManager.instance.ShowToolBar();
        IsDrawing = false;
    }
    // Draws a custom shape by clicking to place points
    IEnumerator DrawCustomShape(int maxPoints = 99)
    {
        // If maxPoints is smaller than or equal to 3, we close the shape, since it is definitely a triangle or a line.
        bool isOpenShape = maxPoints > 3;
        // Waiting a bit to avoid immediately clicking and starting the shape on the same frame as the button click
        yield return new WaitForFixedUpdate();
        // Waiting until the user clicks to start drawing
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        Vector2 startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = startPos.SnapToGrid(GRID_SIZE);
        // Creating the custom shape
        CustomShape shape = new CustomShape(new List<Vector2> { startPos });
        shape.parent = CanvasManager.CurrentDesign;
        // Setting fill
        if (CurrentNoFill) shape.NoFillShape();
        else shape.Fill(CurrentFill);
        // Setting stroke
        shape.Stroke(CurrentStroke, CurrentStrokeWeight);
        // Opening or closing shape
        if (isOpenShape) shape.OpenShape();
        else shape.CloseShape();
        // Creating renderer for the shape
        CustomShapeRenderer renderer =
            ShapeRenderer.InstantiateRenderer(shape).GetComponent<CustomShapeRenderer>();
        renderer.isDrawing = true;

        // As user clicks, we add points to the shape
        for (int i = 1; i < maxPoints; i++)
        {
            yield return new WaitForFixedUpdate();
            bool stopDrawing = false;
            // Checking for input to close the shape or add a new point
            while (true)
            {
                if (Input.GetMouseButtonDown(0)) break;
                if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) && isOpenShape)
                {
                    stopDrawing = true;
                    break;
                }
                yield return null;
            }
            if (stopDrawing) break;

            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPos = currentPos.SnapToGrid(GRID_SIZE);

            const float CLOSE_THRESHOLD = 5f;
            // If we click near the start point, we close the shape and stop drawing
            // or if its not an open shape, we ignore this point
            if (Vector2.Distance(currentPos, startPos) <= CLOSE_THRESHOLD)
            {
                if (isOpenShape)
                {
                    shape.CloseShape();
                    break;
                }
                else
                {
                    i--;
                    continue;
                }
            }
            else
            {
                shape.AddLocalPoint(currentPos);
            }

            renderer.Render();
        }

        AddShape(shape, renderer);
        Debug.Log($"Created custom shape at {shape.Center} with {shape.PointCount} points");
        UIManager.instance.HideDrawShapeMsg();
        UIManager.instance.ShowToolBar();
        IsDrawing = false;
    }
    void AddShape(Shape shape, ShapeRenderer renderer)
    {
        Design design = CanvasManager.CurrentDesign;
        renderer.isDrawing = false;
        AddRenderer(renderer);
        CanvasManager.AddShapeLayer(shape);

        ActionLogger.LogAction(new AddShapeAction(shape, CanvasManager.LayerCount - 1, renderer.gameObject));
    }

    public ShapeRenderer FindShapeRendererFromShape(Shape shape)
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].shape == shape) return renderers[i];
        }
        return null;
    }
    public void AddRenderer(ShapeRenderer renderer, int index = -1)
    {
        if(index >= 0 && index < renderers.Count)
        {
            renderers.Insert(index, renderer);
        }
        else
        {
            renderers.Add(renderer);
        }
    }
    public void RemoveRenderer(ShapeRenderer renderer)
    {
        renderers.Remove(renderer);
    }
    public ShapeRenderer GetRenderer(int index)
    {
        if (index < 0 || index >= renderers.Count) return null;
        return renderers[index];
    }

    // If a layer is added, we want to make sure it has its shape renderers
    void OnLayerAdded(int index, ILayer layer)
    {
        if(layer is Design.DesignLayer designLayer)
        {
            ImportDesign(designLayer.design);
        }
        else if(layer is Design.ShapeLayer shapeLayer && FindShapeRendererFromShape(shapeLayer.shape) == null)
        {
            ShapeRenderer renderer =
                ShapeRenderer.InstantiateRenderer(shapeLayer.shape).GetComponent<ShapeRenderer>();
            renderer.isDrawing = false;
            renderers.Add(renderer);
        }
    }
    // Creates shape renderers for all imported design's shapes and creates a mover object for it
    public void ImportDesign(Design design)
    {
        Debug.Log("Importing " + design.layers.Count + " layers from " + design.Name);
        for (int i = 0; i < design.layers.Count; i++)
        {
            if (design.layers[i] is Design.ShapeLayer shapeLayer)
            {
                ShapeRenderer renderer =
                ShapeRenderer.InstantiateRenderer(shapeLayer.shape).GetComponent<ShapeRenderer>();
                renderer.isDrawing = false;
                renderers.Add(renderer);
            }
            else if (design.layers[i] is Design.DesignLayer designLayer)
            {
                ImportDesign(designLayer.design);
            }
        }
        if(design != CanvasManager.CurrentDesign)
        {
            DesignMover.InstantiateMover(design);
        }
    }

    private void OnDestroy()
    {
        CanvasManager.OnLayerAdded -= OnLayerAdded;
    }

    public void Undo()
    {
        ActionLogger.Undo();
    }
    public void Redo()
    {
        ActionLogger.Redo();
    }

    public void MainMenu()
    {
        CanvasManager.Save();
        SceneManager.LoadScene(0);
    }
    public enum ShapeType
    {
        Rectangle,
        Ellipse,
        Triangle,
        Line,
        Custom
    }
}
