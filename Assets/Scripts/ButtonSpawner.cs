using System.Collections.Generic;
using UnityEngine;

public class ButtonSpawner : MonoBehaviour
{
    [SerializeField] private GameObject phraseButtonPrefab;
    private List<PhraseButton> activeButtons = new List<PhraseButton>();

    // Убрали [SerializeField] MinigameManager targetManager; — он больше не нужен!

    // ТЕПЕРЬ МЕТОД ПРИНИМАЕТ ССЫЛКУ НА ВЫЗВАВШИЙ ЕГО МЕНЕДЖЕР (MinigameManager manager)
    public List<PhraseButton> SpawnButtons(List<string> pieces, float gameSpeed, MinigameManager manager)
    {
        if (phraseButtonPrefab == null)
        {
            Debug.LogError($"[БЕДА] На объекте {gameObject.name} в скрипте ButtonSpawner НЕ ЗАДАН префаб кнопки!", this);
            return new List<PhraseButton>();
        }

        // Очищаем старые
        foreach (var btn in activeButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        activeButtons.Clear();

        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject newBtnObj = Instantiate(phraseButtonPrefab, transform);
            PhraseButton newBtn = newBtnObj.GetComponent<PhraseButton>();

            if (newBtn == null) continue;

            // Передаем manager напрямую из параметров метода!
            newBtn.Initialize(pieces[i], i, manager);

            MovingPhraseButton mover = newBtnObj.GetComponent<MovingPhraseButton>();
            if (mover != null)
            {
                mover.SetSpeed(gameSpeed);
            }
            else
            {
                Debug.LogWarning("На кнопке нет скрипта MovingPhraseButton!");
            }
            activeButtons.Add(newBtn);

            // Спавним все кнопки строго по центру родителя (0, 0)
            float posX = 0f;
            float posY = -450 + 300 * i;

            RectTransform rect = newBtnObj.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = new Vector2(posX, posY);
        }
        return activeButtons;
    }

    public void ClearAll()
    {
        foreach (var btn in activeButtons) if (btn != null) Destroy(btn.gameObject);
        activeButtons.Clear();
    }
}
