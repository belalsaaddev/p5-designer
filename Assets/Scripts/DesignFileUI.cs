using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections.Generic;

public class DesignFileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text pathText;
    [SerializeField] private TMP_Text lastModifiedText;
    [SerializeField] private TMP_Text dateCreatedText;
    public Design design { get; private set; }
    public DateTime LastModified { get; private set; }
    public void Setup(Design design)
    {
        this.design = design;
        nameText.text = design.Name;
        if(pathText) pathText.text = Application.persistentDataPath + design.path;

        FileInfo fileInfo = SaveSystem.GetFileInfo(design.path);

        if (lastModifiedText)
        {
            LastModified = fileInfo.LastWriteTime;
            TimeSpan timePassed = DateTime.Now - LastModified;
            if (timePassed.Days > 0) lastModifiedText.text = timePassed.Days + " days ago";
            else if (timePassed.Hours > 0) lastModifiedText.text = timePassed.Hours + " hours ago";
            else lastModifiedText.text = timePassed.Minutes + " minutes ago";
        }

        if (dateCreatedText)
        {
            DateTime creationTime = fileInfo.CreationTime;
            dateCreatedText.text = $"{creationTime.Day:00}/{creationTime.Month:00}/{creationTime.Year:0000}";
        }
    }
    public void OpenDesign()
    {
        if(MainMenuManager.instance)
        {
            MainMenuManager.instance.OpenDesign(design);
        }
        else
        {
            UIManager.instance.ImportDesign(design);
        }
    }
    public void Delete()
    {
        MainMenuManager.instance.PromptDeleteConfirmation(this);
    }

    // Sorts a list of design file ui based on their last modified time using a bubble sort algorithm
    public static void SortDesignFileUIList(List<DesignFileUI> list)
    {
        list.Sort((p, q) => (int)(q.LastModified - p.LastModified).TotalMinutes);
    }
}
