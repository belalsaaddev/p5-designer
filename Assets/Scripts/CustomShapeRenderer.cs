using PowerfistTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CustomShapeRenderer : ShapeRenderer
{
    private CustomShape customShape;
    private HandleUI[] handles;
    private SpriteShapeController shapeController;
    private SpriteShapeRenderer spriteShapeRenderer;
    protected override void Start()
    {
        base.Start();
        spriteShapeRenderer = gameObject.AddComponent<SpriteShapeRenderer>();
        shapeController = gameObject.AddComponent<SpriteShapeController>();
        shapeController.spriteShape = DesignEditor.instance.shapeProfile;
        if (customShape == null) Setup();
    }
    public override void Setup()
    {
        customShape = (CustomShape)shape;
        transform.position = Vector3.zero;
    }
    public override void Render()
    {
        base.Render();
        if (!shapeController) return;

        spriteShapeRenderer.enabled = !shape.NoFill;

        Spline shapeSpline = shapeController.spline;
        if (shape.PointCount > 2 && !shape.NoFill)
        {
            //Render Shape
            spriteShapeRenderer.color = customShape.FillColor;
            spriteShapeRenderer.sortingOrder = ShapeIndex * 2;
            
            if (shapeSpline.GetPointCount() != customShape.PointCount) SetupPoints();
            for (int i = 0; i < customShape.PointCount; i++)
            {
                shapeSpline.SetPosition(i, customShape.GetGlobalPoint(i));
            }
        }
        else
        {
            if (shapeSpline.GetPointCount() > 0) shapeSpline.Clear();
        }

        //Render Stroke
        Vector3[] strokePoints = new Vector3[customShape.PointCount];
        for (int i = 0; i < strokePoints.Length; i++)
        {
            strokePoints[i] = customShape.GetGlobalPoint(i);
        }
        lineRenderer.positionCount = customShape.PointCount;
        lineRenderer.loop = customShape.IsClosed;
        lineRenderer.SetPositions(strokePoints);
    }
    void SetupPoints()
    {
        Spline shapeSpline = shapeController.spline;
        shapeController.splineDetail = 2;
        shapeSpline.Clear();
        for (int i = 0; i < customShape.PointCount; i++)
        {
            shapeSpline.InsertPointAt(i, customShape.GetGlobalPoint(i));
        }
    }

    protected override void Select()
    {
        base.Select();
        handles = UIManager.instance.GetHandles(customShape.PointCount + 1);

        handles[0].Setup(customShape.Center, HandleUI.MovementDirection.Bidirectional);

        for (int i = 1; i < handles.Length; i++)
        {
            handles[i].Setup(customShape.GetGlobalPoint(i - 1), HandleUI.MovementDirection.Bidirectional);
        }
    }
    protected override void UpdateAllHandlePositions()
    {
        if (!isSelected) return;
        base.UpdateAllHandlePositions();
        UpdateMoveHandlePosition();
        UpdatePointHandlePositions();
    }
    void UpdateMoveHandlePosition()
    {
        handles[0].SetPosition(customShape.Center);
    }
    void UpdatePointHandlePositions() {
        for (int i = 1; i < handles.Length; i++)
        {
            handles[i].SetPosition(customShape.GetGlobalPoint(i - 1));
        }
    }
    protected override void OnHandleHeld(HandleUI handle)
    {
        base.OnHandleHeld(handle);
        if(handle == handles[0]) // Move entire shape
        {
            StartCoroutine(MoveShape());
        }
        else // Drag a single point
        {
            // Finding the index of the handle that is being dragged
            int handleIndex = -1;
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i] == handle)
                {
                    handleIndex = i;
                    break;
                }
            }
            if(handleIndex == -1)
            {
                Debug.LogError("Missing handle UI for point");
                return;
            }
            StartCoroutine(MovePoint(handleIndex - 1, handles[handleIndex]));
        }
    }
    IEnumerator MoveShape()
    {
        HandleUI handle = handles[0];
        // Storing the start positions of all points in a seperate list instance
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < customShape.PointCount; i++)
        {
            startPositions.Add(customShape.GetGlobalPoint(i));
        }

        // We offset all points by the difference between the last and the current positions of the handle
        Vector2 lastPos = ((Vector2)handle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
        while (handle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)handle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - lastPos;

            for (int i = 0; i < customShape.PointCount; i++)
            {
                customShape.SetLocalPoint(i, customShape.GetGlobalPoint(i) + difference);
            }
            UpdateAllHandlePositions();

            lastPos = currentPos;
            yield return new WaitForEndOfFrame();
        }

        // Storing the end positions of all points in a seperate list instance
        List<Vector2> endPositions = new List<Vector2>();
        for (int i = 0; i < customShape.PointCount; i++)
        {
            endPositions.Add(customShape.GetGlobalPoint(i));
        }

        // Logging move action
        MoveShapeAction moveAction = new MoveShapeAction(customShape,
            startPositions,
            endPositions);
        ActionLogger.LogAction(moveAction);
    }
    IEnumerator MovePoint(int pointIndex, HandleUI handle)
    {
        // Storing the start positions of all points in a seperate list instance
        List<Vector2> startPositions = new List<Vector2>();
        for (int i = 0; i < customShape.PointCount; i++)
        {
            startPositions.Add(customShape.GetGlobalPoint(i));
        }

        // We update the position of the dragged point to the position of the handle
        while (handle.IsHeld)
        {
            Vector2 handlePos = ((Vector2)handle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            customShape.SetLocalPoint(pointIndex, handlePos);
            yield return new WaitForEndOfFrame();
        }

        // Storing the end positions of all points in a seperate list instance
        List<Vector2> endPositions = new List<Vector2>();
        for (int i = 0; i < customShape.PointCount; i++)
        {
            endPositions.Add(customShape.GetGlobalPoint(i));
        }

        // Logging move action
        MoveShapeAction moveAction = new MoveShapeAction(customShape,
            startPositions,
            endPositions);
        ActionLogger.LogAction(moveAction);
    }
}
