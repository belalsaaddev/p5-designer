using UnityEngine;
using DesignLayer = Design.DesignLayer;
using ShapeLayer = Design.ShapeLayer;
[System.Serializable]
public class DesignData
{
    [System.Serializable]
    public struct LayerData : ILayer // Stores data for both shape and design layers
    {
        public ShapeData shapeData;
        public string path;
        public Vector2 position;
        public string layerType;
        public LayerData(Shape shape)
        {
            this.shapeData = new ShapeData(shape);
            path = "";
            position = Vector2.zero;
            layerType = "shape";
        }
        public LayerData(ShapeData shapeData)
        {
            this.shapeData = shapeData;
            path = "";
            position = Vector2.zero;
            layerType = "shape";
        }
        public LayerData(Design design)
        {
            shapeData = null;
            path = design.path;
            position = design.LocalPosition;
            layerType = "design";
        }
    }
    public string Name;
    public LayerData[] layers;
    public DesignData()
    {
        Name = "New Design";
        layers = new LayerData[0];
    }
    public DesignData(Design design)
    {
        Name = design.Name;
        layers = new LayerData[design.layers.Count];
        for (int i = 0; i < layers.Length; i++)
        {
            if(design.layers[i] is ShapeLayer shapeLayer)
                layers[i] = new LayerData(shapeLayer.shape);
            else if(design.layers[i] is DesignLayer designLayer)
                layers[i] = new LayerData(designLayer.design);
        }
    }
}
