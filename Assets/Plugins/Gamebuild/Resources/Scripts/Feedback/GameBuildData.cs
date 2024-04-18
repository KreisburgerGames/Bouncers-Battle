using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;


namespace Gamebuild.Feedback
{
    public class GameBuildData : MonoBehaviour
    {
        [HideInInspector]
        public string screenshotName = "Screenshot.png";
        private string screenshotDirectory;
        public static string postUrl = "https://app.gamebuild.io/";
        [HideInInspector]
        public string screenshotPath = "";
        public GamebuildFeedback GamebuildFeedback;
        public bool initalizedSuccessfully = false;
        public GameStartPanelController StartPanelController;
        [Serializable]
        public class PlayerInfo
        {
            public string PlayerName;
            public string PlayerImage;
        }

        [Serializable]
        public class SessionInfo
        {
            public string DeviceModel;
            public string OperatingSystem;
            public int ProcessorCount;
            public string ProcessorType;
            public int SystemMemorySize;
            public int GraphicsMemorySize;
            public string ScreenResolution;
            public string QualitySettings;
            public PlayerInfo PlayerInfo;
            public bool isEditor;
        }

        [Serializable]
        public class Feedback
        {
            public string sessionID;
            public string CurrentUTCTime;
            public string FeedbackJson;
        }

        [Serializable]
        public class Moment
        {
            public string time;
            public string sceneID;
            public string description;
            public string playerAction;
            public string playerState;
            public string conditions;
        }

        [Serializable]
        public class GameSession
        {
            public string sessionID;
            public string videoUrl;
            public SessionInfo sessionInfo;
            // public Feedback[] feedbacks;
            // public Moment[] moments;
            // public string logsUrl;
        }
        
        [Serializable]
        public class SessionResponse
        {
            public string display_token;
            public string session_id;
            public string game_video_url;

        }
        
        public SessionResponse sessionResponse = new SessionResponse();
        
        
        private GameSession currentGameSession = new GameSession();

        private void validateGameBuildToken()
        {
            if (GamebuildFeedback.gamebuildToken == null)
            {
                Debug.LogError("Gamebuild Token is null. Please set the Gamebuild Token in the Gamebuild Feedback Settings");
            }
        }

        // void Awake()
        // {
        //     validateGameBuildToken();
        //     cameraCapture = GetComponent<CameraCapture>();
        //     
        // //Wait for 10 seconds and then call endsession
        //     StartCoroutine(EndSession(5));
        // }
        //
        // IEnumerator EndSession(int seconds)
        // {
        //     yield return new WaitForSeconds(seconds);
        //     cameraCapture.EndSession();
        // }


