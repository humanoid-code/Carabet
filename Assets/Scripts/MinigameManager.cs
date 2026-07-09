using System;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Ссылка на объект, который создает кнопки ВНУТРИ Canvas")]
    [SerializeField] private ButtonSpawner buttonSpawner; 

    private List<string> pieces;
    // private List<PhraseButton> buttons; <-- УДАЛИТЬ, теперь этим занимается Spawner
    private int currentCorrectIndex;
    private bool gameRunning;
    private float moveSpeed;

    public event Action OnGameFinished;
    public event Action<string> OnWrongAnswer;

    void Start()
    {
        if (buttonSpawner == null)
        {
            Debug.LogError("Ошибка: Не назначен ButtonSpawner! Найди его в Canvas.");
        }
        StartGame("Мы дадим вам деньги завтра", 3, 100.0f);
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

    public void StartGame(string phrase, int piecesCount, float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
        gameRunning = true;
        currentCorrectIndex = 0;
        
        pieces = Split(phrase, piecesCount);

        // ГЛАВНОЕ ИЗМЕНЕНИЕ:
        // Мы НЕ создаем кнопки здесь. Мы просто говорим спавнеру: "Сделай их".
        if(buttonSpawner != null)
        {
            buttonSpawner.SpawnButtons(pieces, moveSpeed); // Передаем скорость движения кнопок
        }

        Debug.Log($"Игра началась. Ждем нажатий...");
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
        OnGameFinished?.Invoke();
    }
}
