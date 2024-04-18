#if UNITY_EDITOR

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;
using Gamebuild.Feedback;
using System.Collections.Generic;

namespace Gamebuild.Scripts
{
    public class GameBuildEditorWindow : EditorWindow
    {

        public static GameBuildEditorWindow Instance { get; private set; }

        private void OnEnable()
        {
            Instance = this;
        }


        private static GameBuildConfig data;
        private static string defaultPath = GameBuildBuilder.defaultPath;


        [SerializeField] private VisualTreeAsset _tree;

        //Token
        private TextField tokenField;

        //Sounds toggle
        private DropdownField soundsToggle;
        private DropdownField feedbackRemindersToggle;
        private DropdownField publishedBuildsOnly;
        private TextField feedbackKey;
        private TextField playtestStartMessage;
        private TextField reminderTimer;
        private Toggle handleInputToggle;

        private Button dashboardButton;
        private Button addGamebuildCanvasButton;
        private Button viewGamebuild;
        private Button docsButton;
        private Button guidesButton;
        private Button copyLinkButton;
        private Button getStartedButton;
        private Button discordButton;
        private Button feedbackButton;
        private Button buildButton;
        private Button resetStartMessage;

        private static string api_url = "https://app.gamebuild.io/";
        private List<string> toggleOptions = new List<string> { "Off", "On"};

        public string copylink;


        [MenuItem("Gamebuild/Settings")]
        public static void ShowEditor()
        {
            var window = GetWindow<GameBuildEditorWindow>();
            window.titleContent = new GUIContent("Gamebuild");
        }

        

        [MenuItem("Gamebuild/Dashboard")]
        public static void GoToDashboard()
        {
            Application.OpenURL("https://www.gamebuild.io/gated/dashboard");
        }

        [MenuItem("Gamebuild/Documentation")]
        public static void GoToDocs()
        {
            Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/");
        }

        [MenuItem("Gamebuild/Support")]
        public static void GoToDiscord()
        {
            Application.OpenURL("https://discord.gg/zfHMEnJMgn");
        }

        private void CreateGUI()
        {
            if (_tree == null)
            {
                Debug.LogWarning("Can't open the Gamebuild settings while in play mode.");
                return;
            }
                
            
            _tree.CloneTree(rootVisualElement);

            
            dashboardButton = rootVisualElement.Q<Button>("dashboard-button");
            docsButton = rootVisualElement.Q<Button>("docs-button");
            guidesButton = rootVisualElement.Q<Button>("guides-button");
            getStartedButton = rootVisualElement.Q<Button>("getstarted-button");
            discordButton = rootVisualElement.Q<Button>("discord-button");
            feedbackButton = rootVisualElement.Q<Button>("feedback-button");
            resetStartMessage = rootVisualElement.Q<Button>("reset-playtest-start-message");
            soundsToggle = rootVisualElement.Q<DropdownField>("sound-toggle");
            soundsToggle.choices = toggleOptions;
            publishedBuildsOnly = rootVisualElement.Q<DropdownField>("only-published-builds");
            publishedBuildsOnly.choices = toggleOptions;
            handleInputToggle = rootVisualElement.Q<Toggle>("handle-feedback-input");

            reminderTimer = rootVisualElement.Q<TextField>("feedback-reminder-timer");

            feedbackKey = rootVisualElement.Q<TextField>("feedback-key");
            feedbackRemindersToggle = rootVisualElement.Q<DropdownField>("reminder-toggle");
            feedbackRemindersToggle.choices = toggleOptions;
            playtestStartMessage = rootVisualElement.Q<TextField>("playtest-start-message");
            tokenField = rootVisualElement.Q<TextField>("TokenText");
            viewGamebuild = rootVisualElement.Q<Button>("view-gamebuild");
            buildButton = rootVisualElement.Q<Button>("build-button");
            addGamebuildCanvasButton = rootVisualElement.Q<Button>("add-gamebuild-canvas");
            handleInputToggle.RegisterValueChangedCallback(evt => OnHandleInputTogglePressed(evt.newValue));
            addGamebuildCanvasButton.clicked += OnAddGamebuildCanvasPressed;
            buildButton.clicked += OnBuildPressed;
            viewGamebuild.clicked += OnViewGamebuildPressed;
            dashboardButton.clicked += OnDashboardPressed;
            docsButton.clicked += OnDocsPressed;
            guidesButton.clicked += OnGuidesPressed;
            soundsToggle.RegisterValueChangedCallback(evt => OnSoundOptionChanged(evt.newValue));
            feedbackRemindersToggle.RegisterValueChangedCallback(evt => OnReminderChanged(evt.newValue));
            playtestStartMessage.RegisterValueChangedCallback(evt => OnPlaytestStartMessage(evt.newValue));
            resetStartMessage.clicked += OnResetStartMessagePressed;
            publishedBuildsOnly.RegisterValueChangedCallback(evt => OnPublishedBuildsOnlyChanged(evt.newValue));
            feedbackKey.RegisterValueChangedCallback(evt => OnFeedbackKeyChanged(evt.newValue));
            reminderTimer.RegisterValueChangedCallback(evt => OnFeedbackReminderTimerChanged(evt.newValue));
            tokenField.RegisterValueChangedCallback(evt => HandleToken(evt.newValue));

            init();
        }


