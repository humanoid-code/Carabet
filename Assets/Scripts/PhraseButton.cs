using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhraseButton : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private Button button;
    private MinigameManager gameManager;

    public int Index { get; private set; }

    public event Action<PhraseButton> OnPressed;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }

    public void Initialize(string text, int index, MinigameManager manager)
    {
        textLabel.text = text;
        Index = index;
        gameManager = manager; // Сохраняем ссылку
    }

    public void Hide()
    {
        Destroy(gameObject);
    }

    private void HandleClick()
    {
        // Когда нажали, сразу сообщаем менеджеру
        if(gameManager != null)
        {
            gameManager.HandleButtonPress(this);
        }
        else
        {
            Debug.LogWarning("Кнопка нажата, но менеджер не назначен!");
        }
    
        OnPressed?.Invoke(this); // Оставляем старое событие, вдруг пригодится
}
}   