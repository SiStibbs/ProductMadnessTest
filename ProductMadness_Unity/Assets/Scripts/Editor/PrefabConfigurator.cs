using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Cards
{
    /// <summary>
    /// Editor Window for the Prefab Configurator
    /// </summary>
    public class PrefabConfigurator : EditorWindow
    {
        private DefaultAsset targetFolder = null;
        private string targetPath = null;

        private string[] configItemNames;
        private Dictionary<string, ConfigData.ConfigDataItem> configDict;

        private List<string> prefabList = new List<string>();
        private Dictionary<string, GameObject> prefabDict;

        private int[] selectedIndex;
        private string invalidPrefabName;

        private Dictionary<string, ConfigData.ConfigDataItem> configCacheDict = new Dictionary<string, ConfigData.ConfigDataItem>();

        [MenuItem("Config/Prefabs")]
        public static void ShowWindow()
        {
            GetWindow(typeof(PrefabConfigurator));
        }

        /// <summary>
        /// Parse config data
        /// </summary>
        public void LookupConfig()
        {
            TextAsset targetFile = Resources.Load<TextAsset>("data");
            string json = targetFile.text;
            ConfigData configData = JsonUtility.FromJson<ConfigData>("{\"items\":" + json + "}");
            configItemNames = new string[configData.items.Count];
            configDict = new Dictionary<string, ConfigData.ConfigDataItem>();
            for (int i = 0; i < configData.items.Count; i++)
            {
                ConfigData.ConfigDataItem item = configData.items[i];
                configItemNames[i] = item.text;
                configDict.Add(item.text, item);
            }
        }

        /// <summary>
        /// Update GUI
        /// </summary>
        void OnGUI()
        {
            if (configDict == null || configDict.Count == 0)
                LookupConfig();

            GUILayout.Label("Prefab Configurator", EditorStyles.boldLabel);

            ShowFolderSelect();
            GUILayout.Space(10);

            if (GUILayout.Button("Lookup Prefabs"))
            {
                LookupPrefabs();
            }

            GUILayout.Space(10);
            if (prefabDict != null && prefabDict.Count > 0)
                DisplayPrefabs();
        }

        /// <summary>
        /// Create a folder select section in the GUI
        /// </summary>
        private void ShowFolderSelect()
        {
            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                 "Select Folder",
                 targetFolder,
                 typeof(DefaultAsset),
                 false);

            if (targetFolder != null)
            {
                targetPath = AssetDatabase.GetAssetPath(targetFolder);
                EditorGUILayout.HelpBox(
                    "Folder path: " + targetPath,
                    MessageType.Info,
                    true);
            }
            else
            {
                targetPath = null;
            }
        }

        /// <summary>
        /// Find all prefabs on the given path
        /// </summary>
        private void LookupPrefabs()
        {
            prefabDict = new Dictionary<string, GameObject>();
            string[] allPrefabs = GetAllPrefabs();
            prefabList = new List<string>();
            foreach (string prefab in allPrefabs)
            {
                Object o = AssetDatabase.LoadMainAssetAtPath(prefab);
                GameObject go;
                try
                {
                    go = (GameObject)o;
                    if (go.GetComponentInChildren<Text>() != null)
                    {
                        prefabList.Add(prefab);
                        prefabDict.Add(prefab, go);
                    }
                }
                catch
                {
                    Debug.Log("Prefab " + prefab + " won't cast to GameObject");
                }
            }
            selectedIndex = new int[allPrefabs.Length];
        }

        /// <summary>
        /// Show a list of prefabs in the GUI
        /// </summary>
        private void DisplayPrefabs()
        {
            int i = 0;
            foreach (string prefab in prefabList)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(prefab, EditorStyles.boldLabel);
                selectedIndex[i] = EditorGUILayout.Popup(selectedIndex[i], configItemNames);
                if (GUILayout.Button("Apply"))
                {
                    ApplyConfig(prefab, i);
                }

                if (configCacheDict.ContainsKey(prefab))
                {
                    if (GUILayout.Button("Revert"))
                    {
                        RevertPrefab(prefab);
                    }
                }

                EditorGUILayout.EndHorizontal();
                i++;
            }
            if (!string.IsNullOrEmpty(invalidPrefabName))
            {
                EditorGUILayout.HelpBox(
                        string.Format("Prefab {0} may be incorrectly configured.", invalidPrefabName),
                        MessageType.Warning,
                        true);
            }
        }

        /// <summary>
        /// Apply the config at the selected index to the prefab
        /// </summary>
        /// <param name="prefab">The prefab to update</param>
        /// <param name="index">The config index</param>
        private void ApplyConfig(string prefab, int index)
        {
            ConfigData.ConfigDataItem config = configDict[configItemNames[selectedIndex[index]]];
            GameObject go = prefabDict[prefab];
            Text text = go.GetComponent<Text>();
            Image image = go.GetComponentInChildren<Image>();

            if (text == null || image == null)
            {
                invalidPrefabName = prefab;
            }
            else
            {
                invalidPrefabName = null;

                if (!configCacheDict.ContainsKey(prefab))
                {
                    ConfigData.ConfigDataItem cache = new ConfigData.ConfigDataItem()
                    {
                        text = text.text,
                        sprite = image.sprite,
                        colour = text.color
                    };
                    configCacheDict.Add(prefab, cache);
                }

                text.text = config.text;
                ColorUtility.TryParseHtmlString(config.color, out Color colour);
                text.color = colour;

                Sprite sprite = Resources.Load<Sprite>(config.image.Substring(0, config.image.LastIndexOf('.')));
                image.sprite = sprite;
            }

            EditorUtility.SetDirty(go);
        }

        /// <summary>
        /// Revert the prefab to its initial state
        /// </summary>
        /// <param name="prefab">The prefab to revert</param>
        /// <param name="index">the </param>
        private void RevertPrefab(string prefab)
        {
            ConfigData.ConfigDataItem cache = configCacheDict[prefab];
            GameObject go = prefabDict[prefab];
            Text text = go.GetComponent<Text>();
            Image image = go.GetComponentInChildren<Image>();

            text.text = cache.text;
            text.color = cache.colour;
            image.sprite = cache.sprite;

            EditorUtility.SetDirty(go);
        }

        /// <summary>
        /// Lookup all prefabs on the target path
        /// </summary>
        /// <returns>An array of prefab paths</returns>
        private string[] GetAllPrefabs()
        {
            string[] guids = targetPath != null ? AssetDatabase.FindAssets("t:prefab", new string[] { targetPath }) : AssetDatabase.FindAssets("t:prefab");
            List<string> result = new List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path != null)
                    result.Add(path);
            }
            return result.ToArray();
        }
    }
}