        private void updateUI()
        {
            if (CheckForGamebuildCanvas())
            {
                Debug.Log("Gamebuild Canvas found");
                tokenField.style.display = DisplayStyle.Flex;
            }
            else
            {
                Debug.Log("Gamebuild Canvas not found");
                tokenField.style.display = DisplayStyle.None;
            }
        }
        
        private async void init()
        {
            //LoadEditorPrefs();

            updateUI();

                if (tokenField.value.Length == 64)
            {
                copylink = await VALIDATE_TOKEN(tokenField.value);
                if (copylink.Length > 0)
                {
                    ShowCopyLink();
                }
                else
                {
                    HideCopyLink();
                }
                ShowCopyLink();
            }
            else
            {
                HideCopyLink();
            }
        }
        
        //method to check if gamebuild canvas exists in scene
        private bool CheckForGamebuildCanvas()
        {
            bool exists = false;
            GameObject gameObject = GameObject.Find("Gamebuild Feedback Canvas");
            if (gameObject != null)
            {
                exists = true;
            }
            return exists;
        }

        private void SaveEditorPrefs(string editorPrefName, string editorPrefValue)
        {
            //EditorPrefs.SetString(editorPrefName, editorPrefValue);


            // Add other fields as needed
        }

        private void OnDashboardPressed()
        {
            Application.OpenURL("https://www.gamebuild.io/gated/dashboard");
        }

        private void OnDocsPressed()
        {
            Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/");
        }

        private void OnGuidesPressed()
        {
            Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/");
        }
        

