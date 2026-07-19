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
    [SerializeField] private Slider timeSlider;
    [SerializeField] private float totalTime = 10f;
    [SerializeField] private int defaultPieces = 3;
    [SerializeField] private float defaultVelocity = 200f;
    [SerializeField] private float multiplierVelocity = 100f;
    [SerializeField] private int testEasiness = 1;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource sfxSource;       // Для разовых звуков (победа/проигрыш)
    [SerializeField] private AudioSource timerSource;     // Для тиканья таймера (поставь ему Loop в инспекторе!)
    [SerializeField] private AudioClip winSound;          // Звук победы
    [SerializeField] private AudioClip loseSound;         // Звук проигрыша/таймаута
    [SerializeField] private AudioClip timerTickSound;    // Звук тиканья таймера

    [SerializeField] private GameObject statsPanel;

    private float timeLeft;
    public event Action OnGameFinished;      // Победа (собрали все слова)
    public event Action<string> OnWrongAnswer;// Ошибка
    public event Action OnTimeOut;            // Проигрыш (время вышло)

    int piecesCount;
    float moveSpeed;
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

    public void StartGame(string phrase, int easiness = -1, float? customTime = null)
    {
        easiness = testEasiness;
        if (easiness != -1)
        {
            piecesCount = defaultPieces + easiness;
            moveSpeed = defaultVelocity + multiplierVelocity * easiness;
        }
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

            ButtonDisposer disposer = GetComponent<ButtonDisposer>();
            if (disposer != null) disposer.RegisterButtons(createdButtons);
        }

        Debug.Log($"Игра началась! Время: {timeLeft} сек.");

        if (statsPanel != null)
        {
            statsPanel.SetActive(false);
        }


        // Сбрасываем полосу времени
        if (timeSlider != null) timeSlider.value = 1f;
        if (timeSlider != null) timeSlider.gameObject.SetActive(true);

        // --- ЗВУКОВОЙ ДАКИНГ (ПРИГЛУШЕНИЕ МУЗЫКИ) ---
        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.SetVolumeSmooth(0.15f, 0.8f);
        }

        // Запуск тиканья таймера
        if (timerSource != null && timerTickSound != null)
        {
            timerSource.clip = timerTickSound;
            timerSource.Play();
        }
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
            currentCorrectIndex++; // Увеличиваем ожидаемый индекс

            RenderFullPhrase();
            Debug.Log($"Current correct index {currentCorrectIndex}!");

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
            OnWrongAnswer?.Invoke("Неверный порядок слов!");
        }
    }

    private void EndGame()
    {
        gameRunning = false;
        Debug.Log("Мини-игра окончена! Победа.");

        StopTimerSound();

        // --- ВОЗВРАТ МУЗЫКИ И ЗВУК ПОБЕДЫ ---
        if (BackgroundMusic.Instance != null)
        {
            float normalVol = BackgroundMusic.Instance.GetDefaultVolume();
            BackgroundMusic.Instance.SetVolumeSmooth(normalVol, 1.5f);
        }

        if (sfxSource != null && winSound != null)
        {
            sfxSource.PlayOneShot(winSound);
        }
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }
        if (timeSlider != null) timeSlider.gameObject.SetActive(false); // Прячем вместо удаления
        OnGameFinished?.Invoke();
    }

    private void TimeOut()
    {
        gameRunning = false;
        Debug.Log("Время вышло! ПРОИГРЫШ.");

        StopTimerSound();

        // --- ВОЗВРАТ МУЗЫКИ И ЗВУК ПРОИГРЫША ---
        if (BackgroundMusic.Instance != null)
        {
            float normalVol = BackgroundMusic.Instance.GetDefaultVolume();
            BackgroundMusic.Instance.SetVolumeSmooth(normalVol, 1.5f);
        }

        if (sfxSource != null && loseSound != null)
        {
            sfxSource.PlayOneShot(loseSound);
        }
        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }

        ButtonDisposer disposer = GetComponent<ButtonDisposer>();
        if (disposer != null)
        {
            disposer.DestroyAllButtons();
        }

        if (timeSlider != null) timeSlider.gameObject.SetActive(false); // Прячем вместо удаления

        OnTimeOut?.Invoke();
    }

    private void StopTimerSound()
    {
        if (timerSource != null && timerSource.isPlaying)
        {
            timerSource.Stop();
        }
    }
}