using PowerfistTools;
using System.Collections;
using UnityEngine;

public class DesignMover : MonoBehaviour
{
    public int DesignIndex => CanvasManager.GetDesignLayerIndex(design);
    public Design design { get; private set; }
    bool isSelected = false;
    HandleUI handle;
    // Start is called before the first frame update
    void Start()
    {
        CanvasManager.OnLayerRemoved += OnLayerRemoved;
        CanvasManager.OnLayerChanged += OnLayerChanged;
    }
    void OnLayerRemoved(int index, ILayer layer)
    {
        // If design matches the deleted layer's design, we destroy
        if(layer is Design.DesignLayer designLayer && designLayer.design == design)
        {
            DestroyMover();
        }
    }
    void OnLayerChanged()
    {
        // If the current layer is this design's layer, we select it and setup it's handle ui
        if(isSelected && CanvasManager.CurrentLayerIndex != DesignIndex)
        {
            Deselect();
        }
        if (!isSelected && CanvasManager.CurrentLayerIndex == DesignIndex)
        {
            Select();
        }
    }
    // Sets up handle UI so user could move the design
    void Select()
    {
        handle = UIManager.instance.GetHandles(1)[0];
        handle.Setup(design.Center, HandleUI.MovementDirection.Bidirectional);
        Vector2 handlePos = handle.transform.position;
        UIManager.instance.OnHandleHeld += OnHandleHeld;
        isSelected = true;
    }
    // Once the user holds the handle, we want to track its movement in a Coroutine
    void OnHandleHeld(HandleUI handle)
    {
        if(handle != this.handle) return;
        StartCoroutine(MoveDesign());
    }
    // Displaces the design's position as the handle is moved
    IEnumerator MoveDesign()
    {
        Vector2 startPos = design.LocalPosition;
        Vector2 lastPos = ((Vector2)handle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
        while (handle.IsHeld)
        {
            Vector2 currentPos = ((Vector2)handle.transform.position).SnapToGrid(CanvasManager.GRID_SIZE);
            Vector2 delta = currentPos - lastPos;
            design.LocalPosition += delta;
            lastPos = currentPos;
            yield return null;
        }

        MoveDesignAction moveAction = new MoveDesignAction(design, startPos, design.LocalPosition);
        ActionLogger.LogAction(moveAction);
    }
    void Deselect()
    {
        isSelected = false;
        UIManager.instance.OnHandleHeld -= OnHandleHeld;
    }

    public void DestroyMover()
    {
        if (isSelected) Deselect();
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        CanvasManager.OnLayerRemoved -= OnLayerRemoved;
        CanvasManager.OnLayerChanged -= OnLayerChanged;
    }
    public static void InstantiateMover(Design design)
    {
        GameObject moverObj = new GameObject(design.Name + " Mover");
        DesignMover mover = moverObj.AddComponent<DesignMover>();
        mover.design = design;
    }
}