        private async void HandleToken(string newValue) 
        {
            GameObject gamebuildCanvas = GameObject.Find("Gamebuild Feedback Canvas");
            if (gamebuildCanvas != null)
            {
                GamebuildFeedback gb = gamebuildCanvas.GetComponent<GamebuildFeedback>();
                // Debug.Log("gb value:" +  newValue, gb);
                gb.gamebuildToken = newValue;
                
                if (PrefabUtility.IsPartOfPrefabInstance(gb))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gb);
                }
                // EditorUtility.SetDirty(gb);
                // EditorSceneManager.MarkSceneDirty(gb.gameObject.scene);
                
            }
            
            
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas is to set the token.");
                return;
            }

            //Check length of value
            if (newValue.Length == 64)
            {
                copylink = await VALIDATE_TOKEN(newValue);
                if (copylink.Length > 0)
                {
                    ShowCopyLink();
                }
                else
                {
                    HideCopyLink();
                }
            }
            else
            {
                HideCopyLink();
            }

        }

        public void OnFeedbackKeyChanged(string newValue)
        {
            GameObject feedbackCanvas = GameObject.Find("Gamebuild Feedback Canvas");
            if (feedbackCanvas != null)
            {
                try
                {
                    feedbackCanvas.GetComponent<GamebuildFeedback>().cornerPopUpInputKey = (KeyCode)Enum.Parse(typeof(KeyCode), newValue);

                    updatePrefab();
                    SaveEditorPrefs("FeedbackKey", newValue);
                    Debug.LogWarning("Your key works!");

                }
                catch
                {
                    Debug.LogWarning("There may be an issue with your feedback key, verify it is correct");
                }
                
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas to have these changes apply.");
                return;
            }
        }

        public void OnHandleInputTogglePressed(bool newVal)
        {
            GameObject feedbackCanvas = GameObject.Find("Gamebuild Feedback Canvas");
            if (feedbackCanvas != null)
            {
                try
                {
                    feedbackCanvas.GetComponent<GamebuildFeedback>().handleInput = newVal;
                    
                    if (newVal == true)
                    {
                        Debug.Log("You're now handling input for the feedback panel.");
                    }
                    else
                    {
                        Debug.Log("You are no longer handling input for the feedback panel.");
                    }

                }
                catch
                {
                    Debug.LogWarning("Something went wrong when changing the handle input toggle.");
                }
                
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas to have these changes apply.");
                return;
            }
        }

        private void updatePrefab()
        {
            GameObject feedbackCanvas = GameObject.Find("Gamebuild Feedback Canvas");
            if (feedbackCanvas != null)
            {
                GamebuildFeedback gb = feedbackCanvas.GetComponent<GamebuildFeedback>();
                if (PrefabUtility.IsPartOfPrefabInstance(gb))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gb);
                }
            }   
            
            
        }
        
        public void OnFeedbackReminderTimerChanged(string newValue)
        {
            ReminderController rc;
            GameObject feedbackCanvas = GameObject.Find("Gamebuild Feedback Canvas");
           if (feedbackCanvas != null)
            {
                try
                {
                    rc = FindObjectOfType<ReminderController>(true);
                    rc.showReminderEvery = float.Parse(newValue);
                    SaveEditorPrefs("ReminderTimer", reminderTimer.value);
                    updatePrefab();
                    Debug.Log("Your timer value works!");

                }
                catch
                {
                    Debug.LogWarning("There may be an issue with your timer value, verify it is correct");
                }

            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas to have these changes apply.");
                return;
            }
        }

        public void OnResetStartMessagePressed()
        {
            GameStartPanelController gameStartPanelController;
            if (GameObject.Find("Gamebuild Feedback Canvas"))
            {
                gameStartPanelController = FindObjectOfType<GameStartPanelController>(true);
                string defaultMessage = "Hey there,\n \nThanks for taking the time to check out this playtest.";
                gameStartPanelController.startingTextMessage = defaultMessage;
                playtestStartMessage.value = defaultMessage;
                updatePrefab();
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas to have these changes apply.");
            }
        }

        public void OnAddGamebuildCanvasPressed()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab Gamebuild Feedback Canvas");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    GameObject[] allObjects = FindObjectsOfType<GameObject>();
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
            updateUI();
        }

        public void OnBuildPressed()
        {
            Debug.Log("Building");
            BuildAndZip();
        }

        private void OnPlaytestStartMessage(string newValue)
        {
            GameStartPanelController gameStartPanelController;
            if (GameObject.Find("Gamebuild Feedback Canvas"))
            {
                gameStartPanelController = FindObjectOfType<GameStartPanelController>(true);
                gameStartPanelController.startingTextMessage = newValue;
                SaveEditorPrefs("PlaytestStartMessage", playtestStartMessage.value);
                updatePrefab();
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas to your scene or swap to the scene where your Gamebuild Feedback Canvas to have these changes apply.");
                return;
            }
        }

        private void OnViewGamebuildPressed()
        {
            if (tokenField.value.Length == 64)
            {
                if (!String.IsNullOrEmpty(copylink))
                {
                    Debug.Log("CopyLinkPressed");
                    Application.OpenURL("www.gamebuild.io/build?displayToken=" + copylink);
                }
            }
            else
            {
                Debug.Log("Please provide a token first");
                return;
            }

        }


        private void OnPublishedBuildsOnlyChanged(string newValue)
        {
            GamebuildFeedback gbFeedback;
            if (GameObject.Find("Gamebuild Feedback Canvas"))
            {
                gbFeedback = GameObject.Find("Gamebuild Feedback Canvas").GetComponent<GamebuildFeedback>();
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas for these changes to be made.");
                return;
            }
            switch (newValue)
            {
                case "Off":
                    gbFeedback.PublishedBuildsOnly = false;
                    break;
                case "On":
                    gbFeedback.PublishedBuildsOnly = true;
                    break;
            }
            updatePrefab();
            SaveEditorPrefs("PublishedBuildsOnly", newValue);
        }
        private void OnSoundOptionChanged(string newValue)
        {
            GamebuildFeedback gbFeedback;
            if (GameObject.Find("Gamebuild Feedback Canvas"))
            {
                gbFeedback = GameObject.Find("Gamebuild Feedback Canvas").GetComponent<GamebuildFeedback>();
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas for these changes to be made.");
                return;
            }
            switch (newValue)
            {
                case "Off":
                    // Turn off chimes
                    gbFeedback.playChime = false;
                    break;
                case "On":
                    // Turn on chimes
                    gbFeedback.playChime = true;
                    break;
            }
            updatePrefab();
            SaveEditorPrefs("SoundsToggle", soundsToggle.value);
        }

        private void OnReminderChanged(string newValue)
        {
            ReminderController reminderController;
            if (GameObject.Find("Gamebuild Feedback Canvas"))
            {
                reminderController = FindObjectOfType<ReminderController>(true);
            }
            else
            {
                Debug.LogWarning("Add the Gamebuild Feedback Canvas for these changes to be made.");
                return;
            }
            switch (newValue)
            {
                case "Off":
                    // Turn off chimes
                    reminderController.ReminderEnabled = false;
                    break;
                case "On":
                    // Turn on chimes
                    reminderController.ReminderEnabled = true;
                    break;
            }
            updatePrefab();
            SaveEditorPrefs("FeedbackRemindersToggle", feedbackRemindersToggle.value);
        }


        private void OnGetStartedPressed()
        {
            Debug.Log("OnGetStartedPressed");
            Application.OpenURL("https://gamebuild.gitbook.io/gamebuild.io/");

        }

        private void OnDiscordPressed()
        {
            Debug.Log("OnDiscordPressed");
            Application.OpenURL("https://discord.gg/Mxw8mMzURA");
        }

        private void OnFeedbackPressed()
        {
            Debug.Log("OnFeedbackPressed");
            Application.OpenURL("https://tally.so/r/w2XMOe");
        }


        public void ShowCopyLink()
        {
            viewGamebuild.SetEnabled(true);
            buildButton.SetEnabled(true);
        }

        public void HideCopyLink()
        {
            viewGamebuild.SetEnabled(false);
            buildButton.SetEnabled(false);
        }

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            // if no data exists yet create and reference a new instance
            if (!data)
            {
                // as first option check if maybe there is an instance already
                // and only the reference got lost
                // won't work ofcourse if you moved it elsewhere ...
                data = AssetDatabase.LoadAssetAtPath<GameBuildConfig>("Assets/Plugins/Gamebuild/Gamebuildconfig.asset");
                // if that was successful we are done
                if (data) return;

                // otherwise create and reference a new instance
                data = CreateInstance<GameBuildConfig>();

                AssetDatabase.CreateAsset(data, "Assets/Plugins/Gamebuild/Gamebuildconfig.asset");
                AssetDatabase.Refresh();
            }
        }

        public string UrlEncode(string str)
        {
            if (str == null || str == "")
            {
                return null;
            }

            byte[] bytesToEncode = System.Text.UTF8Encoding.UTF8.GetBytes(str);
            String returnVal = System.Convert.ToBase64String(bytesToEncode);
            return returnVal.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }


        private async void BuildAndZip()
        {

            try
            {
                if (tokenField == null || String.IsNullOrEmpty(tokenField.value))
                {
                    Debug.LogError("Open the gamebuild settings window and set your token");
                    return;
                }


                GameBuildBuilder.BuildServer(false);
                EditorUtility.DisplayProgressBar("Gamebuild", "Zipping Files", 0.4f);
                string zipFile = GameBuildBuilder.ZipServerBuild(copylink);

                string directoryToZip = Path.GetDirectoryName(defaultPath);
                string targetfile = Path.Combine(directoryToZip, @".." + Path.DirectorySeparatorChar + copylink + ".zip");
                EditorUtility.DisplayProgressBar("Gamebuild", "Uploading Files", 0.75f);
                string projectname = UnityEditor.PlayerSettings.productName;
                string studioname = UnityEditor.PlayerSettings.companyName;

                string upload_url = await GET_UPLOAD_URL(tokenField.value, projectname, studioname);

                Debug.Log(upload_url);

                Upload(targetfile, upload_url, tokenField.value, projectname, studioname);


                //PlayFlowBuilder.cleanUp(zipFile);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void Upload(string fileLocation, string upload_url, string token, string projectname, string studioname)
        {
            try
            {
                Uri actionUrl = new Uri(upload_url);

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/zip");
                    client.UploadFile(actionUrl, "PUT", fileLocation);
                    Debug.Log("File uploaded successfully");

                    // If upload is success
                    SUCCESS(token, projectname, studioname);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public static async Task<string> VALIDATE_TOKEN(string token)
        {
            string output = "";
            try
            {
                string actionUrl = api_url + "validate_token";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("token", token);
                    HttpResponseMessage response = await client.GetAsync(actionUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        output = await response.Content.ReadAsStringAsync();
                        output = output.Trim('"');
                        return output;
                    }
                    else
                    {
                        Debug.Log($"Invalid Token: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            //Escape output string
            return output;
        }



        public static async Task<string> SUCCESS(string token, string projectname, string studioname)
        {
            string output = "";
            try
            {
                string actionUrl = api_url + "success";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("token", token);
                    client.DefaultRequestHeaders.Add("projectname", projectname);
                    client.DefaultRequestHeaders.Add("studioname", studioname);

                    HttpResponseMessage response = await client.GetAsync(actionUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        output = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            //Escape output string
            output = output.Trim('"');
            return output;
        }

        public static async Task<string> GET_UPLOAD_URL(string token, string projectname, string studioname)
        {
            string output = "";
            try
            {
                string actionUrl = api_url + "upload_url";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("token", token);
                    client.DefaultRequestHeaders.Add("projectname", projectname);
                    client.DefaultRequestHeaders.Add("studioname", studioname);

                    HttpResponseMessage response = await client.GetAsync(actionUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        output = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            //Escape output string
            output = output.Trim('"');
            return output;
        }

        private void OnGUI()
        {
        }

        public void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}

#endif

