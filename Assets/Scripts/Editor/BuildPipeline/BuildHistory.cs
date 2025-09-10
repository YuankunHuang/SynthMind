using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace YuankunHuang.Unity.Editor.BuildPipeline
{
    [System.Serializable]
    public class BuildHistoryEntry
    {
        public string buildVersion;
        public string buildTarget;
        public string buildProfile;
        public string buildPath;
        public DateTime buildTime;
        public TimeSpan buildDuration;
        public long buildSize;
        public BuildResult buildResult;
        public string buildLog;
        public bool wasZipped;
        public bool addressablesBuilt;

        public BuildHistoryEntry(string version, string target, string profile, string path, 
            DateTime time, TimeSpan duration, long size, BuildResult result, string log, 
            bool zipped, bool addressables)
        {
            buildVersion = version;
            buildTarget = target;
            buildProfile = profile;
            buildPath = path;
            buildTime = time;
            buildDuration = duration;
            buildSize = size;
            buildResult = result;
            buildLog = log;
            wasZipped = zipped;
            addressablesBuilt = addressables;
        }
    }

    [System.Serializable]
    public class BuildHistoryData
    {
        public List<BuildHistoryEntry> entries = new List<BuildHistoryEntry>();
        public int maxEntries = 50; // Keep only the last 50 builds
    }

    public static class BuildHistoryManager
    {
        private const string HISTORY_KEY = "SynthMind.BuildPipeline.History";

        public static void AddBuildEntry(BuildHistoryEntry entry)
        {
            var history = LoadHistory();
            history.entries.Insert(0, entry); // Add to the beginning

            // Trim to max entries
            while (history.entries.Count > history.maxEntries)
            {
                history.entries.RemoveAt(history.entries.Count - 1);
            }

            SaveHistory(history);
        }

        public static BuildHistoryData LoadHistory()
        {
            var json = EditorPrefs.GetString(HISTORY_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                return new BuildHistoryData();
            }

            try
            {
                return UnityEngine.JsonUtility.FromJson<BuildHistoryData>(json);
            }
            catch
            {
                return new BuildHistoryData();
            }
        }

        public static void SaveHistory(BuildHistoryData history)
        {
            var json = UnityEngine.JsonUtility.ToJson(history, true);
            EditorPrefs.SetString(HISTORY_KEY, json);
        }

        public static void ClearHistory()
        {
            EditorPrefs.DeleteKey(HISTORY_KEY);
        }
    }
}