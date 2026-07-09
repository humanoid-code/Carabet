using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MovingPhraseButton : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Скорость движения кнопки")]
    [SerializeField] private float speed = 200f;
    
    [Tooltip("Диапазон рандома для направления (чем больше, тем хаотичнее старт)")]
    [SerializeField] private float randomRange = 1f;

    private RectTransform rectTransform;
    private Vector2 velocity;
    private RectTransform parentRect; // Родитель (обычно Canvas или Panel)
    private bool isInitialized = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Пытаемся найти родителя с RectTransform (Canvas или панель)
        parentRect = transform.parent?.GetComponent<RectTransform>();
        
        if (parentRect == null)
        {
            Debug.LogError($"[MovingPhraseButton] Не найден родитель с RectTransform для кнопки: {gameObject.name}. Движение работать не будет.");
            return;
        }

        // Инициализируем случайное направление
        InitializeDirection();
        isInitialized = true;
    }

    void InitializeDirection()
    {
        // Генерируем случайный вектор направления (-1 до 1)
        float randomX = Random.Range(-randomRange, randomRange);
        float randomY = Random.Range(-randomRange, randomRange);

        // Нормализуем, чтобы скорость была одинаковой во всех направлениях
        velocity = new Vector2(randomX, randomY).normalized;
        
        // Если вдруг нормализация дала 0 (крайне редко), ставим дефолт
        if (velocity.magnitude == 0)
            velocity = Vector2.right;
    }

    void Update()
    {
        if (!isInitialized) return;

        // Двигаем кнопку
        rectTransform.anchoredPosition += velocity * speed * Time.deltaTime;

        CheckBoundsAndBounce();
    }

    /// <summary>
    /// Проверяет, не вышла ли кнопка за границы родителя, и отражает её
    /// </summary>
    void CheckBoundsAndBounce()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        Vector2 size = rectTransform.sizeDelta; // Размер самой кнопки
        Vector2 parentSize = parentRect.sizeDelta; // Размер области (Canvas)

        // Вычисляем границы области движения (с учетом размера кнопки, чтобы она не улетала наполовину за экран)
        float minX = -parentSize.x / 2f + size.x / 2f;
        float maxX = parentSize.x / 2f - size.x / 2f;
        float minY = -parentSize.y / 2f + size.y / 2f;
        float maxY = parentSize.y / 2f - size.y / 2f;

        bool bounced = false;

        // Проверка по X
        if (pos.x <= minX || pos.x >= maxX)
        {
            velocity.x *= -1; // Отражение по горизонтали
            bounced = true;
            
            // Корректируем позицию, чтобы кнопка не застряла в стене из-за высокой скорости
            if (pos.x <= minX) pos.x = minX;
            else pos.x = maxX;
        }

        // Проверка по Y
        if (pos.y <= minY || pos.y >= maxY)
        {
            velocity.y *= -1; // Отражение по вертикали
            bounced = true;

            // Корректировка позиции
            if (pos.y <= minY) pos.y = minY;
            else pos.y = maxY;
        }

        // Применяем исправленную позицию, если было столкновение
        if (bounced)
        {
            rectTransform.anchoredPosition = pos;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        this.speed = newSpeed;
        Debug.Log($"Кнопка '{gameObject.name}' получила скорость: {speed}");
    }
    
    // Опционально: метод для сброса скорости, если нужно остановить кнопку при клике
    public void StopMoving()
    {
        velocity = Vector2.zero;
    }
}

