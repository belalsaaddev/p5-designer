using UnityEngine;
using System.IO;

public static class SaveSystem
{
    /// <summary>
    /// Makes sure the given folder path exists; if it isn't, it creates it
    /// </summary>
    public static void VerifyAndCreateFolder(string folderPath)
    {
        string path = Application.persistentDataPath + folderPath;
        if (!Directory.Exists(path))
        {
            Debug.LogWarning("Folder not found in " + path + ", creating new folder.");
            Directory.CreateDirectory(path);
        }
    }
    /// <summary>
    /// Saves a string of data to a file of a given name
    /// </summary>
    public static void SaveData(string data, string filePath)
    {
        string path = Application.persistentDataPath + filePath;
        File.WriteAllText(path, data);
    }
    /// <summary>
    /// Loads data of a given type from a given path and returns it
    /// </summary>
    public static T LoadData<T>(string filePath) where T : class, new()
    {
        string path = Application.persistentDataPath + filePath;
        if (File.Exists(path))
        {
            T data = JsonUtility.FromJson<T>(File.ReadAllText(path));
            return data;
        }
        Debug.LogError("Save file not found in" + path);
        return null;
    }
    /// <summary>
    /// Loads all files in a given folder path into an array of a given type
    /// </summary>
    public static T[] LoadAllFilesFromFolder<T>(string folderPath) where T : class, new()
    {
        string[] paths = LoadAllFilePathsInFolder(folderPath);
        T[] data = new T[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            data[i] = LoadData<T>(paths[i]);
        }
        return data;
    }
    /// <summary>
    /// Loads the paths of all files inside the given folder path,
    /// and returns them in a string array
    /// </summary>
    public static string[] LoadAllFilePathsInFolder(string folderPath)
    {
        string[] paths = Directory.GetFiles(Application.persistentDataPath + folderPath);
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = paths[i].Replace(Application.persistentDataPath, "");
        }
        return paths;
    }
    /// <summary>
    /// Returns the file info of the given file's name
    /// </summary>
    public static FileInfo GetFileInfo(string fileName)
    {
        string path = Application.persistentDataPath + fileName;
        if (File.Exists(path))
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo;
        }
        Debug.LogError("File not found: " + path);
        return null;
    }
    /// <summary>
    /// Deletes the file with the given name
    /// </summary>
    public static void DeleteFile(string fileName)
    {
        string path = Application.persistentDataPath + fileName;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
    }
}
