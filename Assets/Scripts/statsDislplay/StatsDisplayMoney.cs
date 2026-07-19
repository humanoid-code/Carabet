using UnityEngine;
using UnityEngine.UI;

public class StatsDisplayMoney : MonoBehaviour
{
    public Image fillImage;        // Верхняя заполняющаяся картинка
    public float maxValue = 100f;  // Максимальное значение
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (gameManager != null && fillImage != null)
        {
            float currentValue = gameManager.money;
            float fillAmount = Mathf.Clamp01(currentValue / maxValue);

            fillImage.fillAmount = fillAmount;
        }
    }
}