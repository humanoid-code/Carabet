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
    [Header("Файл диалога JSON")]
    [SerializeField] private TextAsset inkJsonAsset;
    private Story story;

    [Header("Связи с UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Ссылка на персонажа")]
    [SerializeField] private GameObject characterSprite;

    [Header("База картинок персонажей")]
    [SerializeField] private List<CharacterData> charactersList;

    [Header("Менеджеры")]
    [SerializeField] private MinigameManager minigameManager;

    [Header("Окна")]
    [SerializeField] private GameObject dialoguePanel;

    private GameManager gameManager;
    private bool ignorePositivesInNextNode = false;

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

    // Метод для полного сброса и старта истории сначала
    private void RestartDialogueStory()
    {
        if (inkJsonAsset != null)
        {
            story = new Story(inkJsonAsset.text);
            story.ChoosePathString("start");

            // Сбрасываем статы в GameManager на дефолтные при перезапуске
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

    // ИГРОК ВЫИГРАЛ: ресурсы начисляются как обычно
    private void OnMinigameWin()
    {
        ignorePositivesInNextNode = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshView();
    }

    // ИГРОК ПРОИГРАЛ: включаем игнорирование плюсов
    private void OnMinigameLose()
    {
        ignorePositivesInNextNode = true;
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

    private bool ChoiceGivesPlus(List<string> tags)
    {
        if (tags == null || tags.Count == 0) return false;

        foreach (string tag in tags)
        {
            string[] splitTag = tag.Split(':');
            string key = splitTag[0].Trim().ToLower();
            string value = splitTag.Length > 1 ? splitTag[1].Trim() : "";

            if (key == "money" || key == "law" || key == "rating" || key == "workers_wellbeing")
            {
                if (value.StartsWith("+"))
                {
                    return true;
                }
            }
        }
        return false;
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
        ignorePositivesInNextNode = false;

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
                    string sourceKnot = choice.sourcePath;
                    List<string> choiceTags = choice.tags;

                    // Если нажали "ИГРАТЬ СНАЧАЛА", просто перезапускаем всю историю заново
                    if (chosenPhrase.Contains("ИГРАТЬ СНАЧАЛА"))
                    {
                        RestartDialogueStory();
                        return;
                    }

                    // Проверка на запуск мини-игры
                    if (sourceKnot != null && sourceKnot.ToLower().Contains("dialogue") && ChoiceGivesPlus(choiceTags))
                    {
                        Debug.Log($"[DIAG] Выбор дал бонус в {sourceKnot}! Запускаем мини-игру.");

                        if (dialoguePanel != null) dialoguePanel.SetActive(false);

                        story.ChooseChoiceIndex(index);

                        if (minigameManager != null)
                        {
                            minigameManager.StartGame(chosenPhrase, 4, 250f);
                        }
                    }
                    else
                    {
                        Debug.Log($"[DIAG] Обычный шаг сюжета в {sourceKnot} (мини-игра пропущена).");
                        story.ChooseChoiceIndex(index);
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

            // ДОБАВИТЬ ЭТОТ BLOCK:
            if (key == "money" || key == "law" || key == "rating" || key == "workers_wellbeing")
            {
                if (ignorePositivesInNextNode && value.StartsWith("+"))
                {
                    Debug.Log($"[STATS] Проигрыш! Плюс для '{key}' ({value}) проигнорирован.");
                    continue; // Переходим к следующему тегу, этот плюс не применится!
                }
            }

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