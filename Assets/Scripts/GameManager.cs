using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Location1,
        Location2,
        NightResults
    }

    public GameState currentState = GameState.MainMenu;

    [Header("Статистика")]
    public int rating = 0;
    public int money = 0;
    public int law = 0;
    public int workers_wellbeing = 0;
    

    [Header("Текст для статистики")]
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI lawText;
    public TextMeshProUGUI workers_wellbeingText;
    private InputAction escapeAction;

    [Header("UI окна для сцен")]

    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject gameplayWindow;
    [SerializeField] private GameObject nightResultsWindow;

    [Header("Фоновые изображения")]
    [SerializeField] private GameObject location1Background;
    [SerializeField] private GameObject location2Background;
    [SerializeField] private DialogueManager dialogueManager;

    void Start()
    {
        SetState(GameState.MainMenu);

                // Инициализируем действие для Escape
        escapeAction = new InputAction("Escape", binding: "<Keyboard>/escape");
        
        // Подписываемся на событие
        escapeAction.performed += OnEscapePerformed;
        
        // Включаем действие
        escapeAction.Enable();
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        mainMenuWindow.SetActive(false);
        gameplayWindow.SetActive(false);
        nightResultsWindow.SetActive(false);

        switch (currentState)
        {
            case GameState.MainMenu:
                mainMenuWindow.SetActive(true);
                break;

            case GameState.Location1:
                gameplayWindow.SetActive(true);
                location1Background.SetActive(true);
                location2Background.SetActive(false);
                break;

            case GameState.Location2:
                gameplayWindow.SetActive(true);
                location1Background.SetActive(false);
                location2Background.SetActive(true);
                break;

            case GameState.NightResults:
                nightResultsWindow.SetActive(true);
                break;
        }
    }
    // Опциональный метод для сброса игры (можно добавить, если нужно)
    private void ResetGame()
    {
        rating = 0;
        money = 0;
        law = 0;
        workers_wellbeing = 0;
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        ratingText?.SetText("Рейтинг предпринимателя: " + rating);
        workers_wellbeingText?.SetText("Благополучие работников: " + workers_wellbeing);
        lawText?.SetText("Законопослушность: " + law);
        moneyText?.SetText("Деньги: " + money);
    }

    // Кнопки

    // Меню -> Локация1
    public void OnStartGame ()
    {
        ResetGame();
        dialogueManager.RestartDialogueStory();
        SetState(GameState.Location1);
        UpdateStatsUI();
    }

    public void OnContinueButton()
    {
        SetState(GameState.Location1);
        UpdateStatsUI();
    }

    // Локация1 -> Локация2
    public void OnLocation1Finished()
    {
        SetState(GameState.Location2);
    }

    // Локация2 -> Ночь с итогами
    public void OnLocation2Finished()
    {
        SetState(GameState.NightResults);
    }

    // Ночь с итогами -> Локация1
    public void OnNextDayButton()
    {
        SetState(GameState.Location1);
    }

    public void OnExitGameButton()
    {
        Debug.Log("Выход из игры навсегда!");
        Application.Quit();
    }

    private void ReturnToMainMenu()
    {
        // Если вы хотите сбрасывать прогресс при выходе, раскомментируйте:
        // ResetGame();
        
        SetState(GameState.MainMenu);
        UpdateStatsUI();
        Debug.Log("Возврат в главное меню по нажатию Escape");
    }
    void OnDestroy()
    {
        // Отписываемся и очищаем ресурсы
        if (escapeAction != null)
        {
            escapeAction.performed -= OnEscapePerformed;
            escapeAction.Disable();
            escapeAction.Dispose();
        }
    }

    private void OnEscapePerformed(InputAction.CallbackContext context)
    {
        // Если мы не в главном меню, возвращаемся в него
        if (currentState != GameState.MainMenu)
        {
            ReturnToMainMenu();
        }
    }
}