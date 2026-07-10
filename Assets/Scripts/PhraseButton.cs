using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PhraseButton : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private Button button;
    [SerializeField] private float fadeDuration;
    private MinigameManager gameManager;

    public int Index { get; private set; }

    public event Action<PhraseButton> OnPressed;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
        canvasGroup = GetComponent<CanvasGroup>();
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
        transform.DOScale(Vector3.zero, fadeDuration - 0.1f).SetEase(Ease.InBack);
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeDuration - 0.1f).OnComplete(() => gameObject.SetActive(false));
        }
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