using System.Collections.Generic;
using UnityEngine;
using DesignLayer = Design.DesignLayer;
using ShapeLayer = Design.ShapeLayer;

public static class CanvasManager
{
    public const int GRID_SIZE = 1;
    private static List<Design> allDesigns = null;
    //Stores all designs so they can easily be imported or loaded
    public static Design[] AllDesigns
    {
        get
        {
            if(allDesigns == null)
            {
                SaveSystem.VerifyAndCreateFolder(Design.FOLDER_PATH);
                allDesigns = new List<Design>();
                string[] designPaths = SaveSystem.LoadAllFilePathsInFolder(Design.FOLDER_PATH);
                foreach (string path in designPaths)
                {
                    Design design = LoadDesign(path);
                    if (design != null) allDesigns.Add(design);
                }
            }
            return allDesigns.ToArray();
        }
    }
    // Stores all the shapes in the current design and its sub-designs
    // This is used to quickly get the order of shapes in the design,
    // without having to traverse the design tree every time
    private static List<Shape> allShapes = new();
    public static int CurrentLayerIndex { get; private set; } = 0;
    public static int LayerCount
    {
        get
        {
            return CurrentDesign != null ? CurrentDesign.layers.Count : 0;
        }
    }

    public delegate void OnStateChange();
    // Invoked when the current layer changes
    public static event OnStateChange OnLayerChanged;
    // Invoked when a layer is added or removed
    public static event OnStateChange OnLayerCountChanged;
    public delegate void OnLayerChange(int val, ILayer layer);
    public static event OnLayerChange OnLayerRemoved;
    public static event OnLayerChange OnLayerAdded;
    public delegate void OnLayerMove(int from, int to);
    public static event OnLayerMove OnLayerMoved;
    // The design currently being edited. All layer changes will be made to this design
    public static Design CurrentDesign { get; private set; } = null;
    // The shape currently being edited. Will be null if the current layer is a design layer
    public static Shape CurrentShape
    {
        get
        {
            if(LayerCount == 0 || CurrentDesign == null) return null;

            ILayer layer = CurrentDesign.layers[CurrentLayerIndex];
            if(layer is ShapeLayer shapeLayer) return shapeLayer.shape;
            else return null;
        }
    }

    /// <summary>
    /// Sets the current design to the specified design.
    /// </summary>
    public static void EditDesign(Design design)
    {
        // If the design is not already in the all designs list, add it
        if (allDesigns != null && !allDesigns.Contains(design)) allDesigns.Add(design);
        CurrentDesign = design;

        allShapes.Clear();
        InsertDesignIntoAllShapes(CurrentDesign, 0);

        OnLayerCountChanged?.Invoke();
    }

    /// <summary>
    /// Imports all shapes from a design and its sub-designs as shape layers into the current design at the specified index.
    /// </summary>
    public static void ImportShapesFromDesign(Design design, int index = -1, bool logAction = false)
    {
        // If the current design is part of the imported design,
        // we want to return to avoid infinite loops
        if (DesignExistsWithinDesign(design, CurrentDesign)) return;

        if (index < 0) index = LayerCount;
        List<Shape> shapes = GetAllShapesFromDesign(design);
        ILayer[] layersAdded = new ILayer[shapes.Count];
        for (int i = 0; i < shapes.Count; i++)
        {
            // Cloning shape and making its parent the CurrentDesign
            Shape shape = Shape.ShapeFromData(new ShapeData(shapes[i]), CurrentDesign);
            shapes[i] = shape;
            ShapeLayer layer = new ShapeLayer(shape);
            layersAdded[i] = layer;
            CurrentDesign.layers.Insert(index + i, layer);
            OnLayerAdded?.Invoke(index + i, layer);
        }

        //Finding shape index in all shapes list and adding shapes there
        int allShapesIndex = LayerIndexToAllShapesIndex(index);
        for (int i = 0; i < shapes.Count; i++)
        {
            allShapes.Insert(allShapesIndex + i, shapes[i]);
        }

        if (logAction)
        {
            AddLayerGroupAction addLayerGroupAction = new AddLayerGroupAction(index, layersAdded);
            ActionLogger.LogAction(addLayerGroupAction);
        }

        OnLayerCountChanged?.Invoke();
    }
    // Recursively gets all shapes from a design and its sub-designs and returns them in a list
    static List<Shape> GetAllShapesFromDesign(Design design)
    {
        List<Shape> shapes = new List<Shape>();
        for (int i = 0; i < design.layers.Count; i++)
        {
            if (design.layers[i] is ShapeLayer shapeLayer) shapes.Add(shapeLayer.shape);
            else if (design.layers[i] is DesignLayer designLayer)
            {
                List<Shape> nestedShapes = GetAllShapesFromDesign(designLayer.design);
                shapes.AddRange(nestedShapes);
            }
        }
        return shapes;
    }

