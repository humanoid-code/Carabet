using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ссылка на объект, который создает кнопки ВНУТРИ Canvas")]
    [SerializeField] private ButtonSpawner buttonSpawner; 
    [SerializeField] private string testPhrase;
    [SerializeField] private int testPieces;
    [SerializeField] private float testSpeed;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private float totalTime = 10f;
    [SerializeField] private float slotGap = 200f;
    
    private float timeLeft;     
    public event Action OnGameFinished;      // Победа (собрали все слова)
    public event Action<string> OnWrongAnswer;// Ошибка
    public event Action OnTimeOut;            // Проигрыш (время вышло)

    private List<string> pieces;
    private int currentCorrectIndex;
    private bool gameRunning;
    private List<TargetSlot> targetSlots = new List<TargetSlot>();
    public RectTransform targetPanel;

    void Start()
    {
        if (buttonSpawner == null)
        {
            Debug.LogError("Ошибка: Не назначен ButtonSpawner! Найди его в Canvas.");
        }
                if (timeSlider == null)
        {
            Debug.LogError("Ошибка: Не назначен Time Slider в инспекторе!");
        }
        else
        {
            // Инициализируем полосу на максимум
            timeSlider.value = 1f; 
            timeSlider.interactable = false; // Чтобы игрок не мог двигать ползунок мышкой
        }
    }

    void Update()
    {
        if (!gameRunning) return;

        // --- ЛОГИКА ТАЙМЕРА ---
        timeLeft -= Time.deltaTime;

        // Обновляем полосу (от 1 до 0)
        if (timeSlider != null)
        {
            float normalizedTime = Mathf.Max(0, timeLeft / this.totalTime);
            timeSlider.value = normalizedTime;
            
            // Меняем цвет полосы, если мало времени (опционально, красный)
            if (normalizedTime < 0.3f && timeSlider.fillRect != null)
            {
                timeSlider.fillRect.GetComponent<Image>().color = Color.red;
            }
            else if (timeSlider.fillRect != null)
            {
                // Возвращаем белый/стандартный цвет
                timeSlider.fillRect.GetComponent<Image>().color = Color.white; 
            }
        }

        // Проверка на истечение времени
        if (timeLeft <= 0f)
        {
            TimeOut();
        }
    }
    private void RenderTargetPhrase(List<string> parts)
    {
        // 1. Чистим старые слоты
        foreach (var slot in targetSlots)
        {
            if (slot.rectTransform != null) Destroy(slot.rectTransform.gameObject);
        }
        targetSlots.Clear();

        float currentX = 0f;
        
        // --- НАСТРОЙКИ РАЗМЕРОВ (поиграй с этими цифрами, если нужно) ---
        const float slotWidth = 400f;  // Ширина "кармана" для слова
        const float slotHeight = 140f;  // Высота "кармана"
        const float gap = 400f;         // Расстояние между словами
        // -----------------------------------------------------------------

        // Считаем общую ширину, чтобы отцентровать всю фразу на панели
        float totalWidth = (parts.Count * slotWidth) + ((parts.Count - 1) * gap);
        currentX = -totalWidth / 2f + 800;

        foreach (string part in parts)
        {
            // 1. Создаем контейнер слота
            GameObject slotObj = new GameObject($"Slot_{part}");
            slotObj.transform.SetParent(targetPanel);
            
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            if (slotRect == null) slotRect = slotObj.AddComponent<RectTransform>();
            
            // Якоря в точку, позиция задается anchoredPosition
            slotRect.anchorMin = Vector2.zero;
            slotRect.anchorMax = Vector2.zero;
            slotRect.pivot = new Vector2(0.5f, 0.5f); // Опорная точка по центру
            slotRect.anchoredPosition = new Vector2(currentX, 0);
            
            // !!! ГЛАВНОЕ: Мы ЗАДАЕМ размер жестко здесь. Никаких попыток измерить текст!
            slotRect.sizeDelta = new Vector2(slotWidth, slotHeight); 

            // 2. Фон (Image) - белый прямоугольник
            GameObject imageObj = new GameObject("Background");
            imageObj.transform.SetParent(slotObj.transform);
            Image bgImage = imageObj.AddComponent<Image>();
            bgImage.color = Color.white;
            
            RectTransform imgRect = imageObj.GetComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one; // Растягиваем фон на весь слот (100%)
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;

            // 3. Текст (TMP)
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(imageObj.transform); // Текст внутри фона
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = part;
            tmpText.color = new Color(0f, 0f, 0f, 1f); // Черный цвет
            tmpText.alignment = TextAlignmentOptions.Center; // Центрируем текст
            tmpText.fontSize = 32; // Чуть крупнее, чтобы было видно
            tmpText.autoSizeTextContainer = false; // ОТКЛЮЧАЕМ авто-размер! Он враг центрирования.
            
            // ВАЖНО: Якоря текста должны быть РАСТЯНУТЫ на весь родительский фон
            RectTransform txtRect = textObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one; 
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            // Размер текста мы НЕ меняем. Он заполняет весь белый прямоугольник благодаря якорям выше.

            // Сохраняем ссылки
            TargetSlot slotData = new TargetSlot();
            slotData.rectTransform = slotRect;
            slotData.textObject = tmpText;
            targetSlots.Add(slotData);

            // Двигаем позицию для следующего слова
            currentX += slotWidth + gap;
            
            Debug.Log($"[OK] Слот '{part}': позиция X={currentX}, размер слота={slotWidth}x{slotHeight}");
        }
    }

    public static List<string> Split(string phrase, int piecesCount)
    {
        if (string.IsNullOrWhiteSpace(phrase))
        {
            return new List<string>();
        }

        var trimmedPhrase = phrase.Trim();
        if (piecesCount <= 1)
        {
            return new List<string> { trimmedPhrase };
        }

        var words = trimmedPhrase.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return new List<string>();
        }

        if (words.Length < piecesCount) piecesCount = words.Length;

        var actualPiecesCount = Mathf.Min(piecesCount, words.Length);
        var result = new List<string>(actualPiecesCount);
        var baseSize = words.Length / actualPiecesCount;
        var remainder = words.Length % actualPiecesCount;
        var index = 0;

        for (var i = 0; i < actualPiecesCount; i++)
        {
            var size = baseSize + (i < remainder ? 1 : 0);
            if (size <= 0) continue;
            result.Add(string.Join(" ", words, index, size));
            index += size;
        }
        return result;
    }

    public void StartGame(string phrase, int piecesCount, float moveSpeed, float? customTime = null)
    {
        gameRunning = true;
        currentCorrectIndex = 0;
        
        float gameTime = customTime ?? this.totalTime;
        this.totalTime = gameTime;
        timeLeft = gameTime;
        pieces = Split(phrase, piecesCount);

        RenderTargetPhrase(pieces);
        if (buttonSpawner != null)
        {
            var createdButtons = buttonSpawner.SpawnButtons(pieces, moveSpeed, this); 
            // ВАЖНО: SpawnButtons должен возвращать List<PhraseButton>, см. пункт 3!
    
            ButtonDisposer disposer = GetComponent<ButtonDisposer>();
            if(disposer != null) disposer.RegisterButtons(createdButtons);
        }

        Debug.Log($"Игра началась! Время: {timeLeft} сек.");
        
        // Сбрасываем полосу времени
        if(timeSlider != null) timeSlider.value = 1f;
        if (timeSlider != null) timeSlider.gameObject.SetActive(true);
    }

    public void HandleButtonPress(PhraseButton pressedButton)
    {
        if (!gameRunning) return;

        int index = pressedButton.Index;

        // Проверка: правильная ли кнопка?
        if (index == currentCorrectIndex)
        {
            // ПРАВИЛЬНЫЙ ОТВЕТ
            Debug.Log($"Правильно! Нажата кнопка #{index}");
            
            pressedButton.Hide(); // Удаляем кнопку

            if (currentCorrectIndex < targetSlots.Count)
            {
            // КРАСИМ ТОЛЬКО ТЕКСТ В ЗЕЛЕНЫЙ
            targetSlots[currentCorrectIndex].textObject.color = Color.green;
            
            currentCorrectIndex++; // Увеличиваем ожидаемый индекс
            
            // (Опционально) Если хочешь, чтобы и фон стал светло-зеленым:
            // targetSlots[currentCorrectIndex].imageObject.color = new Color(0.8f, 1f, 0.8f);
            }

            // Проверка на победу
            if (currentCorrectIndex >= pieces.Count)
            {
                EndGame();
            }
        }
        else
        {
            // НЕПРАВИЛЬНЫЙ ОТВЕТ
            Debug.LogWarning($"Ошибка! Ожидалась кнопка #{currentCorrectIndex}, нажата #{index}");
            
            // --- МЕСТО ДЛЯ ЛОГИКИ ОШИБКИ ---
            // Например: проиграть звук ошибки, уменьшить жизни, сбросить прогресс
            OnWrongAnswer?.Invoke("Неверный порядок слов!");
            
            // Вариант А: Игра продолжается, игрок должен попробовать снова нажать нужную кнопку
            // (Ничего не делаем с currentCorrectIndex, он остается прежним)
            
            // Вариант Б: Сброс прогресса (раскомментируй, если нужно)
            // currentCorrectIndex = 0; 
            // Debug.Log("Прогресс сброшен.");
        }
    }
    private void EndGame()
    {
        gameRunning = false;
        Debug.Log("Мини-игра окончена! Победа.");

        if (timeSlider != null) timeSlider.gameObject.SetActive(false); // Прячем вместо удаления
        OnGameFinished?.Invoke();
    }

    private void TimeOut()
    {
        gameRunning = false;
        Debug.Log("Время вышло! ПРОИГРЫШ.");

        ButtonDisposer disposer = GetComponent<ButtonDisposer>();
        if (disposer != null)
        {
            disposer.DestroyAllButtons();
        }

        if (timeSlider != null) timeSlider.gameObject.SetActive(false); // Прячем вместо удаления

        OnTimeOut?.Invoke();
    }
}
