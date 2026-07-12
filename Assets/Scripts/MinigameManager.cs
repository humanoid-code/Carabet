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
    public RectTransform targetPanel; // Сюда кинь свою панель
    public TextMeshProUGUI targetText; // Сюда кинь ОДИН текстовый объект на панели

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
    private void RenderFullPhrase()
    {
        string finalText = "";
        
        for (int i = 0; i < pieces.Count; i++)
        {
            string word = pieces[i];
            
            // Логика цвета:
            // Если индекс меньше текущего правильного -> Чёрный
            // Если равен или больше (но мы еще не открыли) -> Черный (или серый, если хочешь показать "будущее")
            // Но по твоей логике: сначала всё чёрное, потом по клику красим.
            
            string colorTag = "<color=black>"; 
            
            // Если слово уже "открыто" (мы нажали на него ранее)
            if (i < currentCorrectIndex)
            {
                colorTag = "<color=green>";
            }
            
            finalText += $"{colorTag}{word}</color>";
            
            // Добавляем пробел между словами, если это не последний символ/знак
            if (i < pieces.Count - 1)
            {
                // Пробел тоже красим в цвет текущего слова, чтобы не было белых дыр, 
                // или делаем его нейтральным. Давай сделаем нейтральным серым для красоты
                finalText += "<color=#AAAAAA> </color>"; 
            }
        }

        targetText.text = finalText;
        
        Debug.Log($"Фраза собрана: {finalText}");
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

                if (targetText != null)
        {
            targetText.richText = true; // ОБЯЗАТЕЛЬНО! Без этого цвета не сработают
            RenderFullPhrase();
        }
        else
        {
            Debug.LogError("НЕ НАЗНАЧЕН ОБЪЕКТ targetText в инспекторе!");
        }
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

            // КРАСИМ ТОЛЬКО ТЕКСТ В ЗЕЛЕНЫЙ
            
            currentCorrectIndex++; // Увеличиваем ожидаемый индекс
            
            RenderFullPhrase(); 
            Debug.Log($"Current correct index {currentCorrectIndex}!");
            
            // (Опционально) Если хочешь, чтобы и фон стал светло-зеленым:
            // targetSlots[currentCorrectIndex].imageObject.color = new Color(0.8f, 1f, 0.8f);

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
