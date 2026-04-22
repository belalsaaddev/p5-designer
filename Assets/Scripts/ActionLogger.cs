using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionLogger
{
    public static bool saveAfterEveryAction = true;
    private static readonly List<IAction> actionHistory = new List<IAction>();
    // currentActionIndex goes through the history list from the end,
    // -1 is the most recent action, -2 is the one before that, etc.
    private static int currentActionIndex = -1;
    public delegate void OnActionChanged();
    public static OnActionChanged OnUndo;
    public static OnActionChanged OnRedo;
    public static void Undo()
    {
        int targetIndex = actionHistory.Count + currentActionIndex;
        if (targetIndex < 0) return;
        actionHistory[targetIndex].Undo();
        Debug.Log("Undo action: " + actionHistory[targetIndex].ToString());
        // Move the index back by one, so the next undo will target the previous action in the history
        currentActionIndex--;
        OnUndo?.Invoke();

        // Saving after undoing
        Save();
    }
    public static void Redo()
    {
        int targetIndex = actionHistory.Count + currentActionIndex + 1;
        if (targetIndex == actionHistory.Count) return;
        actionHistory[targetIndex].Redo();
        Debug.Log("Redo action: " + actionHistory[targetIndex].ToString());
        // Move the index forward by one, so the next redo will target the next action in the history
        currentActionIndex++;
        OnRedo?.Invoke();

        // Saving after redoing
        Save();
    }
    public static void LogAction(IAction action)
    {
        // Remove any actions ahead of the current index in the history,
        // since they are no longer relevant once a new action is taken
        int targetIndex = actionHistory.Count + currentActionIndex + 1;
        if (targetIndex < actionHistory.Count)
        {
            actionHistory.RemoveRange(targetIndex, actionHistory.Count - targetIndex);
        }
        actionHistory.Add(action);
        currentActionIndex = -1;

        // Saving after every action
        Save();
    }
    
    static void Save()
    {
        if (!saveAfterEveryAction) return;
        CanvasManager.Save();
    }
}
