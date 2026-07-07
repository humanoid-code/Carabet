using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime; // Обязательно!

public class DialogueManager : MonoBehaviour
{
    [Header("Файл диалога JSON")]
    [SerializeField] private TextAsset inkJsonAsset;
    private Story story;

    [Header("Связи с UI")]
    [SerializeField] private TextMeshProUGUI dialogueText; // или просто Text, если юзаешь старый UI
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    void Start()
    {
        Debug.Log("=== ТЕСТ: МЕНЕДЖЕР ЗАПУСТИЛСЯ ===");
        if (inkJsonAsset != null)
        {
            Debug.Log("=== ТЕСТ: JSON НАЙДЕН ===");
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

        // Если есть варианты выбора — создаем кнопки
        Debug.Log("Количество выборов в Ink сейчас: " + story.currentChoices.Count);
        if (story.currentChoices.Count > 0)
        {
            for (int i = 0; i < story.currentChoices.Count; i++)
            {
                Choice choice = story.currentChoices[i];
                GameObject button = Instantiate(choiceButtonPrefab, choicesContainer);
                button.transform.localPosition = Vector3.zero; 
                button.transform.localScale = Vector3.one;
                Debug.Log("ФИЗИЧЕСКИ СОЗДАНА КНОПКА: " + choice.text + " внутри " + choicesContainer.name);
                // Пишем текст на кнопке
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;

                // Назначаем кнопке действие при клике
                int index = i;
                button.GetComponent<Button>().onClick.AddListener(() => {
                    story.ChooseChoiceIndex(index);
                    RefreshView();
                });
            }
        }
    }
}
