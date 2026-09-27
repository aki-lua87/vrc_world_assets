using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace aki_lua87.AssetsListGenerator
{

    [Serializable]
    public class AssetItem
    {
        public string assetTitle;
        public string assetAuthor;
        public string assetURL;
    }

    [Serializable]
    public class AssetCategory
    {
        public string categoryName;
        public AssetItem[] assets;
    }

    [Serializable]
    public class AssetsData
    {
        public string assetTitle;
        public string assetAuthor;
        public string assetURL;
    }
}
