using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace aki_lua87.AssetsListGenerator
{
    /// <summary>
    /// Creates the clipped title layout shared by generated AssetsList prefabs.
    /// Runtime motion is intentionally owned by the target-specific controller.
    /// </summary>
    public static class TitleMarqueeEditorUtility
    {
        public const string ViewportName = "titleAndAuther";
        public const string TrackName = "MarqueeText";
        private const float EndPadding = 10f;

        public static Text FindTitleText(Transform content)
        {
            if (content == null) return null;

            Transform titleObject = content.Find(ViewportName);
            if (titleObject == null) return null;

            Text directText = titleObject.GetComponent<Text>();
            if (directText != null) return directText;

            Transform track = titleObject.Find(TrackName);
            return track != null ? track.GetComponent<Text>() : null;
        }

        public static bool Configure(Text text, out RectTransform track, out float distance)
        {
            track = null;
            distance = 0f;
            if (text == null) return false;

            RectTransform textRect = text.rectTransform;
            if (textRect.name != TrackName && text.preferredWidth <= textRect.rect.width)
                return false;
            RectTransform viewport;

            if (textRect.name == TrackName && textRect.parent != null && textRect.parent.name == ViewportName)
            {
                viewport = textRect.parent as RectTransform;
            }
            else
            {
                Transform originalParent = textRect.parent;
                if (originalParent == null) return false;

                int siblingIndex = textRect.GetSiblingIndex();
                Vector2 anchorMin = textRect.anchorMin;
                Vector2 anchorMax = textRect.anchorMax;
                Vector2 anchoredPosition = textRect.anchoredPosition;
                Vector2 sizeDelta = textRect.sizeDelta;
                Vector2 pivot = textRect.pivot;
                Quaternion localRotation = textRect.localRotation;
                Vector3 localScale = textRect.localScale;

                GameObject viewportObject = new GameObject(ViewportName, typeof(RectTransform), typeof(RectMask2D));
                Undo.RegisterCreatedObjectUndo(viewportObject, "Create title marquee viewport");
                viewportObject.layer = text.gameObject.layer;
                viewport = viewportObject.GetComponent<RectTransform>();
                viewport.SetParent(originalParent, false);
                viewport.SetSiblingIndex(siblingIndex);
                viewport.anchorMin = anchorMin;
                viewport.anchorMax = anchorMax;
                viewport.anchoredPosition = anchoredPosition;
                viewport.sizeDelta = sizeDelta;
                viewport.pivot = pivot;
                viewport.localRotation = localRotation;
                viewport.localScale = localScale;

                Undo.SetTransformParent(textRect, viewport, "Move title into marquee viewport");
                textRect.name = TrackName;
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(0f, 1f);
                textRect.pivot = new Vector2(0f, 0.5f);
                textRect.anchoredPosition = Vector2.zero;
                textRect.localRotation = Quaternion.identity;
                textRect.localScale = Vector3.one;
            }

            if (viewport == null) return false;

            // The text's preferredWidth balloons once horizontalOverflow is set to Overflow below.
            // Text implements ILayoutElement, so without this, any ancestor LayoutGroup/ContentSizeFitter
            // (e.g. the scroll content's VerticalLayoutGroup) would pick up that huge width and blow out
            // the whole list's bounds. ignoreLayout keeps the viewport's manually-set size authoritative.
            LayoutElement layoutElement = viewport.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = Undo.AddComponent<LayoutElement>(viewport.gameObject);
            }
            layoutElement.ignoreLayout = true;

            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;

            float viewportWidth = viewport.rect.width;
            float preferredWidth = text.preferredWidth;
            textRect.sizeDelta = new Vector2(Mathf.Max(viewportWidth, preferredWidth), 0f);

            track = textRect;
            distance = Mathf.Max(0f, preferredWidth - viewportWidth + EndPadding);
            EditorUtility.SetDirty(text);
            EditorUtility.SetDirty(viewport);
            return distance > EndPadding;
        }
    }
}
