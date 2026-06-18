// ItemWordUI.cs
using UnityEngine;
using TMPro;

public class ItemWordUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private string originalText;

    public void sSetText(string word)
    {
        originalText = word;
        text.text = word;
    }

    // Backwards-compatible method name used by callers
    public void SetText(string word)
    {
        sSetText(word);
    }

    public void MarkAsFound()
    {
        text.text = "<s>" + originalText + "</s>"; // зачёркивание в TMP
        // Или: text.fontStyle = FontStyles.Strikethrough;
    }

    public string GetText()
    {
        return originalText;
    }
}