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

    [Header("�����")]
    public int rating = 0;
    public int money = 0;
    public int law = 0;
    public int workers_wellbeing = 0;

    [Header("����� �� ������")]
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI lawText;
    public TextMeshProUGUI workers_wellbeingText;
    private InputAction escapeAction;

    [Header("UI ������ �� ����")]

    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject gameplayWindow;
    [SerializeField] private GameObject nightResultsWindow;

    [Header("���������� ����")]
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

    void Update()
    {
        // Проверяем нажатие Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Если мы не в главном меню, возвращаемся в него
            if (currentState != GameState.MainMenu)
            {
                ReturnToMainMenu();
            }
        }
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
                // ��� ����� �������� ������� ��������
                break;

            case GameState.Location2:
                gameplayWindow.SetActive(true);
                location1Background.SetActive(false);
                location2Background.SetActive(true);
                // ��� ��������
                break;

            case GameState.NightResults:
                nightResultsWindow.SetActive(true);
                break;
        }
    }


    public void UpdateStatsUI()
    {
        ratingText?.SetText("������� ���������: " + rating);
        workers_wellbeingText?.SetText("����������������� �����������: " + workers_wellbeing);
        lawText?.SetText("�����������: " + law);
        moneyText?.SetText("������: " + money);
    }

    // ���������

    // ����� � ������� -> ����
    public void OnStartGameButton()
    {
        SetState(GameState.Location1);
        UpdateStatsUI();
    }

    // ���� -> �����
    public void OnLocation1Finished()
    {
        SetState(GameState.Location2);
    }

    // ����� -> ���� � ����� ���
    public void OnLocation2Finished()
    {
        SetState(GameState.NightResults);
    }

    // ����� ��� -> ����
    public void OnNextDayButton()
    {
        SetState(GameState.Location1);
    }

    public void OnExitGameButton()
    {
        Debug.Log("����� ����� �� ����!");
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
