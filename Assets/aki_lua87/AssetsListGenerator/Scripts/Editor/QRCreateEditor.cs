using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Threading.Tasks;

namespace aki_lua87.AssetsListGenerator
{
    [CustomEditor(typeof(QRCreate))]
    public class QRCreateEditor : UnityEditor.Editor
    {
        private SerializedProperty _titleText;
        private SerializedProperty _supplementText;
        private SerializedProperty _urlInputField;
        private SerializedProperty _backgroundImage;
        private SerializedProperty _qrCodeImage;
        private SerializedProperty _title;
        private SerializedProperty _supplement;
        private SerializedProperty _url;
        private SerializedProperty _textColor;
        private SerializedProperty _backgroundColor;

        private void OnEnable()
        {
            _titleText = serializedObject.FindProperty("titleText");
            _supplementText = serializedObject.FindProperty("supplementText");
            _urlInputField = serializedObject.FindProperty("urlInputField");
            _backgroundImage = serializedObject.FindProperty("backgroundImage");
            _qrCodeImage = serializedObject.FindProperty("qrCodeImage");
            _title = serializedObject.FindProperty("title");
            _supplement = serializedObject.FindProperty("supplement");
            _url = serializedObject.FindProperty("url");
            _textColor = serializedObject.FindProperty("textColor");
            _backgroundColor = serializedObject.FindProperty("backgroundColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("参照設定", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_titleText, new GUIContent("タイトル用テキスト"));
            EditorGUILayout.PropertyField(_supplementText, new GUIContent("補足用テキスト"));
            EditorGUILayout.PropertyField(_urlInputField, new GUIContent("URL入力フィールド"));
            EditorGUILayout.PropertyField(_backgroundImage, new GUIContent("背景画像"));
            EditorGUILayout.PropertyField(_qrCodeImage, new GUIContent("QRコード画像"));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("以下を編集してください", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_title, new GUIContent("タイトル"));
            EditorGUILayout.PropertyField(_supplement, new GUIContent("補足説明"));
            EditorGUILayout.PropertyField(_url, new GUIContent("URL"));
            EditorGUILayout.PropertyField(_textColor, new GUIContent("文字色"));
            EditorGUILayout.PropertyField(_backgroundColor, new GUIContent("背景色"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate QRCode"))
            {
                var qrCreate = (QRCreate)target;
                if (qrCreate.qrCodeImage != null && !string.IsNullOrEmpty(qrCreate.url))
                {
                    if (qrCreate.titleText != null)
                        Undo.RecordObject(qrCreate.titleText, "Generate QRCode");
                    if (qrCreate.supplementText != null)
                        Undo.RecordObject(qrCreate.supplementText, "Generate QRCode");
                    if (qrCreate.urlInputField != null)
                        Undo.RecordObject(qrCreate.urlInputField, "Generate QRCode");
                    if (qrCreate.backgroundImage != null)
                        Undo.RecordObject(qrCreate.backgroundImage, "Generate QRCode");

                    qrCreate.SetTexts();
                    qrCreate.ApplyColors();

                    if (qrCreate.titleText != null)
                    {
                        EditorUtility.SetDirty(qrCreate.titleText);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(qrCreate.titleText);
                    }
                    if (qrCreate.supplementText != null)
                    {
                        EditorUtility.SetDirty(qrCreate.supplementText);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(qrCreate.supplementText);
                    }
                    if (qrCreate.urlInputField != null)
                    {
                        EditorUtility.SetDirty(qrCreate.urlInputField);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(qrCreate.urlInputField);
                    }
                    if (qrCreate.backgroundImage != null)
                    {
                        EditorUtility.SetDirty(qrCreate.backgroundImage);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(qrCreate.backgroundImage);
                    }

                    ProcessQRCodeCreateAsync(qrCreate.qrCodeImage, qrCreate.url);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "QRCode ImageとURLが必要です", "OK");
                }
            }
        }

        private async void ProcessQRCodeCreateAsync(Image qrCodeImage, string url)
        {
            var sprite = await QRCodeCache.GetOrCreateQRSprite(url);
            if (sprite != null)
            {
                qrCodeImage.sprite = sprite;
                EditorUtility.SetDirty(qrCodeImage);
                PrefabUtility.RecordPrefabInstancePropertyModifications(qrCodeImage);
            }
        }
    }
}
