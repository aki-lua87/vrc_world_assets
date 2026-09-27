using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using UdonSharp;
using UdonSharpEditor;

namespace aki_lua87.AssetsListGenerator
{
    [CustomEditor(typeof(AssetsManagerBehaviour))]
    public class AssetsManagerInspector : UnityEditor.Editor
    {
        private AssetsManagerBehaviour _target;
        private SerializedProperty _categories;
        private SerializedProperty _assets;

        private const string PrefKeyGenerateQRCode = "aki_lua87.AssetsListGenerator.isGenerateQRCode";
        private bool isGenerateQRCode;
        private readonly List<RectTransform> marqueeViewports = new List<RectTransform>();
        private readonly List<float> marqueeDistances = new List<float>();

        private void OnEnable()
        {
            _target = target as AssetsManagerBehaviour;
            _categories = serializedObject.FindProperty("categories");
            _assets = serializedObject.FindProperty(nameof(AssetsData));
            isGenerateQRCode = EditorPrefs.GetBool(PrefKeyGenerateQRCode, false);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 基本設定を表示（categories以外）
            DrawBasicSettings();

            EditorGUILayout.Space();
            DrawCategoriesInput();

            EditorGUILayout.Space();

            // 必須設定のチェック
            if (_target.targetScrollContent == null)
            {
                EditorGUILayout.HelpBox("Target Content が設定されていません", MessageType.Error);
            }

            GUI.enabled = _target.targetScrollContent != null;
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                Generate();
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBasicSettings()
        {
            // Prefabs
            // EditorGUILayout.LabelField("プレファブ（編集しないでください）", EditorStyles.boldLabel);
            DrawPropertyFieldSafe("TwoLinePrefab", "URLPrefab");
            DrawPropertyFieldSafe("OneLinePrefab", "SimplePrefab");
            DrawPropertyFieldSafe("CategoryPrefab", "CategoryPrefab");
            DrawPropertyFieldSafe("targetScrollContent", "TargetContent");
            DrawPropertyFieldSafe("targetBackground", "TargetBackground");

            EditorGUILayout.Space();

            // Settings
            // EditorGUILayout.LabelField("設定", EditorStyles.boldLabel);

            // URL関連設定
            DrawPropertyFieldSafe("enableUrlInput", "URL入力を有効にする");
            GUI.enabled = _target.enableUrlInput;
            var newIsGenerateQRCode = EditorGUILayout.Toggle("QRコードを生成する", isGenerateQRCode);
            if (newIsGenerateQRCode != isGenerateQRCode)
            {
                isGenerateQRCode = newIsGenerateQRCode;
                EditorPrefs.SetBool(PrefKeyGenerateQRCode, isGenerateQRCode);
            }
            GUI.enabled = true;

            DrawPropertyFieldSafe("titleAuthorSeparator", "タイトル/作者の区切り文字");
            DrawPropertyFieldSafe("categoryBackgroundColor", "背景色");
            DrawPropertyFieldSafe("categoryFontColor", "文字色");
        }

        private void DrawPropertyFieldSafe(string propertyName, string label)
        {
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            }
            else
            {
                EditorGUILayout.HelpBox($"Property '{propertyName}' not found", MessageType.Warning);
            }
        }

