using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class AboutWindow : EditorWindow
    {
        private const string SamplesPackagePath = "Assets/Plugins/InsaneScatterbrain/MapGraph.Samples/Samples.unitypackage";
        
        private bool installingSample;
        private List<string> packagesToImport;
        private AddRequest activeSampleInstallRequest;

        [MenuItem("Tools/Map Graph")]
        public static AboutWindow ShowWindow()
        {
            var window = GetWindow<AboutWindow>(true, "Map Graph");
            window.Show();
            return window;
        }
        
        private void OnEnable()
        {
            if (activeSampleInstallRequest != null && !activeSampleInstallRequest.IsCompleted)
            {
                EditorApplication.update += ProcessActiveSampleInstallRequest;
            }
            else
            {
                ProcessSampleInstallRequest();
            }
        }

        public void StartSampleInstallProcess()
        {
            installingSample = true;
            packagesToImport = new List<string>
            {
                "com.unity.2d.tilemap@1.0.0",
                "com.unity.2d.tilemap.extras@1.5.0-preview",
                "com.unity.2d.pixel-perfect@2.0.4"
            };

            StartPrepareSampleInstallation();
        }

        private ListRequest listRequest;

        private void StartPrepareSampleInstallation()
        {
            listRequest = Client.List();
            EditorApplication.update += ProcessPrepareSampleInstallation;
        }

        private void ProcessPrepareSampleInstallation()
        {
            if (!listRequest.IsCompleted) return;

            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError(listRequest.Error.message);
            }
            else
            {
                foreach (var package in listRequest.Result)
                {
                    packagesToImport.Remove(package.packageId);
                }
            }

            EditorApplication.update -= ProcessPrepareSampleInstallation;
            ProcessSampleInstallRequest();
        }
        
        private void ProcessSampleInstallRequest()
        {
            if (!installingSample) return;

            if (packagesToImport.Count > 0)
            {
                activeSampleInstallRequest = Client.Add(packagesToImport[0]);
                EditorApplication.update += ProcessActiveSampleInstallRequest;
            }
            else
            {
                AssetDatabase.ImportPackage(SamplesPackagePath, false);
                AssetDatabase.Refresh();
                installingSample = false;
            }
        }
        
        private void ProcessActiveSampleInstallRequest()
        {
            if (!activeSampleInstallRequest.IsCompleted) return;

            if (activeSampleInstallRequest.Status == StatusCode.Failure)
            {
                Debug.LogError(activeSampleInstallRequest.Error.message);
            }

            EditorApplication.update -= ProcessActiveSampleInstallRequest;
            packagesToImport.RemoveAt(0);
            ProcessSampleInstallRequest();
        }

        private void OnGUI()
        {
            var size = new Vector2(420, 300);
            minSize = size;
            maxSize = size;

            const int columnWidths = 207;
            
            var bigRichButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35, 
                fontSize = 18, 
                richText = true
            };

            var mediumButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 14
            };

            var centerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var headerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 20, 
                fontStyle = FontStyle.Bold, 
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Space(20);

            GUILayout.Label("Thank you for using Map Graph!", headerLabelStyle);
            GUILayout.Space(15);
            GUILayout.Label("If you like Map Graph, please consider leaving a review.\nIt really helps a lot!", centerLabelStyle);
            GUILayout.Space(15);
            
            if (GUILayout.Button("Leave a review <color=#d54930ff>❤</color>", bigRichButtonStyle)) 
            {
                Application.OpenURL(Urls.Reviews);
            }
            
            GUILayout.Space(40);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(columnWidths));
            
            GUILayout.Label("Documentation", headerLabelStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("Getting Started", mediumButtonStyle))
            {
                Application.OpenURL(Urls.GettingStarted);
            }
            if (GUILayout.Button("Manual", mediumButtonStyle))
            {
                Application.OpenURL(Urls.OnlineDocs);
            }

            if (installingSample) GUI.enabled = false;
            var buttonText = installingSample ? "Installing Samples ..." : "Install Samples";
            if (GUILayout.Button(buttonText, mediumButtonStyle))
            {
                if (EditorUtility.DisplayDialog("Install Samples?",
                    "It's recommended to import the samples into an empty project.",
                    "Continue", "Cancel"))
                {
                    StartSampleInstallProcess();
                }

                GUIUtility.ExitGUI();
            }
            if (installingSample) GUI.enabled = true;
            
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical(GUILayout.Width(columnWidths));

            GUILayout.Label("Support", headerLabelStyle);
            GUILayout.Space(5);
            
            if (GUILayout.Button("Twitter", mediumButtonStyle))
            {
                Application.OpenURL(Urls.TwitterProfile);
            }
            
            if (GUILayout.Button("Unity Forums", mediumButtonStyle))
            {
                Application.OpenURL(Urls.UnityForum);
            }

            if (GUILayout.Button("E-mail", mediumButtonStyle))
            {
                Application.OpenURL(Urls.SupportMail);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        } 
    }
}