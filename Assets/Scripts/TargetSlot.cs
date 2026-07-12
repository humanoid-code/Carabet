using UnityEngine;
using UnityEngine.UI;
using TMPro; // Если используешь TextMeshPro

[System.Serializable]
public class TargetSlot : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI textObject;
    
    public void SetText(string text)
    {
        if (textObject != null) textObject.text = text;
    }

    public void ShowCorrect()
    {
        // Делаем текст ЗЕЛЁНЫМ
        if (textObject != null) 
        {
            textObject.color = Color.green;
            Debug.Log($"Часть текста стала зелёной: {textObject.text}");
        }
    }
    
    public void Reset()
    {
        if (textObject != null) textObject.color = Color.black;
    }
}
