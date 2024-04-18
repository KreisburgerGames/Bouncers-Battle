#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using UnityEngine;

namespace Gamebuild.Scripts
{
    public class GameBuildBuilder
    {
        public static string defaultPath = @"Builds" + Path.DirectorySeparatorChar + "GameBuild" +
                                           Path.DirectorySeparatorChar +
                                           "GameBuild" + Path.DirectorySeparatorChar + "GameBuild" +
                                           Path.DirectorySeparatorChar + "GameBuild" +
                                           Path.DirectorySeparatorChar + "Game.exe";
        public static void BuildServer(bool devmode)
        {

            BuildTarget standaloneTarget = EditorUserBuildSettings.selectedStandaloneTarget;
            BuildTargetGroup currentBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(standaloneTarget);
#if UNITY_2021_2_OR_NEWER
#endif
            try
            {
                EditorUtility.DisplayProgressBar("PlayFlowCloud", "Build Linux Server", 0.25f);
                List<string> scenes = new List<string>();
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                    {
                        scenes.Add(scene.path);
                    }
                }

                BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                buildPlayerOptions.scenes = scenes.ToArray();
                buildPlayerOptions.locationPathName = defaultPath;
                buildPlayerOptions.target = standaloneTarget;
                buildPlayerOptions.targetGroup = currentBuildTargetGroup;

#if UNITY_2021_2_OR_NEWER
                if (Application.unityVersion.CompareTo(("2021.2")) >= 0)
                {
                    if (devmode)
                    {
                        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC | BuildOptions.Development;
                    }
                    else
                    {
                        buildPlayerOptions.options = BuildOptions.CompressWithLz4HC;
                    }
                }
#endif
                BuildPipeline.BuildPlayer(buildPlayerOptions);
            }

            catch (Exception e)
            {
                Debug.Log(e.StackTrace);
            }
            
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static string ZipServerBuild(string token)
        {
            string directoryToZip = Path.GetDirectoryName(defaultPath);
            string zipFile = "";
            if (Directory.Exists(directoryToZip))
            {
                string targetfile = Path.Combine(directoryToZip, @".." + Path.DirectorySeparatorChar + token + ".zip");
                zipFile = ZipPath(targetfile, directoryToZip, null, true, null);
            }

            return zipFile;
        }

        public static string ZipPath(string zipFilePath, string sourceDir, string pattern, bool withSubdirs,
            string password)
        {
            FastZip fz = new FastZip();
            fz.CompressionLevel = Deflater.CompressionLevel.DEFAULT_COMPRESSION;
            fz.CreateZip(zipFilePath, sourceDir, withSubdirs, pattern);
            return zipFilePath;
        }

        public static void cleanUp(string zipFilePath)
        {
            string sourceDir = Path.GetDirectoryName(defaultPath);
            if (Directory.Exists(sourceDir))
            {
                Directory.Delete(sourceDir, true);
            }

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

        }
    }
}

#endif
