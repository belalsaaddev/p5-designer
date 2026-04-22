using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private GraphicRaycaster raycaster;
    [Header("Shapes and Toolbar References")]
    [SerializeField] private GameObject toolBar;
    [SerializeField] private GameObject drawShapeMsg;
    [SerializeField] private TMP_Text drawShapeText;
    [SerializeField] private Transform orginMark;
    [Header("Layer References")]
    [SerializeField] private RectTransform layersHolder;
    [SerializeField] private RectTransform layersBackground;
    private float layerSpacing;
    private float layerHeight = 40;
    private float layersHolderMinHeight = 165f;
    [SerializeField] private ScrollRect layersScrollRect;
    [SerializeField] private GameObject layerUIPrefab;
    private readonly List<GameObject> layerUIs = new ();
    [Header("Handles References")]
    [SerializeField] private Transform handlesHolder;
    [SerializeField] private GameObject handlePrefab;
    private readonly List<HandleUI> handles = new();
    private HandleUI currentHandle = null;
    public delegate void OnHandleState(HandleUI handle);
    public event OnHandleState OnHandleHeld;
    public event OnHandleState OnHandleReleased;

    [Header("Import References")]
    [SerializeField] private GameObject importPanel;
    [SerializeField] private GameObject importDesignUI;
    [SerializeField] private RectTransform importDesignHolder;
    private readonly List<DesignFileUI> importFileUIs = new();
    private float importDesignHolderMinHeight;
    [SerializeField] private Toggle importShapesToggle;
    [SerializeField] private ScrollRect importScrollRect;

    [Header("Shape Info References")]
    [SerializeField] private GameObject propertiesHolder;
    [SerializeField] private Toggle noFill;
    [SerializeField] private Image fillColor;
    [SerializeField] private TMP_InputField strokeWeight;
    [SerializeField] private Image strokeColor;
    [SerializeField] private Toggle closedShape;
    [SerializeField] private TMP_Text codePreviewText;

    [SerializeField] private ColorPicker picker;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        layerHeight = layerUIPrefab.GetComponent<RectTransform>().rect.height;
        layerSpacing = layersHolder.GetComponent<VerticalLayoutGroup>().spacing;
        layersHolderMinHeight = layersBackground.GetComponent<RectTransform>().rect.height - 10f;

        CanvasManager.OnLayerCountChanged += LayerCountChanged;
        CanvasManager.OnLayerChanged += UpdateInfo;
        ActionLogger.OnUndo += UpdateInfo;
        ActionLogger.OnRedo += UpdateInfo;

        InitialiseLayers();

        InitialiseImportPanel();
    }
    private void Update()
    {
        // Positioning orgin mark at the orgin
        orginMark.position = Vector3.zero;

        if(currentHandle == null && Input.GetMouseButtonDown(0))
        {
            // Create a PointerEventData object
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            // Create a list to store raycast results
            List<RaycastResult> results = new List<RaycastResult>();

            // Perform the raycast
            raycaster.Raycast(pointerData, results);

            // Holding handles if they are hit by the raycast so user can drag them
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.CompareTag("Handle"))
                {
                    HandleUI handle = results[i].gameObject.GetComponent<HandleUI>();
                    HoldHandle(handle);
                    break;
                }
            }
        }

        // Displaying code of shape in code preview text
        if (CanvasManager.LayerCount != 0 && CanvasManager.CurrentShape != null)
        {
            codePreviewText.text = CanvasManager.CurrentShape.GetCode();
        }
    }
    private void OnDestroy()
    {
        CanvasManager.OnLayerCountChanged -= LayerCountChanged;
        CanvasManager.OnLayerChanged -= UpdateInfo;
        ActionLogger.OnUndo -= UpdateInfo;
        ActionLogger.OnRedo -= UpdateInfo;
    }
    public void InitialiseLayers()
    {
        // Creates layer ui for each layer
        for (int i = 0; i < CanvasManager.LayerCount; i++)
        {
            GameObject newLayer = Instantiate(layerUIPrefab, layersHolder);
            layerUIs.Add(newLayer);
        }

        UpdateInfo();
        ScaleLayersHolder();
        // Making scroll rect snap to the top
        layersScrollRect.verticalNormalizedPosition = 1;
    }
    // Makes sure layers holder fits the size of layer ui
    void ScaleLayersHolder()
    {
        float layersHolderHeight = Mathf.Max(CanvasManager.LayerCount * (layerHeight + layerSpacing) - layerSpacing, layersHolderMinHeight);
        layersHolder.sizeDelta = new Vector2(layersHolder.sizeDelta.x, layersHolderHeight);
    }
    void LayerCountChanged()
    {
        int difference = CanvasManager.LayerCount - layerUIs.Count;
        // Add layer ui for each layer we are missing
        while (difference > 0)
        {
            GameObject newLayer = Instantiate(layerUIPrefab, layersHolder);
            layerUIs.Add(newLayer);
            difference--;
        }
        // Remove layer ui for each extra layer ui we have
        while (difference < 0)
        {
            //Remove layer
            Destroy(layerUIs[^1]);
            layerUIs.RemoveAt(layerUIs.Count - 1);
            difference++;
        }

        ScaleLayersHolder();

        // If there are no layers, we disable all handles
        if (CanvasManager.LayerCount == 0)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                handles[i].gameObject.SetActive(false);
            }
        }
    }
    public void DuplicateLayer(int index)
    {
        Debug.Log("Duplicate layer at index " + index);
        Design currentDesign = CanvasManager.CurrentDesign;
        if(currentDesign.layers[index] is Design.ShapeLayer shapeLayer)
        {
            // Cloning shape
            Shape shape = Shape.ShapeFromData(new ShapeData(shapeLayer.shape), currentDesign);
            // Creating a new renderer for cloned shape
            GameObject renderer = ShapeRenderer.InstantiateRenderer(shape);
            // Adding new layer
            CanvasManager.AddShapeLayer(shape, index + 1, false);
            CanvasManager.ChangeLayer(index);

            AddShapeAction addAction = new AddShapeAction(shape, index + 1, renderer);
            ActionLogger.LogAction(addAction);
        }
        else if(currentDesign.layers[index] is Design.DesignLayer designLayer)
        {
            // Cloning design by loading a new one from the same path
            Design design = CanvasManager.LoadDesign(designLayer.design.path);
            CanvasManager.ImportDesign(design, index + 1, false);

            AddDesignAction addAction = new AddDesignAction(design, index + 1);
            ActionLogger.LogAction(addAction);
        }
    }

    // Provides an array of handle uis containing the specified count of handles
    // Uses an object pooling approach to save performance
    public HandleUI[] GetHandles(int count)
    {
        // If we dont have enough handles, we create more
        if(handles.Count < count)
        {
            int difference = count - handles.Count;
            for (int i = 0; i < difference; i++)
            {
                HandleUI newHandle = Instantiate(handlePrefab, handlesHolder).GetComponent<HandleUI>();
                handles.Add(newHandle);
            }
        }

        // Collecting required amount of handles and disabling the rest
        HandleUI[] selectedHandles = new HandleUI[count];
        for (int i = 0; i < handles.Count; i++)
        {
            if(i < count) selectedHandles[i] = handles[i];
            handles[i].gameObject.SetActive(i < count);
        }
        return selectedHandles;
    }
    void HoldHandle(HandleUI handle)
    {
        handle.Hold();
        currentHandle = handle;
        OnHandleHeld?.Invoke(handle);
        StartCoroutine(WaitForHandleRelease());
    }
    // Moniter user input so handle is released once left mouse button is released
    IEnumerator WaitForHandleRelease()
    {
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
        currentHandle.Release();
        OnHandleReleased?.Invoke(currentHandle);
        currentHandle = null;
    }

    public void ShowToolBar()
    {
        toolBar.SetActive(true);
    }
    public void HideToolBar()
    {
        toolBar.SetActive(false);
    }

    public void ShowDrawShapeMsg(string msg = "Click and hold to draw shape")
    {
        drawShapeText.text = msg;
        drawShapeMsg.SetActive(true);
    }
    public void HideDrawShapeMsg()
    {
        drawShapeMsg.SetActive(false);
    }

    public void DrawRectangle()
    {
        DesignEditor.instance.CreateShape(DesignEditor.ShapeType.Rectangle);
    }
    public void DrawEllipse()
    {
        DesignEditor.instance.CreateShape(DesignEditor.ShapeType.Ellipse);
    }
    public void DrawTriangle()
    {
        DesignEditor.instance.CreateShape(DesignEditor.ShapeType.Triangle);
    }
    public void DrawLine()
    {
        DesignEditor.instance.CreateShape(DesignEditor.ShapeType.Line);
    }
    public void DrawCustomShape()
    {
        DesignEditor.instance.CreateShape(DesignEditor.ShapeType.Custom);
    }
    public void Export(bool withVariableNames)
    {
        Design design = CanvasManager.CurrentDesign;
        string code = design.GetCode(withVariableNames);
        GUIUtility.systemCopyBuffer = code;
    }

    // Updates properties tab UI
    public void UpdateInfo()
    {
        CloseColorPicker();
        // If there are currently no layers in the design or we are selecting a design layer,
        // we disable properties tab
        if (CanvasManager.LayerCount == 0 || CanvasManager.CurrentShape == null)
        {
            propertiesHolder.SetActive(false);
            return;
        }
        else propertiesHolder.SetActive(true);

        Shape currentShape = CanvasManager.CurrentShape;
        noFill.isOn = currentShape.NoFill;
        fillColor.color = currentShape.FillColor;
        strokeWeight.text = currentShape.StrokeWeight.ToString();
        strokeColor.color = currentShape.StrokeColor;
        closedShape.isOn = currentShape.IsClosed;
    }

    // Sets No Fill of current shape
    public void SetNoFill(bool val)
    {
        if (val == CanvasManager.CurrentShape.NoFill) return;

        if (val)
        {
            CanvasManager.CurrentShape.NoFillShape();
        }
        else
        {
            CanvasManager.CurrentShape.Fill(CanvasManager.CurrentShape.FillColor);
        }

        ShapeNoFillAction noFillAction = new ShapeNoFillAction(CanvasManager.CurrentShape, val, CanvasManager.CurrentShape.FillColor);
        ActionLogger.LogAction(noFillAction);
    }

    private Coroutine colorEditCoroutine;
    private Shape colorEditShape;
    private Color shapeEditStartColor;
    // If index is 1 then we are editing fill color,
    // if index is 2 then we are editing stroke color,
    // if index is 0 then no color is being edited
    int currentColorEditIndex = 0;
    public void StartEditColor(int index)
    {
        if(picker.gameObject.activeSelf)
        {
            CloseColorPicker();
        }

        currentColorEditIndex = index;
        picker.gameObject.SetActive(true);
        colorEditCoroutine = StartCoroutine(EditColor());
    }
    // Monitors the color picker and applys color changes to the current shape
    IEnumerator EditColor()
    {
        colorEditShape = CanvasManager.CurrentShape;
        shapeEditStartColor = currentColorEditIndex == 1 ? colorEditShape.FillColor : colorEditShape.StrokeColor;

        // If shape has no fill and we are editing fill color, we fill it so we can change the fill color
        if (currentColorEditIndex == 1 && colorEditShape.NoFill)
        {
            // Toggle events will set the shape's no fill value on their own,
            // so we just adjust the Toggle value and it handles the rest
            noFill.isOn = false;
        }

        // While color picker is open, we set the shape's color to the color picker's color
        picker.SetColor(shapeEditStartColor);
        while (picker.gameObject.activeSelf)
        {
            if (currentColorEditIndex == 1)
            {
                colorEditShape.Fill(picker.CurrentColor);
                fillColor.color = picker.CurrentColor;
            }
            else if (currentColorEditIndex == 2)
            {
                colorEditShape.Stroke(picker.CurrentColor, colorEditShape.StrokeWeight);
                strokeColor.color = picker.CurrentColor;
            }
            yield return null;
        }

        LogColorChange();
    }
    // Logs the action for the color change to ActionLogger
    void LogColorChange()
    {
        if(currentColorEditIndex == 1)
        {
            if(shapeEditStartColor == colorEditShape.FillColor) return;

            ShapeFillAction fillAction = new ShapeFillAction(colorEditShape, shapeEditStartColor, colorEditShape.FillColor);
            ActionLogger.LogAction(fillAction);
        }
        else if(currentColorEditIndex == 2)
        {
            if (shapeEditStartColor == colorEditShape.StrokeColor) return;

            ShapeStrokeAction strokeAction =
                    new ShapeStrokeAction(colorEditShape, shapeEditStartColor, colorEditShape.StrokeColor,
                    colorEditShape.StrokeWeight, colorEditShape.StrokeWeight);
            ActionLogger.LogAction(strokeAction);
        }
    }
    public void CloseColorPicker()
    {
        if (colorEditCoroutine != null)
        {
            StopCoroutine(colorEditCoroutine);
            LogColorChange();
        }

        currentColorEditIndex = 0;
        picker.gameObject.SetActive(false);
    }

    float prevStrokeWeight;
    // When stroke weight input field is selected
    public void StrokeWeightSelected(string weight)
    {
        prevStrokeWeight = CanvasManager.CurrentShape.StrokeWeight;
    }
    // When stroke weight input field is edited
    public void SetStrokeWeight(string weight)
    {
        if(weight == string.Empty || !strokeWeight.isFocused) return;

        float.TryParse(weight, out float newWeight);
        CanvasManager.CurrentShape.Stroke(CanvasManager.CurrentShape.StrokeColor, newWeight);
    }
    // When stroke weight input field is done editing
    public void LogStrokeWeight(string weight)
    {
        ShapeStrokeAction strokeAction = new ShapeStrokeAction(CanvasManager.CurrentShape, CanvasManager.CurrentShape.StrokeColor, CanvasManager.CurrentShape.StrokeColor,
            prevStrokeWeight, CanvasManager.CurrentShape.StrokeWeight);
        ActionLogger.LogAction(strokeAction);
    }

    // Sets is Closed of the shape
    public void SetIsClosed(bool val)
    {
        // If no change was made, we return
        if (val == CanvasManager.CurrentShape.IsClosed) return;

        if(!val && !CanvasManager.CurrentShape.CanOpen)
        {
            closedShape.isOn = true;
            return;
        }

        if (val)
        {
            CanvasManager.CurrentShape.CloseShape();
        }
        else
        {
            CanvasManager.CurrentShape.OpenShape();
        }

        ShapeClosedAction closedAction = new ShapeClosedAction(CanvasManager.CurrentShape, val);
        ActionLogger.LogAction(closedAction);
    }

    // Imports a design from the import panel into the current design
    public void ImportDesign(Design design)
    {
        int index = CanvasManager.LayerCount > 0 ? CanvasManager.CurrentLayerIndex + 1 : 0;
        if (importShapesToggle.isOn)
        {
            CanvasManager.ImportShapesFromDesign(design, index, true);
        }
        else
        {
            CanvasManager.ImportDesign(design, index);
            AddDesignAction addAction = new AddDesignAction(design, index);
            ActionLogger.LogAction(addAction);
        }
        
        importPanel.SetActive(false);
    }
    // Sets up import panel UI with all designs
    void InitialiseImportPanel()
    {
        // If no design is loaded, we return
        if (CanvasManager.CurrentDesign == null) return;

        importDesignHolderMinHeight = importDesignHolder.sizeDelta.y;
        Design[] designs = CanvasManager.AllDesigns;
        foreach (Design design in designs)
        {
            // If design is that same as the current design being edited, we skip it
            if(design == CanvasManager.CurrentDesign) continue;
            // If the current design being edited is a part of the design's sub-design tree,
            // we skip the design to avoid infinite looping when rendering shapes
            if (CanvasManager.DesignExistsWithinDesign(design, CanvasManager.CurrentDesign)) continue;

            GameObject designUI = Instantiate(importDesignUI, importDesignHolder);
            DesignFileUI ui = designUI.GetComponent<DesignFileUI>();
            ui.Setup(design);
            importFileUIs.Add(ui);
        }

        // Sorting them based on their last modified time
        DesignFileUI.SortDesignFileUIList(importFileUIs);
        // Ordering them in the import design holder's child hierarchy
        for (int i = 0; i < importFileUIs.Count - 1; i++)
        {
            for (int j = i; j < importFileUIs.Count; j++)
            {
                Transform currentChild = importDesignHolder.GetChild(j);
                if (currentChild.GetComponent<DesignFileUI>() == importFileUIs[i])
                {
                    currentChild.SetSiblingIndex(i);
                    break;
                }
            }
        }

        // Adjusting import design ui holder to fit all loaded designs
        float spacing = importDesignHolder.GetComponent<VerticalLayoutGroup>().spacing;
        float designUIHeight = importDesignUI.GetComponent<RectTransform>().rect.height;
        float totalHeight = Mathf.Max(importFileUIs.Count * (designUIHeight + spacing) - spacing, importDesignHolderMinHeight);
        importDesignHolder.sizeDelta = new Vector2(importDesignHolder.sizeDelta.x, totalHeight);
        // Making scroll rect snap to the top
        importScrollRect.verticalNormalizedPosition = 1;
    }
}
