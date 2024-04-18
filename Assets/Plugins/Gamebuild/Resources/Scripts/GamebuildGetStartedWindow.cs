#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Gamebuild.Feedback;


[InitializeOnLoad]
public class GamebuildGetStartedWindow : EditorWindow
{
    // Serialized fields
    [SerializeField] private VisualTreeAsset _tree;

    // Private fields
    private TextField tokenField;
    private Button step1Button;
    private Button step2Button;
    private Button getStartedButton;
    private Button discordButton;
    private Button feedbackButton;
    private Button step3Button;
    private Button step4Button;

    // Public properties
    public static GamebuildGetStartedWindow Instance { get; private set; }

    private void OnEnable()
    {
        Instance = this;
    }

    static GamebuildGetStartedWindow()
    {
        EditorApplication.update += RunOnce;
    }

    static void RunOnce()
    {
        EditorApplication.update -= RunOnce;

        if (!EditorPrefs.GetBool("GamebuildGetStartedShown", false))
        {
            EditorPrefs.SetBool("GamebuildGetStartedShown", true);
            ShowGetStarted();
        }
    }

    // Show the Get Started window
    [MenuItem("Gamebuild/Get Started")]
    public static void ShowGetStarted()
    {
        var window = GetWindow<GamebuildGetStartedWindow>();
        window.minSize = new Vector2(400, 370);
        window.maxSize = new Vector2(470, 400);
        window.titleContent = new GUIContent("Get Started with Gamebuild");
    }

    // Create GUI and assign button click handlers
    private void CreateGUI()
    {
        _tree.CloneTree(rootVisualElement);

        // Find buttons and text field
        step1Button = rootVisualElement.Q<Button>("step1-button");
        step2Button = rootVisualElement.Q<Button>("step2-button");
        step3Button = rootVisualElement.Q<Button>("step3-button");
        step4Button = rootVisualElement.Q<Button>("step4-button");
        discordButton = rootVisualElement.Q<Button>("discord-button");
        feedbackButton = rootVisualElement.Q<Button>("feedback-button");
        getStartedButton = rootVisualElement.Q<Button>("getstarted-button");
        tokenField = rootVisualElement.Q<TextField>("token-field");

        // Assign button click handlers
        step1Button.clicked += OnStep1Pressed;
        step2Button.clicked += OnStep2Pressed;
        step3Button.clicked += OnStep3Pressed;
        step4Button.clicked += OnStep4Pressed;
        getStartedButton.clicked += OnGetStartedPressed;
        discordButton.clicked += OnDiscordPressed;
        feedbackButton.clicked += OnFeedbackPressed;

        // Assign text field value changed handler
        //tokenField.RegisterValueChangedCallback(HandleToken);
    }

    // Handle token value change
    void HandleToken(ChangeEvent<string> value)
    {
        if (GameObject.Find("Gamebuild Feedback Canvas"))
        {
            GameObject.Find("Gamebuild Feedback Canvas").GetComponent<GamebuildFeedback>().gamebuildToken = value.newValue;
        }
    }

    // Button click handlers
    public void OnStep1Pressed() => Application.OpenURL("https://www.gamebuild.io/new-signup");
    public void OnStep2Pressed()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab Gamebuild Feedback Canvas");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab && PrefabUtility.GetCorrespondingObjectFromSource(obj).name == prefab.name)
                    {
                        Debug.LogWarning("Prefab already exists in the scene.");
                        return;
                    }
                }

                Debug.Log("Found Prefab: " + prefab.name);
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            }
        }
    }
    public void OnStep3Pressed() => Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/get-started/copying-your-build-token");
    public void OnStep4Pressed() => Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/get-started/unity-quick-start-guide");
    private void OnGetStartedPressed() => Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/");
    private void OnDiscordPressed() => Application.OpenURL("https://discord.gg/Mxw8mMzURA");
    private void OnFeedbackPressed() => Application.OpenURL("https://tally.so/r/w2XMOe");
}

#endif
