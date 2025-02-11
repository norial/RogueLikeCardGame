using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public UIDocument UIDocument;
    public BoardManager boardManager;
    public PlayerController playerController;

    private ProgressBar _foodAmountBar;
    private VisualElement _gameOverPanel;
    private Label _gameOverMessage;
    private float _hungerLevel = 100f;
    private int _currentLevel = 1;
    private float _maxHunger = 100;

    public void FoodPicked(float amountOfFood)
    {
        ChangeFood(amountOfFood);
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        InitializeLevel();
        InitializeUiElements();
    }

    private void InitializeUiElements()
    {
        _foodAmountBar = UIDocument.rootVisualElement.Q<ProgressBar>("FoodAmountBar");
        _foodAmountBar.value = _hungerLevel;
        _foodAmountBar.title = $"HP: {_hungerLevel}/{_maxHunger}";
        _gameOverPanel = UIDocument.rootVisualElement.Q<VisualElement>("GameOverPanel");
        _gameOverMessage = _gameOverPanel.Q<Label>("GameOverText");

        _gameOverPanel.style.visibility = Visibility.Hidden;
    }

    public void InitializeLevel()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        TurnManager.OnTick += UpdateFoodBar;

        boardManager.Clean();
        boardManager.Init();
        playerController.Spawn(boardManager, new Vector2Int(1, 1));
        playerController.playerState = PlayerStateEnum.GameStarted;
        
        if (_currentLevel <= 10)
        {
            GameManager.Instance.boardManager.enemiesAmount = _currentLevel;
        }
        else
        {
            GameManager.Instance.boardManager.enemiesAmount = 10;
        }

        _currentLevel++;
    }

    public void StartNewGame()
    {
        _gameOverPanel.style.visibility = Visibility.Hidden;
        _hungerLevel = _maxHunger;
        _currentLevel = 1;
        playerController.hasBoots = false;
        playerController.hasBoots = false;
        _maxHunger = 100;
        InitializeLevel();
    }

    public void AddHealth(int amountOfHealth)
    {
        _maxHunger += amountOfHealth;
    }

    public void UpdateFoodBar()
    {
        if (_hungerLevel >= _maxHunger)
        {
            _foodAmountBar.value = _maxHunger;
            _foodAmountBar.title = $"HP: {_hungerLevel}/{_maxHunger}";
        }
        else
        {
            _foodAmountBar.value = _hungerLevel;
            _foodAmountBar.title = $"HP: {_hungerLevel}/{_maxHunger}";
        }
        if (_hungerLevel <= 0)
        {
            playerController.GameOver();
            _gameOverPanel.style.visibility = Visibility.Visible;
            _foodAmountBar.title = $"HP: {0}/{_maxHunger}";
            _gameOverMessage.text = $"Another stranger lost in Wastelands \n\nYou survived {_currentLevel} lands";
        }
    }

    private void OnTurnHappen()
    {
        ChangeFood(-5);
        Debug.Log($"{_hungerLevel} food left");
    }

    public void ChangeFood(float amountOfFood)
    {
        _hungerLevel += amountOfFood;
    }
}
