using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;
using System.Collections.Generic;
[System.Serializable]
public struct CharacterData
{
    public string nameId; // Имя из Ink (manager, LiyoMern, GiyJarum и т.п.)
    public Sprite sprite; // Картинка персонажа из папки
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

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (inkJsonAsset != null)
        {
            story = new Story(inkJsonAsset.text);
            story.ChoosePathString("start");
            RefreshView();
        }
    }

    void RefreshView()
    {
        // Удаляем старые кнопки
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);

        // Читаем текст из Ink, пока он не кончится или не упрется в выбор
        string text = "";
        while (story.canContinue)
        {
            text += story.Continue();

            HandleTags(story.currentTags);
        }
        dialogueText.text = text;

        // Если есть варианты выбора, то создаем кнопки
        if (story.currentChoices.Count > 0)
        {
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                GameObject button = Instantiate(choiceButtonPrefab, choicesContainer);
                //Instantiate(1, 2) делает копию префаба (1) в (2)
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text; // текст на кнопке

                // Назначаем кнопке действие при клике (чоткая короткая запись через лямбду)
                int index = i; //ОБЯЗАТЕЛЬНО
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    story.ChooseChoiceIndex(index);
                    RefreshView();
                });
            }
        }
    }

    private void HandleTags(System.Collections.Generic.List<string> tags)
    {
        foreach (string tag in tags)
        {
            // Бьем тег по двоеточию (например, "money:-50" станет ["money", "-50"])
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

                        // Достаем компонент Image из нашего грейбокса
                        Image charImage = characterSprite.GetComponent<Image>();
                        if (charImage != null)
                        {
                            // Ищем в списке нужный спрайт по имени из Ink
                            Sprite foundSprite = null;
                            foreach (var charData in charactersList)
                            {
                                if (charData.nameId == value)
                                {
                                    foundSprite = charData.sprite;
                                    break;
                                }
                            }

                            // Если нашли картинку — подставляем её!
                            if (foundSprite != null)
                            {
                                charImage.sprite = foundSprite;
                                charImage.color = Color.white; // Убираем серость/прозрачность, если была
                            }
                            else
                            {
                                Debug.LogWarning($"Картинка для персонажа '{value}' не найдена в Characters List!");
                            }
                        }
                    }
                    break;

                case "money":
                    if (value.StartsWith("="))
                    {
                        string cleanValue = value.Substring(1);
                        gameManager.money = int.Parse(cleanValue);
                    }
                    else
                    {
                        gameManager.money += int.Parse(value);
                    }
                    gameManager.UpdateStatsUI();
                    break;

                case "law":
                    if (value.StartsWith("="))
                    {
                        string cleanValue = value.Substring(1);
                        gameManager.law = int.Parse(cleanValue);
                    }
                    else
                    {
                        gameManager.law += int.Parse(value);
                    }
                    gameManager.UpdateStatsUI();
                    break;

                case "rating":
                    if (value.StartsWith("="))
                    {
                        string cleanValue = value.Substring(1);
                        gameManager.rating = int.Parse(cleanValue);
                    }
                    else
                    {
                        gameManager.rating += int.Parse(value);
                    }
                    gameManager.UpdateStatsUI();
                    break;

                case "workers_wellbeing":
                    if (value.StartsWith("="))
                    {
                        string cleanValue = value.Substring(1);
                        gameManager.workers_wellbeing = int.Parse(cleanValue);
                    }
                    else
                    {
                        gameManager.workers_wellbeing += int.Parse(value);
                    }
                    gameManager.UpdateStatsUI();
                    break;
            }
        }
    }
}
