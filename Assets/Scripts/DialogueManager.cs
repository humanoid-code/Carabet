using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections.Generic;

[System.Serializable]
public struct CharacterData
{
    public string nameId;
    public Sprite sprite;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Файл истории JSON")]
    [SerializeField] private TextAsset inkJsonAsset;
    private Story story;

    [Header("Текст и UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Ссылки на персонажей")]
    [SerializeField] private GameObject characterSprite;

    [Header("База данных персонажей")]
    [SerializeField] private List<CharacterData> charactersList;

    [Header("Мини-игра")]
    [SerializeField] private MinigameManager minigameManager;

    [Header("Окна")]
    [SerializeField] private GameObject dialoguePanel;

    private GameManager gameManager;

    // Новые переменные для простой логики "каждый второй" и отката
    private int dialogueCounter = 0;
    private string savedStoryState = "";
    private string lastChosenPhrase = "";

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        RestartDialogueStory();

        if (minigameManager != null)
        {
            minigameManager.OnGameFinished += OnMinigameWin;
            minigameManager.OnTimeOut += OnMinigameLose;
        }
    }

    private void RestartDialogueStory()
    {
        if (inkJsonAsset != null)
        {
            story = new Story(inkJsonAsset.text);
            story.ChoosePathString("start");
            dialogueCounter = 0;

            if (gameManager != null)
            {
                gameManager.law = 50;
                gameManager.money = 100;
                gameManager.workers_wellbeing = 50;
                gameManager.rating = 1;
                gameManager.UpdateStatsUI();
            }

            RefreshView();
        }
    }

    // ПОБЕДА: Выбор подтверждается, мы просто показываем панель
    private void OnMinigameWin()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshView();
    }

    // ПРОИГРЫШ: Выбор аннулируется, откатываем Ink назад
    private void OnMinigameLose()
    {
        if (!string.IsNullOrEmpty(savedStoryState))
        {
            story.state.LoadJson(savedStoryState); // Полный откат состояния Ink до нажатия
            dialogueCounter--; // Возвращаем счетчик шагов назад
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshView();
    }

    private void OnDestroy()
    {
        if (minigameManager != null)
        {
            minigameManager.OnGameFinished -= OnMinigameWin;
            minigameManager.OnTimeOut -= OnMinigameLose;
        }
    }

    void RefreshView()
    {
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        string text = "";
        while (story.canContinue)
        {
            text += story.Continue();
            HandleTags(story.currentTags);
        }
        dialogueText.text = text;

        if (story.currentChoices.Count > 0)
        {
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                GameObject button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;

                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    string chosenPhrase = choice.text;
                    string upperPhrase = chosenPhrase.ToUpper(); // Переводим в верхний регистр для надежности проверки

                    // 1. Технический рестарт игры
                    if (upperPhrase.Contains("НАЧАТЬ СНАЧАЛА") || upperPhrase.Contains("ИГРАТЬ СНАЧАЛА"))
                    {
                        RestartDialogueStory();
                        return;
                    }

                    // Сохраняем состояние ДЛЯ ОТКАТА на случай проигрыша
                    savedStoryState = story.state.ToJson();
                    lastChosenPhrase = chosenPhrase;

                    // Делаем выбор в Ink
                    story.ChooseChoiceIndex(index);

                    // 2. ПРОВЕРКА: Является ли кнопка технической/обучающей?
                    // Если на кнопке написано "ДАЛЕЕ", "ПРОПУСТИТЬ ОБУЧЕНИЕ" или "ИГРАТЬ" — это НЕ сюжетный выбор
                    bool isTechnicalClick = upperPhrase.Contains("ДАЛЕЕ") ||
                                            upperPhrase.Contains("ПРОПУСТИТЬ") ||
                                            upperPhrase.Contains("ИГРАТЬ");

                    if (!isTechnicalClick)
                    {
                        // Увеличиваем счётчик только для реальных выборов персонажей (Выбор 1.1, Ответить и т.д.)
                        dialogueCounter++;
                        Debug.Log($"[МЕХАНИКА] Сюжетный выбор! Шаг счетчика: {dialogueCounter}");
                    }
                    else
                    {
                        Debug.Log($"[МЕХАНИКА] Пропуск счетчика (нажата техническая кнопка: {chosenPhrase})");
                    }

                    // 3. Запускаем мини-игру только на каждый второй РЕАЛЬНЫЙ выбор
                    if (!isTechnicalClick && dialogueCounter % 2 == 0)
                    {
                        Debug.Log($"[МЕХАНИКА] Шаг {dialogueCounter}. Запуск мини-игры.");

                        // Скрываем диалог и уходим играть
                        if (dialoguePanel != null) dialoguePanel.SetActive(false);
                        if (minigameManager != null)
                        {
                            minigameManager.StartGame(lastChosenPhrase, 5);
                        }
                    }
                    else
                    {
                        // Обычный шаг текста (для "Далее", обучения или нечетных выборов)
                        RefreshView();
                    }
                });
            }
        }
    }

    private void HandleTags(List<string> tags)
    {
        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            string key = splitTag[0].Trim();
            string value = splitTag.Length > 1 ? splitTag[1].Trim() : "";

            switch (key)
            {
                case "char":
                    if (value == "none")
                    {
                        characterSprite.SetActive(false);
                    }
                    else
                    {
                        characterSprite.SetActive(true);
                        Image charImage = characterSprite.GetComponent<Image>();
                        if (charImage != null)
                        {
                            Sprite foundSprite = null;
                            foreach (var charData in charactersList)
                            {
                                if (charData.nameId == value)
                                {
                                    foundSprite = charData.sprite;
                                    break;
                                }
                            }

                            if (foundSprite != null)
                            {
                                charImage.sprite = foundSprite;
                                charImage.color = Color.white;
                            }
                        }
                    }
                    break;

                case "money":
                    if (gameManager == null) break;
                    if (value.StartsWith("=")) gameManager.money = int.Parse(value.Substring(1));
                    else gameManager.money += int.Parse(value);
                    gameManager.UpdateStatsUI();
                    break;

                case "law":
                    if (gameManager == null) break;
                    if (value.StartsWith("=")) gameManager.law = int.Parse(value.Substring(1));
                    else gameManager.law += int.Parse(value);
                    gameManager.UpdateStatsUI();
                    break;

                case "rating":
                    if (gameManager == null) break;
                    if (value.StartsWith("=")) gameManager.rating = int.Parse(value.Substring(1));
                    else gameManager.rating += int.Parse(value);
                    gameManager.UpdateStatsUI();
                    break;

                case "workers_wellbeing":
                    if (gameManager == null) break;
                    if (value.StartsWith("=")) gameManager.workers_wellbeing = int.Parse(value.Substring(1));
                    else gameManager.workers_wellbeing += int.Parse(value);
                    gameManager.UpdateStatsUI();
                    break;
            }
        }
    }
}