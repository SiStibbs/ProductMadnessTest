using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
    /// <summary>
    /// A class to hold prefab config data. 
    /// Structure intended to be used with JsonUtility.
    /// </summary>
    [System.Serializable]
    public class ConfigData
    {
        public List<ConfigDataItem> items;

        [System.Serializable]
        public class ConfigDataItem
        {
            public string text;
            public string color;
            public string image;

            public Sprite sprite;
            public Color colour;
        }
    }
}