        private void DrawCategoriesInput()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("カテゴリを追加", GUILayout.Width(120)))
            {
                _categories.arraySize++;
                var newCategory = _categories.GetArrayElementAtIndex(_categories.arraySize - 1);
                newCategory.FindPropertyRelative("categoryName").stringValue = $"Category {_categories.arraySize}";
                var assetsArray = newCategory.FindPropertyRelative("assets");
                assetsArray.arraySize = 0;
            }
            if (_categories.arraySize == 0)
            {
                EditorGUILayout.LabelField("カテゴリが作成されていません", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            for (int i = 0; i < _categories.arraySize; i++)
            {
                DrawCategoryInput(i);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCategoryInput(int categoryIndex)
        {
            var categoryProp = _categories.GetArrayElementAtIndex(categoryIndex);
            var categoryNameProp = categoryProp.FindPropertyRelative("categoryName");
            var assetsProp = categoryProp.FindPropertyRelative("assets");

            EditorGUILayout.Space();

            var headerStyle = new GUIStyle(EditorStyles.foldout);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = new Color(0.1f, 0.2f, 0.6f);

            bool isExpanded = EditorPrefs.GetBool($"AssetsManager_Category_{categoryIndex}", true);

            EditorGUILayout.BeginHorizontal();
            isExpanded = EditorGUILayout.Foldout(isExpanded, $"{categoryNameProp.stringValue} ({assetsProp.arraySize})", headerStyle);
            EditorPrefs.SetBool($"AssetsManager_Category_{categoryIndex}", isExpanded);

            if (GUILayout.Button("カテゴリを削除", GUILayout.Width(90)))
            {
                _categories.DeleteArrayElementAtIndex(categoryIndex);
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (isExpanded)
            {
                EditorGUI.indentLevel++;

                // カテゴリ名編集
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("カテゴリ名:", GUILayout.Width(100));
                categoryNameProp.stringValue = EditorGUILayout.TextField(categoryNameProp.stringValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                for (int j = 0; j < assetsProp.arraySize; j++)
                {
                    DrawAssetInput(assetsProp.GetArrayElementAtIndex(j), j, categoryIndex);
                }
                if (GUILayout.Button("項目を追加", GUILayout.Width(100)))
                {
                    assetsProp.arraySize++;
                    var newAsset = assetsProp.GetArrayElementAtIndex(assetsProp.arraySize - 1);
                    newAsset.FindPropertyRelative("assetTitle").stringValue = "";
                    newAsset.FindPropertyRelative("assetAuthor").stringValue = "";
                    newAsset.FindPropertyRelative("assetURL").stringValue = _target.enableUrlInput ? "" : "";
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawAssetInput(SerializedProperty assetProp, int assetIndex, int categoryIndex)
        {
            var titleProp = assetProp.FindPropertyRelative("assetTitle");
            var authorProp = assetProp.FindPropertyRelative("assetAuthor");
            var urlProp = assetProp.FindPropertyRelative("assetURL");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"項目 {assetIndex + 1}", GUILayout.Width(80));
            if (GUILayout.Button("項目を削除", GUILayout.Width(75)))
            {
                var assetsProp = _categories.GetArrayElementAtIndex(categoryIndex).FindPropertyRelative("assets");
                assetsProp.DeleteArrayElementAtIndex(assetIndex);
                return;
            }
            EditorGUILayout.EndHorizontal();

            titleProp.stringValue = EditorGUILayout.TextField("タイトル", titleProp.stringValue);
            authorProp.stringValue = EditorGUILayout.TextField("作者", authorProp.stringValue);

            // URL入力はenableUrlInputで制御
            if (_target.enableUrlInput)
            {
                urlProp.stringValue = EditorGUILayout.TextField("URL", urlProp.stringValue);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }


        private async void Generate()
        {
            if (_target.targetScrollContent == null)
            {
                Debug.LogError("Target Content is not assigned!");
                return;
            }
            var marquee = _target.GetComponent<TitleMarquee>();
            UdonSharpBehaviour optionalMarquee = null;
            FieldInfo optionalTracks = null;
            FieldInfo optionalDistances = null;
            // Optional packages can provide their own Udon with these two public fields.
            foreach (var component in _target.GetComponents<UdonSharpBehaviour>())
            {
                if (component == marquee) continue;
                var tracksField = component.GetType().GetField("titleMarqueeTracks");
                var distancesField = component.GetType().GetField("titleMarqueeDistances");
                if (tracksField == null || tracksField.FieldType != typeof(RectTransform[]) ||
                    distancesField == null || distancesField.FieldType != typeof(float[])) continue;
                optionalMarquee = component;
                optionalTracks = tracksField;
                optionalDistances = distancesField;
                break;
            }
            if (marquee == null && optionalMarquee == null)
            {
                Debug.LogError("A marquee Udon component is missing from AssetsListGenerator.");
                return;
            }

            marqueeViewports.Clear();
            marqueeDistances.Clear();
            DestroyChildAll(_target.targetScrollContent.transform);

            await GenerateVerticalList();
            if (_target == null) return;
            var tracks = marqueeViewports.ToArray();
            var distances = marqueeDistances.ToArray();
            if (marquee != null)
            {
                marquee.titleMarqueeTracks = tracks;
                marquee.titleMarqueeDistances = distances;
                EditorUtility.SetDirty(marquee);
                UdonSharpEditorUtility.CopyProxyToUdon(marquee);
            }
            if (optionalMarquee != null)
            {
                optionalTracks.SetValue(optionalMarquee, tracks);
                optionalDistances.SetValue(optionalMarquee, distances);
                EditorUtility.SetDirty(optionalMarquee);
                UdonSharpEditorUtility.CopyProxyToUdon(optionalMarquee);
            }
        }


        private async Task GenerateVerticalList()
        {
            if (_target == null || _target.targetScrollContent == null) return;

            SetupVerticalLayoutGroup();

            await GenerateWithCategories((assetData) => CreateBasicItem(assetData));
        }

        private async Task GenerateWithCategories(System.Func<(string category, string title, string author, string url), GameObject> createItemFunc)
        {
            if (_target.categories == null || _target.categories.Length == 0)
            {
                Debug.LogWarning("No categories found. Please add categories first.");
                return;
            }

            foreach (var category in _target.categories)
            {
                CreateCategoryHeader(category.categoryName);

                if (category.assets != null && category.assets.Length > 0)
                {
                    foreach (var asset in category.assets)
                    {
                        var assetData = (category.categoryName, asset.assetTitle, asset.assetAuthor, asset.assetURL);
                        var content = createItemFunc(assetData);
                        await ProcessQRCode(content, asset.assetURL);
                    }
                }
            }
        }

        private GameObject CreateCategoryHeader(string categoryName)
        {
            if (_target.CategoryPrefab == null)
            {
                Debug.LogWarning("CategoryPrefab is not assigned, creating runtime item instead");
                return CreateRuntimeCategoryHeader(categoryName);
            }

            var header = Instantiate(_target.CategoryPrefab, _target.targetScrollContent.transform);
            header.name = $"Category_{categoryName}";

            // カテゴリ色設定を適用
            ApplyColorSettings(header);

            var titleText = header.transform.Find("CategoryTitle")?.GetComponent<Text>();
            if (titleText != null)
            {
                titleText.text = $"■ {categoryName}";
            }

            return header;
        }

        private GameObject CreateRuntimeCategoryHeader(string categoryName)
        {
            var header = new GameObject($"Category_{categoryName}");
            header.transform.SetParent(_target.targetScrollContent.transform, false);

            var rectTransform = header.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0, 40);
            rectTransform.localScale = Vector3.one;

            var image = header.AddComponent<Image>();
            // targetBackgroundに背景色を適用
            if (_target.targetBackground != null)
            {
                var targetBgImage = _target.targetBackground.GetComponent<Image>();
                if (targetBgImage != null)
                {
                    targetBgImage.color = _target.categoryBackgroundColor.a == 0f ?
                        Color.clear : _target.categoryBackgroundColor;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetBgImage);
                    EditorUtility.SetDirty(targetBgImage);
                }
            }
            image.color = Color.clear;

            var titleText = CreateTextComponent(header, "CategoryTitle", $"■ {categoryName}");
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = _target.categoryFontColor; // 全体設定のフォント色を使用
            titleText.alignment = TextAnchor.MiddleLeft;

            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, 0);

            return header;
        }

        private void SetupVerticalLayoutGroup()
        {
            if (_target == null || _target.targetScrollContent == null) return;

            RemoveOtherLayoutComponents(typeof(VerticalLayoutGroup));

            var layout = _target.targetScrollContent.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = _target.targetScrollContent.AddComponent<VerticalLayoutGroup>();

            layout.spacing = 0f;
            layout.padding = new RectOffset(10, 0, 0, 0);
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(layout);
            EditorUtility.SetDirty(layout);
        }

        private void ApplyColorSettings(GameObject obj)
        {
            // ルートのImage要素に背景色を設定
            var rootImage = obj.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.color = Color.clear;
            }

            // targetBackgroundに背景色を適用
            if (_target.targetBackground != null)
            {
                var targetBgImage = _target.targetBackground.GetComponent<Image>();
                if (targetBgImage != null)
                {
                    targetBgImage.color = _target.categoryBackgroundColor.a == 0f ?
                        Color.clear : _target.categoryBackgroundColor;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(targetBgImage);
                    EditorUtility.SetDirty(targetBgImage);
                }
            }

            // 子要素のすべてのText要素にフォント色を設定
            var textComponents = obj.GetComponentsInChildren<Text>();
            foreach (var text in textComponents)
            {
                text.color = _target.categoryFontColor;
            }
        }

        private void RemoveOtherLayoutComponents(System.Type keepType)
        {
            if (_target == null || _target.targetScrollContent == null) return;

            var layouts = _target.targetScrollContent.GetComponents<LayoutGroup>();
            foreach (var layout in layouts)
            {
                if (layout.GetType() != keepType)
                {
                    DestroyImmediate(layout);
                }
            }
        }

        private GameObject CreateBasicItem((string category, string title, string author, string url) assetData)
        {
            // URL入力が無効の場合は常にOneLinePrefabを使用、有効の場合はURL有無で判定
            var hasUrl = _target.enableUrlInput && !string.IsNullOrEmpty(assetData.url);
            var targetPrefab = hasUrl ? _target.TwoLinePrefab : _target.OneLinePrefab;

            if (targetPrefab == null)
            {
                Debug.LogWarning("Prefab is not assigned, creating runtime item instead");
                return CreateRuntimeItem((assetData.title, assetData.author, hasUrl ? assetData.url : ""), new Vector2(300, 80));
            }

            return CreateItemFromPrefab(targetPrefab, (assetData.title, assetData.author, hasUrl ? assetData.url : ""));
        }


        private GameObject CreateItemFromPrefab(GameObject prefab, (string title, string author, string url) assetData)
        {
            var content = Instantiate(prefab, _target.targetScrollContent.transform);
            content.name = assetData.title;

            // アセット色設定を適用
            ApplyColorSettings(content);

            var titleAndAuthorText = content.transform.Find("titleAndAuther")?.GetComponent<Text>();
            if (titleAndAuthorText != null)
            {
                titleAndAuthorText.text = string.IsNullOrEmpty(assetData.author) ?
                    assetData.title :
                    assetData.title + _target.titleAuthorSeparator + assetData.author;
                RegisterMarquee(titleAndAuthorText);
            }

            if (!string.IsNullOrEmpty(assetData.url))
            {
                var urlField = content.transform.Find("url")?.GetComponent<InputField>();
                if (urlField != null)
                {
                    urlField.text = assetData.url;
                }
            }

            return content;
        }

        private GameObject CreateRuntimeItem((string title, string author, string url) assetData, Vector2 size)
        {
            var content = new GameObject(assetData.title);
            content.transform.SetParent(_target.targetScrollContent.transform, false);

            var rectTransform = content.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.localScale = Vector3.one;

            var titleTextContent = string.IsNullOrEmpty(assetData.author) ?
                assetData.title :
                assetData.title + _target.titleAuthorSeparator + assetData.author;
            var titleText = CreateTextComponent(content, "Title", titleTextContent);
            titleText.transform.localPosition = new Vector3(0, size.y * 0.2f, 0);
            titleText.fontSize = Mathf.RoundToInt(size.y * 0.12f);
            RegisterMarquee(titleText);

            // URL入力が有効かつURLが存在する場合のみURL欄を表示
            if (_target.enableUrlInput && !string.IsNullOrEmpty(assetData.url))
            {
                var urlField = CreateInputField(content, "URL", assetData.url);
                urlField.transform.localPosition = new Vector3(0, -size.y * 0.2f, 0);
            }

            return content;
        }

        private void RegisterMarquee(Text titleText)
        {
            RectTransform track;
            float distance;
            if (!TitleMarqueeEditorUtility.Configure(titleText, out track, out distance)) return;
            marqueeViewports.Add(track);
            marqueeDistances.Add(distance);
        }

        private Text CreateTextComponent(GameObject parent, string name, string text)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(0, 20);
            rectTransform.localScale = Vector3.one;

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 12;
            textComponent.color = Color.black;
            textComponent.alignment = TextAnchor.MiddleCenter;

            return textComponent;
        }

        private InputField CreateInputField(GameObject parent, string name, string text)
        {
            var inputObj = new GameObject(name);
            inputObj.transform.SetParent(parent.transform, false);

            var rectTransform = inputObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(0, 20);
            rectTransform.localScale = Vector3.one;

            var image = inputObj.AddComponent<Image>();
            image.color = Color.white;

            var inputField = inputObj.AddComponent<InputField>();
            inputField.text = text;

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            textRect.localScale = Vector3.one;

            var textComponent = textObj.AddComponent<Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 10;
            textComponent.color = Color.black;

            inputField.textComponent = textComponent;

            return inputField;
        }


        private async Task ProcessQRCode(GameObject content, string url)
        {
            if (_target.enableUrlInput && !string.IsNullOrEmpty(url) && isGenerateQRCode)
            {
                var qrCodeImage = content.transform.Find("QRCode")?.GetComponent<Image>();
                if (qrCodeImage != null)
                {
                    var sprite = await QRCodeCache.GetOrCreateQRSprite(url);
                    if (sprite != null) qrCodeImage.sprite = sprite;
                }
            }
            else
            {
                var qrCodeObj = content.transform.Find("QRCode");
                if (qrCodeObj != null)
                {
                    qrCodeObj.gameObject.SetActive(false);
                }
            }
        }

        private void DestroyChildAll(Transform root)
        {
            var count = root.childCount;
            for (int i = 0; i < count; i++)
            {
                Debug.Log("Destroy:" + root.GetChild(0).name);
                DestroyImmediate(root.GetChild(0).gameObject);
            }
        }
    }
}
