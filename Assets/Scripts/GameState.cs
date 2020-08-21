using UnityEngine;

public class GameState : MonoBehaviour
{
    public int Money = 0;
    public bool HasDoughMachine = false;
    public bool HasSwarm = false;
    public bool HasSupplyBot = false;
    public int NumToppersReplaced = 0;
    public GameManager.GameStage CurrentStage;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public int NumPurchases()
    {
        var result = 0;
        if (HasDoughMachine) result++;
        if (HasSwarm) result++;
        if (HasSupplyBot) result++;
        result += NumToppersReplaced;
        return result;
    }
}