        IEnumerator CreateSessionInfo(string jsonData, Action callback)
        {
            string endpoint_url = postUrl + "create_new_session";
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

            // Add data to the formData before creating the UnityWebRequest
            formData.Add(new MultipartFormDataSection("json", jsonData));

            UnityWebRequest request = UnityWebRequest.Post(endpoint_url, formData);
            request.SetRequestHeader("token", GamebuildFeedback.gamebuildToken);

            try
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("POST Error: " + request.error);
                }
                else
                {
                    string response = request.downloadHandler.text;
                    var responseJson = JsonUtility.FromJson<SessionResponse>(response);

                    sessionResponse.display_token = responseJson.display_token;
                    sessionResponse.session_id = responseJson.session_id;
                    sessionResponse.game_video_url = responseJson.game_video_url;

                    initalizedSuccessfully = true;
                    callback();
                }
            }
            finally
            {
                // Dispose of the request
                request?.Dispose();
            }
        }



        void Start()
        {
            if (GamebuildFeedback.IgnoreEditorSessions())
            {
                Debug.Log("Gamebuild Feedback is disabled for this build. Please publish the build to enable Gamebuild Feedback");
                return;
            }
            
            screenshotDirectory = Path.Combine(Application.persistentDataPath, "gamebuild/");
            Directory.CreateDirectory(screenshotDirectory);
            currentGameSession.sessionID = Guid.NewGuid().ToString();
            currentGameSession.sessionInfo = new SessionInfo();
            currentGameSession.sessionInfo.PlayerInfo = new PlayerInfo();
            //System Unique Device ID as Player Name
            currentGameSession.sessionInfo.PlayerInfo.PlayerName = SystemInfo.deviceUniqueIdentifier;

            //Set device info
            currentGameSession.sessionInfo.DeviceModel = SystemInfo.deviceModel;
            currentGameSession.sessionInfo.OperatingSystem = SystemInfo.operatingSystem;
            currentGameSession.sessionInfo.ProcessorCount = SystemInfo.processorCount;
            currentGameSession.sessionInfo.ProcessorType = SystemInfo.processorType;
            currentGameSession.sessionInfo.SystemMemorySize = SystemInfo.systemMemorySize;
            currentGameSession.sessionInfo.GraphicsMemorySize = SystemInfo.graphicsMemorySize;
            currentGameSession.sessionInfo.ScreenResolution = Screen.currentResolution.ToString();
            currentGameSession.sessionInfo.QualitySettings = QualitySettings.names[QualitySettings.GetQualityLevel()];

            //Find the cameara capture component from this gameobject
            //Check if we are in the editor, if so, use the editor flag and add it to the json
            if (Application.isEditor)
            {
                currentGameSession.sessionInfo.isEditor = true;
            }
            else
            {
                currentGameSession.sessionInfo.isEditor = false;
            }

            // Print session info
            string sessionInfoJson = JsonUtility.ToJson(currentGameSession);
            StartCoroutine(CreateSessionInfo(sessionInfoJson, onCreatedSession)); // Remove the parentheses here

            //Start recording
            //Start recording after 5 seconds
            StartCoroutine(StartSession(1));
            StartCoroutine(StartRecording(3));
            
        }
        
        void onCreatedSession()
        {
            StartPanelController.url = sessionResponse.game_video_url;
        }
        
        IEnumerator StartSession(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            StartRecording();

        }
        
        IEnumerator StartRecording(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            // cameraCapture.recording = true;
        }
        
        void StartRecording()
        {
            // cameraCapture = GetComponent<CameraCapture>();
            // Debug.Log(currentGameSession.sessionID);
            // cameraCapture.sessionId = currentGameSession.sessionID;
            // cameraCapture.gamebuildtoken = GamebuildFeedback.gamebuildToken;
        }
        
        
        public void CaptureScreenshot()
        {
            screenshotDirectory = Path.Combine(Application.persistentDataPath, "gamebuild/");
            screenshotPath = Path.Combine(screenshotDirectory, screenshotName);
            ScreenCapture.CaptureScreenshot(screenshotPath);
        }
        
        public void CaptureDataAndSend(string feedback_json, bool isFeedback)
        {
            if (!initalizedSuccessfully)
            {
                Debug.Log("Gamebuild Feedback not initialized successfully");
                return;
            }
            Feedback data = new Feedback();
            data.sessionID = currentGameSession.sessionID;
            data.CurrentUTCTime = DateTime.UtcNow.ToString();
            data.FeedbackJson = feedback_json;
            
            

            string jsonData = JsonUtility.ToJson(data);

            if (!isFeedback)
            {
                CaptureScreenshot();
            }
            
            StartCoroutine(WaitForScreenshot(screenshotPath, jsonData));
        }


        IEnumerator WaitForScreenshot(string path, string jsonData)
        {
            yield return new WaitUntil(() => File.Exists(path));
            byte[] imageBytes = File.ReadAllBytes(path);
            StartCoroutine(PostData(jsonData, imageBytes));
        }

        IEnumerator PostData(string jsonData, byte[] imageBytes)
        {
            string endpoint_url = postUrl + "upload_feedback";
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            //Add headers to the HTTP request

            formData.Add(new MultipartFormDataSection("json", jsonData));
            formData.Add(new MultipartFormFileSection("image", imageBytes, "screenshot.png", "image/png"));

            UnityWebRequest request = UnityWebRequest.Post(endpoint_url, formData);
            request.SetRequestHeader("token", GamebuildFeedback.gamebuildToken);


            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("POST Error: " + request.error);
            }
            else
            {
            }
        }
        
        
    }
}
