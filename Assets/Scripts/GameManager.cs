using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Статы")]
    public int rating = 0;
    public int money = 0;
    public int law = 0;
    public int workers_wellbeing = 0;

    [Header("Статы на экране")]
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI lawText;
    public TextMeshProUGUI workers_wellbeingText;

    [Header("UI Ссылки на окна")]

    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject gameplayWindow;
    [SerializeField] private GameObject nightResultsWindow;

    [Header("Компоненты игры")]
    [SerializeField] private GameObject location1Background;
    [SerializeField] private GameObject location2Background;
    [SerializeField] private DialogueManager dialogueManager;

    void Start()
    {
        SetState(GameState.MainMenu);
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
                mainMenuWindow.SetActive(false);
                mainMenuWindow.SetActive(true);
                break;

            case GameState.Location1:
                gameplayWindow.SetActive(true);
                location1Background.SetActive(true);
                location2Background.SetActive(false);
                // Тут потом запустим диалоги утренние
                break;

            case GameState.Location2:
                gameplayWindow.SetActive(true);
                location1Background.SetActive(false);
                location2Background.SetActive(true);
                // тут вечерние
                break;

            case GameState.NightResults:
                nightResultsWindow.SetActive(true);
                break;
        }
    }


    public void UpdateStatsUI()
    {
        ratingText?.SetText("Рейтинг заведения: " + rating);
        workers_wellbeingText?.SetText("Удовлетворенность сотрудников: " + workers_wellbeing);
        lawText?.SetText("Легальность: " + law);
        moneyText?.SetText("Бабосы: " + money);
    }

    // интерфейс

    // старт в менюшке -> утро
    public void OnStartGameButton()
    {
        SetState(GameState.Location1);
        UpdateStatsUI();
    }

    // утро -> вечер
    public void OnLocation1Finished()
    {
        SetState(GameState.Location2);
    }

    // вечер -> меню о конце дня
    public void OnLocation2Finished()
    {
        SetState(GameState.NightResults);
    }

    // конец дня -> утро
    public void OnNextDayButton()
    {
        SetState(GameState.Location1);
    }

    public void OnExitGameButton()
    {
        Debug.Log("Игрок вышел из игры!");
        Application.Quit();
    }
}
