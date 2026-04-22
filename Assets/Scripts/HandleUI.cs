using PowerfistTools;
using UnityEngine;

public class HandleUI : MonoBehaviour
{
    Vector2 worldPosition;
    MovementDirection direction;
    public bool IsHeld { get; private set; }
    int GRID_SIZE => CanvasManager.GRID_SIZE;
    void Update()
    {
        if (IsHeld)
        {
            // Move handle to mouse position based on allowed movement direction
            Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = mousePos.SnapToGrid(GRID_SIZE);
            switch (direction)
            {
                case MovementDirection.Bidirectional:
                    transform.position = mousePos;
                    break;
                case MovementDirection.Horizontal:
                    transform.position = new Vector2(mousePos.x, transform.position.y);
                    break;
                case MovementDirection.Vertical:
                    transform.position = new Vector2(transform.position.x, mousePos.y);
                    break;
            }
        }
        else
        {
            transform.position = worldPosition;
        }
    }
    public void Setup(Vector2 worldPos, MovementDirection direction)
    {
        worldPosition = worldPos;
        this.direction = direction;
    }
    public void SetPosition(Vector2 worldPos)
    {
        worldPosition = worldPos;
    }
    public void Hold()
    {
        IsHeld = true;
    }
    public void Release()
    {
        IsHeld = false;
        worldPosition = transform.position;
    }
    public enum MovementDirection { Horizontal, Vertical, Bidirectional };
}
