using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Design;

public class LayerUI : MonoBehaviour
{
    public int LayerIndex => transform.GetSiblingIndex();
    [Header("UI References")]
    [SerializeField] private TMP_Text layerNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedUI;
    [SerializeField] private Image shapeIcon;
    [Header("Shape Icons")]
    [SerializeField] private Sprite rectangleIcon;
    [SerializeField] private Sprite ellipseIcon;
    [SerializeField] private Sprite triangleIcon;
    [SerializeField] private Sprite lineIcon;
    [SerializeField] private Sprite customShapeIcon;
    [SerializeField] private Sprite designIcon;
    void Start()
    {
        CanvasManager.OnLayerChanged += UpdateUI;
        UpdateUI();
    }
    private void OnDestroy()
    {
        CanvasManager.OnLayerChanged -= UpdateUI;
    }
    public void UpdateUI()
    {
        if (LayerIndex >= CanvasManager.LayerCount) return;

        // If it is a shape layer, we display the shape's icon
        if(CanvasManager.CurrentDesign.layers[LayerIndex] is Design.ShapeLayer shapeLayer)
        {
            Shape shape = shapeLayer.shape;
            layerNameText.text = shape.ToString();
            shapeIcon.color = shape.FillColor;
            switch (shape.ToString().ToLower())
            {
                case "rectangle":
                    shapeIcon.sprite = rectangleIcon;
                    break;
                case "ellipse":
                    shapeIcon.sprite = ellipseIcon;
                    break;
                case "triangle":
                    shapeIcon.sprite = triangleIcon;
                    break;
                case "line":
                    shapeIcon.sprite = lineIcon;
                    break;
                default:
                    shapeIcon.sprite = customShapeIcon;
                    break;
            }
        }
        else if (CanvasManager.CurrentDesign.layers[LayerIndex] is Design.DesignLayer designLayer)
        {
            Design design = designLayer.design;
            layerNameText.text = design.Name;
            shapeIcon.color = Color.white;
            shapeIcon.sprite = designIcon;
        }

        // If this layer is the current layer, we enable its selected ui
        selectedUI.SetActive(LayerIndex == CanvasManager.CurrentLayerIndex);
    }
    public void Up()
    {
        CanvasManager.MoveLayer(LayerIndex, LayerIndex - 1, true);
    }
    public void Down()
    {
        CanvasManager.MoveLayer(LayerIndex, LayerIndex + 1, true);
    }
    public void Select()
    {
        CanvasManager.ChangeLayer(LayerIndex);
    }
    public void Delete()
    {
        int shapeLayerIndex = LayerIndex;
        ILayer currentLayer = CanvasManager.CurrentDesign.layers[shapeLayerIndex];
        IAction action = null;
        // Creating action to log it later
        if (currentLayer is ShapeLayer shapeLayer)
        {
            Shape shape = shapeLayer.shape;
            GameObject rendererObj = DesignEditor.instance.FindShapeRendererFromShape(shape).gameObject;
            action = new RemoveShapeAction(shape, shapeLayerIndex, rendererObj);
        }
        else if (currentLayer is DesignLayer designLayer)
        {
            action = new RemoveDesignAction(designLayer.design, shapeLayerIndex);
        }

        CanvasManager.RemoveLayer(shapeLayerIndex);
        ActionLogger.LogAction(action);
    }
    public void Duplicate()
    {
        UIManager.instance.DuplicateLayer(LayerIndex);
    }
}
