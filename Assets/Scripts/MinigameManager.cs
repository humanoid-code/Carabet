using System;
using System.Collections.Generic;
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
    [SerializeField] private float totalTime = 10f;       // Можно менять в инспекторе или передавать в StartGame
    
    private float timeLeft;     
    public event Action OnGameFinished;      // Победа (собрали все слова)
    public event Action<string> OnWrongAnswer;// Ошибка
    public event Action OnTimeOut;            // Проигрыш (время вышло)

    private List<string> pieces;
    private int currentCorrectIndex;
    private bool gameRunning;

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

        if (words.Length >= piecesCount)
        {
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

        // Fallback: если слов меньше чем нужно, режем по символам
        var chars = trimmedPhrase.ToCharArray();
        var charPiecesCount = Mathf.Min(piecesCount, chars.Length);
        var charResult = new List<string>(charPiecesCount);
        var charBaseSize = chars.Length / charPiecesCount;
        var charRemainder = chars.Length % charPiecesCount;
        var charIndex = 0;

        for (var i = 0; i < charPiecesCount; i++)
        {
            var size = charBaseSize + (i < charRemainder ? 1 : 0);
            if (size <= 0) continue;

            charResult.Add(new string(chars, charIndex, size));
            charIndex += size;
        }

        return charResult;
    }

    public void StartGame(string phrase, int piecesCount, float moveSpeed, float? customTime = null)
    {
        gameRunning = true;
        currentCorrectIndex = 0;
        
        float gameTime = customTime ?? this.totalTime;
        this.totalTime = gameTime;
        timeLeft = gameTime;
        pieces = Split(phrase, piecesCount);


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
            
            currentCorrectIndex++; // Увеличиваем ожидаемый индекс
            
            pressedButton.Hide(); // Удаляем кнопку

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
