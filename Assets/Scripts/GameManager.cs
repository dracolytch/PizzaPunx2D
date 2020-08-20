using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class IngredientMax
{
    public GameManager.GameStage stage;
    public int maxIngredients;
}

public class GameManager : MonoBehaviour
{
    public enum GameStage { early, mid, late, end }

    public GameObject GameStatePrefab;
    public OrderManager orderManager;
    //public MoneyDisplay moneyDisplay;
    public PizzaSpriteManager pizzaSpriteManager;
    public IngredientSpriteManager ingredientSpriteManager;

    public List<PlacementSocket> PizzaSockets;
    public List<PlacementSocket> IngredientSockets;

    public float StageLengthSeconds = 240;
    public float OrderCutoffTime = 210;
    public float CurrentStageTime = 0f;

    public GameStage currentGameStage = GameStage.early;

    public List<IngredientMax> IngredientsPerStage;

    public float CurrentMoney;
    public AudioClip EarlyMusic;
    public AudioClip MidMusic;
    public AudioClip LateMusic;
    public AudioSource BgmAudioSource;
    public AudioSource SfxAudioSource;
    public WorkerManager workerManager;

    public UnityIntEvent OnMoneyChanged;
    public UnityEvent OnLevelDone;

    GameState currentGameState;

    private void Start()
    {
        if (orderManager == null) Debug.LogError("Game manager has no order manager!");
        if (pizzaSpriteManager == null) Debug.LogError("Game manager has no pizza sprite manager!");
        if (BgmAudioSource == null) Debug.LogError("Can't play background music");
        if (SfxAudioSource == null) Debug.LogError("Can't play global sound effects");
        if (workerManager == null) Debug.LogError("Can't find worker manager");

        currentGameState = FindObjectOfType<GameState>();
        if (currentGameState == null)
        {
            Debug.Log("No current state defined, defaulting");
            var gso = Instantiate(GameStatePrefab);
            currentGameState = gso.GetComponent<GameState>();
        }

        if (currentGameState == null) Debug.LogError("Still null? Not sure what to do");
        else
        {
            CurrentMoney = currentGameState.Money;
            currentGameStage = currentGameState.CurrentStage;

            AddMoney(0); // Force money display to change

            PlayBackgroundMusic(currentGameState.CurrentStage);

            workerManager.UpgradeWorkers(currentGameState);
        }
    }

    private void PlayBackgroundMusic(GameStage stage)
    {
        switch (stage)
        {
            case GameStage.early:
                BgmAudioSource.PlayOneShot(EarlyMusic);
                break;

            case GameStage.mid:
                BgmAudioSource.PlayOneShot(MidMusic);
                break;

            case GameStage.late:
                BgmAudioSource.PlayOneShot(LateMusic);
                break;
        }
    }

    public void PurchaseSwarm(int amount)
    {
        if (amount < CurrentMoney && currentGameState.HasSwarm == false)
        {
            AddMoney(-amount);
            currentGameState.HasSwarm = true;
        }
    }

    public void PurchaseDoughMachine(int amount)
    {
        if (amount < CurrentMoney && currentGameState.HasDoughMachine == false)
        {
            AddMoney(-amount);
            currentGameState.HasDoughMachine = true;
        }
    }

    public void PurchaseSupplyBot(int amount)
    {
        if (amount < CurrentMoney && currentGameState.HasSupplyBot == false)
        {
            AddMoney(-amount);
            currentGameState.HasSupplyBot = true;
        }
    }

    public void PurchaseTopper(int amount)
    {
        if (amount < CurrentMoney && currentGameState.NumToppersReplaced < 3)
        {
            AddMoney(-amount);
            currentGameState.NumToppersReplaced++;
        }
    }

    public void GoToNextScene()
    {
        currentGameState.Money = (int)CurrentMoney;
        switch (currentGameStage)
        {
            case GameStage.early:
                currentGameState.CurrentStage = GameStage.mid;
                break;

            case GameStage.mid:
                currentGameState.CurrentStage = GameStage.late;
                break;

            case GameStage.late:
                currentGameState.CurrentStage = GameStage.end;
                break;
        }
        SceneManager.LoadScene(2);
    }

    private void Update()
    {
        CurrentStageTime += Time.deltaTime;
    }

    public bool TakingNewOrders()
    {
        return CurrentStageTime < OrderCutoffTime;
    }

    public int MaxToppingsThisStage()
    {
        return IngredientsPerStage.Where(a => a.stage == currentGameStage).First().maxIngredients;
    }

    public void AddMoney(int newMoney)
    {
        CurrentMoney += newMoney;
        if (OnMoneyChanged != null) OnMoneyChanged.Invoke((int)CurrentMoney);
    }

    public void CheckIfLevelDone()
    {
        if (CurrentStageTime > OrderCutoffTime && orderManager.NumOpenOrders() == 0)
        {
            if (OnLevelDone != null) OnLevelDone.Invoke();
        }
    }
}
