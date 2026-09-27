using UnityEngine;
using UnityEngine.UI;

public class QRCreate : MonoBehaviour
{
    [Header("References")]
    public Text titleText;
    public Text supplementText;
    public InputField urlInputField;
    public Image backgroundImage;
    public Image qrCodeImage;

    public string title = "";
    public string supplement = "";
    public string url = "";
    public Color textColor = Color.white;
    public Color backgroundColor = Color.black;

    public void SetTexts()
    {
        if (titleText != null)
            titleText.text = title;
        if (supplementText != null)
            supplementText.text = supplement;
        if (urlInputField != null)
            urlInputField.text = url;
    }

    public void ApplyColors()
    {
        if (titleText != null)
            titleText.color = textColor;
        if (supplementText != null)
            supplementText.color = textColor;
        if (urlInputField != null && urlInputField.textComponent != null)
            urlInputField.textComponent.color = textColor;
        if (backgroundImage != null)
            backgroundImage.color = backgroundColor;
    }
}