    /// <summary>
    /// Imports a design as a design layer into the current design at the specified index.
    /// </summary>
    public static void ImportDesign(Design design, int index = -1, bool changeLayer = true)
    {
        // If the current design is part of the imported design, we want to return to avoid infinite loops
        if (DesignExistsWithinDesign(design, CurrentDesign)) return;

        if (index < 0) index = LayerCount;
        design.parent = CurrentDesign;
        DesignLayer designLayer = new DesignLayer(design);
        CurrentDesign.layers.Insert(index, designLayer);
        if (changeLayer) ChangeLayer(index);

        //Finding shape index in all shapes list
        int indexOfShapes = LayerIndexToAllShapesIndex(index);
        InsertDesignIntoAllShapes(design, indexOfShapes);

        OnLayerCountChanged?.Invoke();
        OnLayerAdded?.Invoke(index, designLayer);
    }
    // Converts Layer index in CurrentDesign to index in allShapes list
    static int LayerIndexToAllShapesIndex(int layerIndex)
    {
        int index = 0;
        for (int i = 0; i < layerIndex && CurrentDesign.layers.Count > 0; i++)
        {
            if (CurrentDesign.layers[i] is ShapeLayer)
            {
                index++;
            }
            else if (CurrentDesign.layers[i] is DesignLayer designLayer)
            {
                index += GetDesignTotalShapeCount(designLayer.design);
            }
        }
        return index;
    }
    // Recursively counts the total number of shapes in a design and its sub-designs
    static int GetDesignTotalShapeCount(Design design)
    {
        int shapeCount = 0;
        for (int i = 0; i < design.layers.Count; i++)
        {
            if (design.layers[i] is ShapeLayer)
            {
                shapeCount++;
            }
            else if (design.layers[i] is DesignLayer designLayer)
            {
                shapeCount += GetDesignTotalShapeCount(designLayer.design);
            }
        }
        return shapeCount;
    }
    // Recursively inserts all shapes from a design and its sub-designs into the allShapes list
    static void InsertDesignIntoAllShapes(Design design, int allShapesIndex)
    {
        for (int i = 0; i < design.layers.Count; i++)
        {
            if (design.layers[i] is ShapeLayer shapeLayer)
                allShapes.Insert(allShapesIndex + i, shapeLayer.shape);
            else if (design.layers[i] is DesignLayer designLayer)
            {
                InsertDesignIntoAllShapes(designLayer.design, allShapesIndex + i);
                allShapesIndex += designLayer.design.layers.Count - 1;
            }
        }
    }
    /// <summary>
    /// Recursively checks if a design exists within another design or its sub-designs
    /// </summary>
    public static bool DesignExistsWithinDesign(Design parent, Design design)
    {
        for (int i = 0; i < parent.layers.Count; i++)
        {
            if (parent.layers[i] is not DesignLayer designLayer) continue;
            if (designLayer.design.Equals(design)) return true;
            else if (DesignExistsWithinDesign(designLayer.design, design)) return true;
        }
        return false;
    }
    /// <summary>
    /// Adds a shape layer to the current design at the specified index. If index is -1, adds to the end of the layers list.
    /// </summary>
    public static void AddShapeLayer(Shape shape, int index = -1, bool changeLayer = true)
    {
        if(index < 0) index = LayerCount;
        ShapeLayer shapeLayer = new ShapeLayer(shape);
        CurrentDesign.layers.Insert(index, shapeLayer);
        if (changeLayer) ChangeLayer(index);

        //Finding shape index in all shapes list and adding it there
        int indexOfShape = LayerIndexToAllShapesIndex(index);
        allShapes.Insert(indexOfShape, shape);

        OnLayerCountChanged?.Invoke();
        OnLayerAdded?.Invoke(index, shapeLayer);
    }
    /// <summary>
    /// Sets current layer index to specified index
    /// </summary>
    public static void ChangeLayer(int layer)
    {
        if(layer < 0) CurrentLayerIndex = 0;
        else if(layer >= LayerCount) CurrentLayerIndex = LayerCount - 1;
        else CurrentLayerIndex = layer;
        OnLayerChanged?.Invoke();
    }
    /// <summary>
    /// Moves layer from one index to another.
    /// </summary>
    public static void MoveLayer(int from, int to, bool logAction = false)
    {
        // Validating from and to indices
        if (from < 0 || from >= LayerCount || to < 0 || to >= LayerCount) return;
        ILayer layer = CurrentDesign.layers[from];
        CurrentDesign.layers.RemoveAt(from);
        CurrentDesign.layers.Insert(to, layer);
        ChangeLayer(to);

        // Moving in all shapes list
        int allShapesIndex = LayerIndexToAllShapesIndex(to);
        if (layer is ShapeLayer shapeLayer)
        {
            allShapes.Remove(shapeLayer.shape);
            allShapes.Insert(allShapesIndex, shapeLayer.shape);
        }
        else if(layer is DesignLayer designLayer)
        {
            RemoveShapesOfDesign(designLayer.design);
            InsertDesignIntoAllShapes(designLayer.design, allShapesIndex);
        }

        if (logAction) ActionLogger.LogAction(new MoveLayerAction(from, to));
        OnLayerMoved?.Invoke(from, to);
    }
    /// <summary>
    /// Removes layer at specified index.
    /// </summary>
    public static void RemoveLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= LayerCount) return;
        ILayer removedLayer = CurrentDesign.layers[layerIndex];
        CurrentDesign.layers.RemoveAt(layerIndex);
        // If the removed layer is before or at the current layer index,
        // we want to move the current layer index down by 1 to avoid out of range errors
        if (layerIndex <= CurrentLayerIndex) ChangeLayer(CurrentLayerIndex - 1);

        //Remove from all shapes list
        if(removedLayer is ShapeLayer shapeLayer)
        {
            allShapes.Remove(shapeLayer.shape);
        }
        else if (removedLayer is DesignLayer designLayer)
        {
            RemoveShapesOfDesign(designLayer.design);
        }

        OnLayerCountChanged?.Invoke();
        OnLayerRemoved?.Invoke(layerIndex, removedLayer);
    }
    // Recursively removes all shapes from a design and its sub-designs from the allShapes list
    static void RemoveShapesOfDesign(Design design)
    {
        foreach (ILayer layer in design.layers)
        {
            if (layer is ShapeLayer shapeLayer) allShapes.Remove(shapeLayer.shape);
            else if (layer is DesignLayer designLayer) RemoveShapesOfDesign(designLayer.design);
        }
    }

    /// <summary>
    /// Gets the index of a shape in the allShapes list.
    /// This is used to determine the order of shapes in the design.
    /// </summary>
    public static int GetShapeIndex(Shape shape)
    {
        return allShapes.IndexOf(shape);
    }
    /// <summary>
    /// Gets the index of a design layer in the current design's layers list.
    /// Returns -1 if the design is not found in any design layer.
    /// </summary>
    public static int GetDesignLayerIndex(Design design)
    {
        for (int i = 0; i < CurrentDesign.layers.Count; i++)
        {
            if (CurrentDesign.layers[i] is DesignLayer designLayer && designLayer.design == design) return i;
        }
        return -1;
    }

    /// <summary>
    /// Returns true if a design with the same name as the specified path exists in the Designs folder, false otherwise.
    /// </summary>
    public static bool DesignExists(string path)
    {
        SaveSystem.VerifyAndCreateFolder(Design.FOLDER_PATH);
        return SaveSystem.LoadData<DesignData>(path) != null;
    }
    /// <summary>
    /// Returns a design loaded from the specified path. Returns null if no design is found at the path.
    /// </summary>
    public static Design LoadDesign(string path)
    {
        SaveSystem.VerifyAndCreateFolder(Design.FOLDER_PATH);
        DesignData designData = SaveSystem.LoadData<DesignData>(path);
        if (designData == null) return null;

        Design design = new Design(designData, path);
        return design;
    }
    /// <summary>
    /// Deletes the file with the given name
    /// </summary>
    public static void DeleteDesignFile(string fileName)
    {
        DeleteDesignFile(LoadDesign(fileName));
    }
    /// <summary>
    /// Deletes the file with the given file name
    /// </summary>
    public static void DeleteDesignFile(Design design)
    {
        SaveSystem.DeleteFile(design.path);

        // Removing from all designs list
        for (int i = 0; i < allDesigns.Count; i++)
        {
            if (allDesigns[i].Equals(design))
            {
                allDesigns.RemoveAt(i);
                return;
            }
        }
    }
    /// <summary>
    /// Saves the current design to its path.
    /// </summary>
    public static void Save()
    {
        //Save Design
        SaveSystem.VerifyAndCreateFolder(Design.FOLDER_PATH);
        SaveSystem.SaveData(CurrentDesign.ExtractJson(), CurrentDesign.path);
    }

    /// <summary>
    /// prints a list of all shapes in the current design and its sub-designs to the console. Used for debugging purposes.
    /// </summary>
    public static void LogAllShapes()
    {
        string msg = "All shapes in current design: \n";
        for (int i = 0; i < allShapes.Count; i++)
        {
            msg += allShapes[i].ToString() + "\n";
        }
        Debug.Log(msg);
    }
}
