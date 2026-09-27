using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace aki_lua87.AssetsListGenerator
{
    public class AssetsManagerBehaviour : MonoBehaviour
    {
        [Header("Prefabs(Don't edit)")]
        public GameObject TwoLinePrefab;
        public GameObject OneLinePrefab;
        public GameObject CategoryPrefab;
        public GameObject targetScrollContent;
        public GameObject targetBackground;


        [Header("設定")]
        public bool enableUrlInput = true;
        public string titleAuthorSeparator = " - ";
        public Color categoryBackgroundColor = Color.clear;
        public Color categoryFontColor = new Color(0.1f, 0.2f, 0.6f, 1f);

        // [Header("Assets Data")]
        public AssetCategory[] categories;

        // [Header("Legacy Support")]
        public AssetsData[] AssetsData;
    }
}
