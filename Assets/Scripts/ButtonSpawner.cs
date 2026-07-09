using System.Collections.Generic;
using UnityEngine;

public class ButtonSpawner : MonoBehaviour
{
    [SerializeField] private GameObject phraseButtonPrefab;
    
    private List<PhraseButton> activeButtons = new List<PhraseButton>();

    [SerializeField] MinigameManager targetManager;
    // Метод, который вызывает MinigameManager, когда нужно создать кнопки
    public void SpawnButtons(List<string> pieces, float gameSpeed)
    {
        // Очищаем старые
        foreach (var btn in activeButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        activeButtons.Clear();

        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject newBtnObj = Instantiate(phraseButtonPrefab, transform); // Родитель - сам Holder
            PhraseButton newBtn = newBtnObj.GetComponent<PhraseButton>();
            
            if (newBtn == null) continue;

            newBtn.Initialize(pieces[i], i, targetManager); 
            MovingPhraseButton mover = newBtnObj.GetComponent<MovingPhraseButton>();
            if (mover != null)
            {
                mover.SetSpeed(gameSpeed); // ВОТ ЗДЕСЬ ЗАДАЕТСЯ СКОРОСТЬ
            }
            else
            {
                Debug.LogWarning("На кнопке нет скрипта MovingPhraseButton!");
            }
            activeButtons.Add(newBtn);

            // ТВОИ КООРДИНАТЫ
            float posX = -300f + (250f * i);
            float posY = -100f;
            
            RectTransform rect = newBtnObj.GetComponent<RectTransform>();
            if(rect != null) rect.anchoredPosition = new Vector2(posX, posY);
        }
    }

    // Метод для очистки, если игра закончилась
    public void ClearAll()
    {
        foreach (var btn in activeButtons) Destroy(btn.gameObject);
        activeButtons.Clear();
    }
}
