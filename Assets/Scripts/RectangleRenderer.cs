using PowerfistTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectangleRenderer : ShapeRenderer
{
    private Rectangle rect;
    private HandleUI moveHandle;
    private HandleUI upHandle;
    private HandleUI downHandle;
    private HandleUI leftHandle;
    private HandleUI rightHandle;
    protected override void Start()
    {
        base.Start();
        if (rect == null) Setup();
    }
    public override void Setup()
    {
        rect = (Rectangle)shape;
        spriteRenderer.sprite = DesignEditor.instance.rectangleSprite;

        Render();
    }
    public override void Render()
    {
        base.Render();
        transform.position = rect.Center;
        transform.localScale = rect.Scale;

        // Rendering stroke using a line renderer with four points (one for each corner)
        Vector3[] strokePoints = new Vector3[4];
        strokePoints[0] = rect.TopLeft;
        strokePoints[1] = (Vector3)rect.TopLeft + Vector3.right * rect.Scale.x;
        strokePoints[2] = rect.TopLeft + rect.Scale;
        strokePoints[3] = (Vector3)rect.TopLeft - Vector3.down * rect.Scale.y;
        lineRenderer.positionCount = 4;
        lineRenderer.loop = true;
        lineRenderer.SetPositions(strokePoints);
    }
    // Sets up points for scaling in all four directions and moving the entire shape
    protected override void Select()
    {
        base.Select();
        HandleUI[] handles = UIManager.instance.GetHandles(5);

        // Center Movement Handle
        handles[0].Setup(rect.Center, HandleUI.MovementDirection.Bidirectional);
        moveHandle = handles[0];

        // Scale Up Handle
        Vector2 up = rect.TopLeft + new Vector2(rect.Scale.x / 2f, 0f);
        handles[1].Setup(up, HandleUI.MovementDirection.Vertical);
        upHandle = handles[1];

        // Scale Down Handle
        Vector2 down = rect.TopLeft + new Vector2(rect.Scale.x / 2f, rect.Scale.y);
        handles[2].Setup(down, HandleUI.MovementDirection.Vertical);
        downHandle = handles[2];

        // Scale Left Handle
        Vector2 left = rect.TopLeft + new Vector2(0f, rect.Scale.y / 2f);
        handles[3].Setup(left, HandleUI.MovementDirection.Horizontal);
        leftHandle = handles[3];

        // Scale Right Handle
        Vector2 right = rect.TopLeft + new Vector2(rect.Scale.x, rect.Scale.y / 2f);
        handles[4].Setup(right, HandleUI.MovementDirection.Horizontal);
        rightHandle = handles[4];
    }

    // Places all handle UIs at their correct positions
    protected override void UpdateAllHandlePositions()
    {
        if (!isSelected) return;
        base.UpdateAllHandlePositions();
        UpdateMoveHandlePosition();
        UpdateUpHandlePosition();
        UpdateDownHandlePosition();
        UpdateLeftHandlePosition();
        UpdateRightHandlePosition();
    }
    void UpdateMoveHandlePosition()
    {
        moveHandle.SetPosition(rect.Center);
    }
    void UpdateUpHandlePosition()
    {
        Vector2 up = rect.TopLeft + new Vector2(rect.Scale.x / 2f, 0f);
        upHandle.SetPosition(up);
    }
    void UpdateDownHandlePosition()
    {
        Vector2 down = rect.TopLeft + new Vector2(rect.Scale.x / 2f, rect.Scale.y);
        downHandle.SetPosition(down);
    }
    void UpdateLeftHandlePosition()
    {
        Vector2 left = rect.TopLeft + new Vector2(0f, rect.Scale.y / 2f);
        leftHandle.SetPosition(left);
    }
    void UpdateRightHandlePosition()
    {
        Vector2 right = rect.TopLeft + new Vector2(rect.Scale.x, rect.Scale.y / 2f);
        rightHandle.SetPosition(right);
    }
    // When handle is held, we find out which handle it is
    // and transfrom the shape accordingly
    protected override void OnHandleHeld(HandleUI handle)
    {
        base.OnHandleHeld(handle);
        if(handle == moveHandle)
        {
            StartCoroutine(MoveShape());
        }
        else if(handle == upHandle)
        {
            StartCoroutine(ScaleUp());
        }
        else if (handle == downHandle)
        {
            StartCoroutine(ScaleDown());
        }
        else if (handle == leftHandle)
        {
            StartCoroutine(ScaleLeft());
        }
        else if (handle == rightHandle)
        {
            StartCoroutine(ScaleRight());
        }
    }
    // Moves shape from center point
    IEnumerator MoveShape()
    {
        Vector2 startPos = rect.TopLeft;
        Vector2 lastPos = ((Vector2)moveHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
        // While handle is held we offset the center of the rectangle
        // by the difference between the previous and current handle positions
        while (moveHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)moveHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - lastPos;
            rect.SetLocalPoint(0, rect.TopLeft + difference);
            // Updating other handles so they match the new transform
            UpdateUpHandlePosition();
            UpdateDownHandlePosition();
            UpdateLeftHandlePosition();
            UpdateRightHandlePosition();

            lastPos = currentPos;
            yield return new WaitForEndOfFrame();
        }

        MoveShapeAction moveAction = new MoveShapeAction(rect,
            new List<Vector2>() { startPos },
            new List<Vector2>() { rect.TopLeft });
        ActionLogger.LogAction(moveAction);
    }
    // Scales shape upward using the up handle
    IEnumerator ScaleUp()
    {
        Vector2 startPos = rect.TopLeft;
        Vector2 startScale = rect.Scale;
        // While handle is held we offset the top of the rectangle
        // by the difference between the previous and current handle positions
        while (upHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)upHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            rect.SetLocalPoint(0, startPos + Vector2.up * difference.y);
            rect.SetScale(startScale + Vector2.up * -difference.y);
            // Updating other handles so they match the new transform
            UpdateMoveHandlePosition();
            UpdateDownHandlePosition();
            UpdateLeftHandlePosition();
            UpdateRightHandlePosition();

            yield return new WaitForEndOfFrame();
        }

        MoveAndScaleShapeAction action = new MoveAndScaleShapeAction(rect,
            new List<Vector2>() { startPos }, new List<Vector2>() { rect.TopLeft },
            startScale, rect.Scale);
        ActionLogger.LogAction(action);
    }
    // Scales the shape downwards using the down handle
    IEnumerator ScaleDown()
    {
        Vector2 startPos = rect.TopLeft + new Vector2(rect.Scale.x / 2f, rect.Scale.y);
        Vector2 startScale = rect.Scale;
        // While handle is held we offset the bottom of the rectangle
        // by the difference between the previous and current handle positions
        while (downHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)downHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            rect.SetScale(startScale + Vector2.up * difference.y);
            // Updating other handles so they match the new transform
            UpdateMoveHandlePosition();
            UpdateUpHandlePosition();
            UpdateLeftHandlePosition();
            UpdateRightHandlePosition();

            yield return new WaitForEndOfFrame();
        }

        ScaleShapeAction action = new ScaleShapeAction(rect, startScale, rect.Scale);
        ActionLogger.LogAction(action);
    }
    // Scales shape to the left using the left handle
    IEnumerator ScaleLeft()
    {
        Vector2 startPos = rect.TopLeft;
        Vector2 startScale = rect.Scale;
        // While handle is held we offset the left of the rectangle
        // by the difference between the previous and current handle positions
        while (leftHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)leftHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            rect.SetLocalPoint(0, startPos + Vector2.right * difference.x);
            rect.SetScale(startScale + Vector2.right * -difference.x);
            // Updating other handles so they match the new transform
            UpdateMoveHandlePosition();
            UpdateDownHandlePosition();
            UpdateUpHandlePosition();
            UpdateRightHandlePosition();

            yield return new WaitForEndOfFrame();
        }

        MoveAndScaleShapeAction action = new MoveAndScaleShapeAction(rect,
            new List<Vector2>() { startPos }, new List<Vector2>() { rect.TopLeft },
            startScale, rect.Scale);
        ActionLogger.LogAction(action);
    }
    // Scales shape to the right using right handle
    IEnumerator ScaleRight()
    {
        Vector2 startPos = rect.TopLeft + new Vector2(rect.Scale.x, rect.Scale.y / 2f);
        Vector2 startScale = rect.Scale;
        // While handle is held we offset the center of the rectangle
        // by the difference between the previous and current handle positions
        while (rightHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)rightHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            rect.SetScale(startScale + Vector2.right * difference.x);
            // Updating other handles so they match the new transform
            UpdateMoveHandlePosition();
            UpdateDownHandlePosition();
            UpdateUpHandlePosition();
            UpdateLeftHandlePosition();

            yield return new WaitForEndOfFrame();
        }

        ScaleShapeAction action = new ScaleShapeAction(rect, startScale, rect.Scale);
        ActionLogger.LogAction(action);
    }
    protected override void OnHandleReleased(HandleUI handle)
    {
        base.OnHandleReleased(handle);
    }
}
