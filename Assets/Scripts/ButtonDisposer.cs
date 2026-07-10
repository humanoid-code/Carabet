using System.Collections.Generic;
using UnityEngine;

public class ButtonDisposer : MonoBehaviour
{
    private List<PhraseButton> activeButtons = new List<PhraseButton>();

    // Вызывать из MinigameManager при старте игры, передавая созданные кнопки
    public void RegisterButtons(List<PhraseButton> buttons)
    {
        activeButtons = buttons;
    }

    // Вызывать при TimeOut (истечении времени)
    public void DestroyAllButtons()
    {
        foreach (var btn in activeButtons)
        {
            if (btn != null && btn.gameObject != null)
            {
                Destroy(btn.gameObject);
            }
        }
        activeButtons.Clear();
    }
}

