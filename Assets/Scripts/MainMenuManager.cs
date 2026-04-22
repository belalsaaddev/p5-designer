using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    [SerializeField] private GameObject designFileUIPrefab;
    [SerializeField] private RectTransform designFileUIContainer;
    [SerializeField] private ScrollRect designFilesScrollRect;
    [SerializeField] private TMP_InputField newDesignNameInput;
    [SerializeField] private TMP_Text designNameText;
    [SerializeField] private Color defaultDesignNameColor;
    [SerializeField] private Color errorDesignNameColor;
    [SerializeField] private GameObject deletePanel;
    [SerializeField] private TMP_Text deleteText;
    private DesignFileUI designUIToDelete;
    private readonly List<DesignFileUI> designFileUIs = new();

    float designUIHeight;
    float minContainerHeight;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        designUIHeight = designFileUIPrefab.GetComponent<RectTransform>().rect.height;
        minContainerHeight = designFileUIContainer.sizeDelta.y;
        LoadAllDesigns();
    }

    // Loads all designs from CanvasManager and creates UI object for them
    void LoadAllDesigns()
    {
        Design[] designs = CanvasManager.AllDesigns;
        foreach (Design design in designs)
        {
            // Load each design
            GameObject designUI = Instantiate(designFileUIPrefab, designFileUIContainer);
            // Set up the designUI with the design data
            DesignFileUI ui = designUI.GetComponent<DesignFileUI>();
            ui.Setup(design);
            designFileUIs.Add(ui);
        }

        SortAndOrderFileUI();

        UpdateDesignContainer();
    }

    public void OpenDesign(Design design)
    {
        CanvasManager.EditDesign(design);
        LoadEditScene();
    }

    // Asks user for confirmation before deleting
    public void PromptDeleteConfirmation(DesignFileUI designUI)
    {
        designUIToDelete = designUI;

        deleteText.text = $"Are you sure you want to delete\n\"{designUI.design.Name}\"";
        deletePanel.SetActive(true);
    }
    // Deletes design file from storage
    public void DeleteDesign()
    {
        CanvasManager.DeleteDesignFile(designUIToDelete.design);
        Destroy(designUIToDelete.gameObject);
        designFileUIs.Remove(designUIToDelete);
        UpdateDesignContainer();
    }
    // Sorts all design file uis and orders them in the child hierarchy of their holder
    void SortAndOrderFileUI()
    {
        DesignFileUI.SortDesignFileUIList(designFileUIs);

        for (int i = 0; i < designFileUIs.Count - 1; i++)
        {
            for (int j = i; j < designFileUIs.Count; j++)
            {
                Transform currentChild = designFileUIContainer.GetChild(j);
                if (currentChild.GetComponent<DesignFileUI>() == designFileUIs[i])
                {
                    currentChild.SetSiblingIndex(i);
                    break;
                }
            }
        }
    }
    // Adjusts design container height to fit all its children
    void UpdateDesignContainer()
    {
        int designCount = designFileUIs.Count;
        float spacing = designFileUIContainer.GetComponent<VerticalLayoutGroup>().spacing;
        float totalHeight = Mathf.Max(designCount * (designUIHeight + spacing) - spacing, minContainerHeight);
        designFileUIContainer.sizeDelta = new Vector2(designFileUIContainer.sizeDelta.x, totalHeight);
        // Making scroll rect snap to the top
        designFilesScrollRect.verticalNormalizedPosition = 1;
    }
    // Creates a design if and only if the name is valid
    public void TryCreateDesign()
    {
        string name = newDesignNameInput.text;
        if (name == string.Empty || ContainsSpecialCharacters(name)) return;
        while (name[0] == ' ')
        {
            name = name.Substring(1);
            if (name == string.Empty) return;
        }

        Design design = new Design(name);
        CanvasManager.EditDesign(design);
        CanvasManager.Save();
        LoadEditScene();
    }
    // Colors the text differently if the string is invalid
    public void CheckForNamingErrors(string name)
    {
        //Checking for special characters in the name, if any are found display error and return
        if (ContainsSpecialCharacters(name))
        {
            designNameText.color = errorDesignNameColor;
        }
        else designNameText.color = defaultDesignNameColor;
    }
    bool ContainsSpecialCharacters(string name)
    {
        const string specialChars = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
        return name.IndexOfAny(specialChars.ToCharArray()) != -1;
    }
    void LoadEditScene()
    {
        SceneManager.LoadScene(1);
    }
}
