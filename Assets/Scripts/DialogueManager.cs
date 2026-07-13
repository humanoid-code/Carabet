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
    [Header("���� ������� JSON")]
    [SerializeField] private TextAsset inkJsonAsset;
    private Story story;

    [Header("����� � UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("������ �� ���������")]
    [SerializeField] private GameObject characterSprite;

    [Header("���� �������� ����������")]
    [SerializeField] private List<CharacterData> charactersList;

    [Header("���������")]
    [SerializeField] private MinigameManager minigameManager;

    [Header("����")]
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

    // ����� ��� ������� ������ � ������ ������� �������
    public void RestartDialogueStory()
    {
        if (inkJsonAsset != null)
        {
            story = new Story(inkJsonAsset.text);
            story.ChoosePathString("start");

            // ���������� ����� � GameManager �� ��������� ��� �����������
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

    // ����� �������: ������� ����������� ��� ������
    private void OnMinigameWin()
    {
        ignorePositivesInNextNode = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        RefreshView();
    }

    // ����� ��������: �������� ������������� ������
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

                    // ���� ������ "������ �������", ������ ������������� ��� ������� ������
                    if (chosenPhrase.Contains("������ �������"))
                    {
                        RestartDialogueStory();
                        return;
                    }

                    // �������� �� ������ ����-����
                    if (sourceKnot != null && sourceKnot.ToLower().Contains("dialogue") && ChoiceGivesPlus(choiceTags))
                    {
                        Debug.Log($"[DIAG] ����� ��� ����� � {sourceKnot}! ��������� ����-����.");

                        if (dialoguePanel != null) dialoguePanel.SetActive(false);

                        story.ChooseChoiceIndex(index);

                        if (minigameManager != null)
                        {
                            minigameManager.StartGame(chosenPhrase, 5);
                        }
                    }
                    else
                    {
                        Debug.Log($"[DIAG] ������� ��� ������ � {sourceKnot} (����-���� ���������).");
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

            // �������� ���� BLOCK:
            if (key == "money" || key == "law" || key == "rating" || key == "workers_wellbeing")
            {
                if (ignorePositivesInNextNode && value.StartsWith("+"))
                {
                    Debug.Log($"[STATS] ��������! ���� ��� '{key}' ({value}) ��������������.");
                    continue; // ��������� � ���������� ����, ���� ���� �� ����������!
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