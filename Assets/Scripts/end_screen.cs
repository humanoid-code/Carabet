using UnityEngine;
using UnityEngine.UI;

public class end_screen : MonoBehaviour
{
    public enum StatType { Money, Law, WorkersWellbeing}

    [Header("Настройки связи")]
    [SerializeField] private StatType targetStat; // Какую стату отслеживаем
    [SerializeField] private Image targetImage;   // Компонент картинки

    [Header("Спрайты состояний")]
    [Tooltip("Если значение СТРОГО МЕНЬШЕ порога")]
    [SerializeField] private Sprite lowSprite;    // Например, красный/грустный спрайт
    [Tooltip("Если значение БОЛЬШЕ или РАВНО порогу")]
    [SerializeField] private Sprite highSprite;   // Например, зеленый/веселый спрайт

    [Header("Порог разделения")]
    [SerializeField] private int threshold = 60;  // Граница между состояниями

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        UpdateImage();
    }

    public void UpdateImage()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null) return;
        }

        int currentStatValue = GetStatValue();

        // Логика всего для двух состояний:
        if (currentStatValue <= threshold)
        {
            if (lowSprite != null) targetImage.sprite = lowSprite;
        }
        else
        {
            if (highSprite != null) targetImage.sprite = highSprite;
        }
    }

    private int GetStatValue()
    {
        switch (targetStat)
        {
            case StatType.Money: return gameManager.money;
            case StatType.Law: return gameManager.law;
            case StatType.WorkersWellbeing: return gameManager.workers_wellbeing;
            default: return 0;
        }
    }
}
