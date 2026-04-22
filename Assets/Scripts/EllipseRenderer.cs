using PowerfistTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipseRenderer : ShapeRenderer
{
    private Ellipse ellipse;
    private HandleUI moveHandle;
    private HandleUI upHandle;
    private HandleUI rightHandle;
    protected override void Start()
    {
        base.Start();
        if (ellipse == null) Setup();
    }
    public override void Setup()
    {
        ellipse = (Ellipse)shape;
        spriteRenderer.sprite = DesignEditor.instance.ellipseSprite;

        Render();
    }
    public override void Render()
    {
        base.Render();
        transform.position = ellipse.Center;
        transform.localScale = ellipse.Scale;

        // Drawing stroke using a line renderer that covers the circumference of this ellipse
        const int POINT_COUNT = 36;
        Vector3[] strokePoints = new Vector3[POINT_COUNT];
        float angleInterval = 2*Mathf.PI / POINT_COUNT;
        for (int i = 0; i < POINT_COUNT; i++)
        {
            float angle = angleInterval * i;
            float x = Mathf.Cos(angle) * ellipse.Scale.x/2f;
            float y = Mathf.Sin(angle) * ellipse.Scale.y/2f;
            strokePoints[i] = new Vector3(x, y, 0) + (Vector3)ellipse.Center;
        }
        lineRenderer.positionCount = POINT_COUNT;
        lineRenderer.loop = true;
        lineRenderer.SetPositions(strokePoints);
    }

    // Sets up required handles to move ellipse
    protected override void Select()
    {
        base.Select();
        HandleUI[] handles = UIManager.instance.GetHandles(3);

        // Move Handle
        handles[0].Setup(ellipse.Center, HandleUI.MovementDirection.Bidirectional);
        moveHandle = handles[0];

        // Up (Vertical Scaling) Handle
        Vector2 up = ellipse.Center + ellipse.Scale.y / 2 * Vector2.up;
        handles[1].Setup(up, HandleUI.MovementDirection.Vertical);
        upHandle = handles[1];

        // Right (Horizontal Scaling) Handle
        Vector2 right = ellipse.Center + ellipse.Scale.x / 2 * Vector2.right;
        handles[2].Setup(right, HandleUI.MovementDirection.Horizontal);
        rightHandle = handles[2];
    }
    // Places all handle UIs at their correct positions
    protected override void UpdateAllHandlePositions()
    {
        if (!isSelected) return;
        base.UpdateAllHandlePositions();
        UpdateMoveHandlePosition();
        UpdateUpHandlePosition();
        UpdateRightHandlePosition();
    }
    void UpdateMoveHandlePosition()
    {
        moveHandle.SetPosition(ellipse.Center);
    }
    void UpdateUpHandlePosition()
    {
        Vector2 up = ellipse.Center + ellipse.Scale.y / 2 * Vector2.up;
        upHandle.SetPosition(up);
    }
    void UpdateRightHandlePosition()
    {
        Vector2 right = ellipse.Center + ellipse.Scale.x / 2 * Vector2.right;
        rightHandle.SetPosition(right);
    }
    protected override void OnHandleHeld(HandleUI handle)
    {
        base.OnHandleHeld(handle);
        if (handle == moveHandle)
        {
            StartCoroutine(MoveShape());
        }
        else if (handle == upHandle)
        {
            StartCoroutine(ScaleUp());
        }
        else if (handle == rightHandle)
        {
            StartCoroutine(ScaleRight());
        }
    }
    // Moves Shape from center point
    IEnumerator MoveShape()
    {
        Vector2 startPos = ellipse.Center;
        // Settings center to handle position
        while (moveHandle.IsHeld)
        {
            ellipse.Center = ((Vector2)moveHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            UpdateUpHandlePosition();
            UpdateRightHandlePosition();
            yield return new WaitForEndOfFrame();
        }

        MoveShapeAction moveAction = new MoveShapeAction(ellipse,
            new List<Vector2>() { startPos },
            new List<Vector2>() { ellipse.Center });
        ActionLogger.LogAction(moveAction);
    }
    // Scales ellipse vertically using the up handle
    IEnumerator ScaleUp()
    {
        Vector2 startPos = ellipse.Center + ellipse.Scale.y / 2 * Vector2.up;
        Vector2 startScale = ellipse.Scale;
        // While handle is dragged we update position,
        // by the difference in handle's previous and current positions
        while (upHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)upHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            ellipse.SetScale(startScale + difference.y * 2 * Vector2.up);
            UpdateMoveHandlePosition();
            UpdateRightHandlePosition();
            yield return new WaitForEndOfFrame();
        }

        // If scale is negative, we invert it
        if (ellipse.Scale.y < 0)
        {
            ellipse.SetScale(new Vector2(ellipse.Scale.x, ellipse.Scale.y * -1));
            UpdateUpHandlePosition();
        }

        ScaleShapeAction action = new ScaleShapeAction(ellipse, startScale, ellipse.Scale);
        ActionLogger.LogAction(action);
    }
    // Scales ellipse horizontally using right handle
    IEnumerator ScaleRight()
    {
        Vector2 startPos = ellipse.Center + ellipse.Scale.x / 2 * Vector2.right;
        Vector2 startScale = ellipse.Scale;
        // While handle is held we update position,
        // by the differnce between handle's current position and previous position
        while (rightHandle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)rightHandle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 difference = currentPos - startPos;
            ellipse.SetScale(startScale + difference.x * 2 * Vector2.right);
            UpdateMoveHandlePosition();
            UpdateUpHandlePosition();
            yield return new WaitForEndOfFrame();
        }

        // If scale is negative, we invert it
        if (ellipse.Scale.x < 0)
        {
            ellipse.SetScale(new Vector2(ellipse.Scale.x * -1, ellipse.Scale.y));
            UpdateRightHandlePosition();
        }

        ScaleShapeAction action = new ScaleShapeAction(ellipse, startScale, ellipse.Scale);
        ActionLogger.LogAction(action);
    }
}
