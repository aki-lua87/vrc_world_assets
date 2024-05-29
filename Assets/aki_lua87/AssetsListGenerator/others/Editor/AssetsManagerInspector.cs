using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;

namespace aki_lua87.AssetsListGenerator
{
    [CustomEditor(typeof(AssetsManagerBehaviour))]
    public class AssetsManagerInspector : Editor
    {
        private AssetsManagerBehaviour _target;
        private SerializedProperty _assets;
        private ReorderableList _reorderableList;

        private void OnEnable()
        {
            _target = target as AssetsManagerBehaviour;
            _assets = serializedObject.FindProperty(nameof(AssetsData));
            _reorderableList = new ReorderableList(serializedObject, _assets)
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.PropertyField(rect, _assets.GetArrayElementAtIndex(index));
                },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(_assets.GetArrayElementAtIndex(index)),
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Assets List"),
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void Generate()
        {
            DestroyChildAll(_target.targetContent.transform);
            for (int i = 0; i < _reorderableList.count; i++)
            {
                var element = _assets.GetArrayElementAtIndex(i);
                var assetTitle = element.FindPropertyRelative(nameof(AssetsData.assetTitle)).stringValue;
                var assetAuthor = element.FindPropertyRelative(nameof(AssetsData.assetAuthor)).stringValue;
                var assetURL = element.FindPropertyRelative(nameof(AssetsData.assetURL)).stringValue;

                var targetPrefab = _target.TwoLinePrefab;
                if (assetURL == "" || assetURL == null)
                {
                    targetPrefab = _target.OneLinePrefab;
                }

                var content = Instantiate(targetPrefab, _target.targetContent.transform);
                content.name = assetTitle;
                var titleAndAutherText = content.transform.Find("titleAndAuther").GetComponent<Text>();
                titleAndAutherText.text = assetTitle + " / " + assetAuthor;

                if (targetPrefab == _target.TwoLinePrefab)
                {
                    var url = content.transform.Find("url").GetComponent<InputField>();
                    url.text = assetURL;
                }
                content.transform.SetParent(_target.targetContent.transform);
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
