using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    [Header("Файл диалога JSON")]
    [SerializeField] private TextAsset inkJsonAsset;
    private Story story;

    [Header("Связи с UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    void Start()
    {
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
                button.GetComponent<Button>().onClick.AddListener(() => {
                    story.ChooseChoiceIndex(index);
                    RefreshView();
                });
            }
        }
    }
}
