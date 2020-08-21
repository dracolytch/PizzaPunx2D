using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using static ShowReview;

public class OrderManager : MonoBehaviour
{
    public float basePizzaFee = 100;
    public float perToppingFee = 10;
    public int PizzaTrashPenalty = -20;
    public float penaltyForMissedReview = 0.5f;
    public float penaltyForWrongButton = 0.75f;
    public float rewardForRightButton = 1.1f;
    public float rewardForGoodReview = 1.05f;

    public float timeBetweenOrders = 8f;
    public float orderRandomness = 2f;
    public int maxOpenOrders = 4;
    public int maxToppings = 4; // does not include sauce and cheese

    public GameObject UserFeedbackPrefab;
    public float DelayBetweenFeedbackItems = 0.2f;

    float timeUntilNextOrder = 0f;

    public UnityIntEvent OnOrderComplete;
    public UnityIntEvent OnNewOrder;
    public UnityEvent OnCorrectOrder;
    public UnityEvent OnIncorrectOrder;
    public UnityEvent OnLastCall;

    private bool LastCall = false;

    GameManager gameManager;

    List<PizzaOrder> allOrders;

    int pizzaOrderCounter = 0;

    private void Update()
    {
        timeUntilNextOrder -= Time.deltaTime;

        if (timeUntilNextOrder < 0 && gameManager.TakingNewOrders() && NumOpenOrders() < maxOpenOrders)
        {
            CreateOrder();
        }

        // Are we switching from taking orders to not?
        if (gameManager.TakingNewOrders() == false && LastCall == false)
        {
            LastCall = true;
            if (OnLastCall != null) OnLastCall.Invoke();
        }

        // Add time to each open order in parallel
        var t = Time.deltaTime; // Time.deltaTime can only be called from the main thread
        allOrders.Where(a => a.isOrderComplete == false).AsParallel().ForAll(a => a.AddWaitTime(t));
    }

    public PizzaIngredient.PizzaInredientType RandomTopping()
    {
        PizzaIngredient.PizzaInredientType topping = AnyRandomTopping();

        // No special meat early in the game
        if (gameManager.GetGameState().NumPurchases() == 0) // No purchases = no meat topping
        {
            while (topping == PizzaIngredient.PizzaInredientType.Cheese || topping == PizzaIngredient.PizzaInredientType.Sauce || topping == PizzaIngredient.PizzaInredientType.SpecialMeat)
            {
                topping = AnyRandomTopping();
            }
        }
        else
        {
            while (topping == PizzaIngredient.PizzaInredientType.Cheese || topping == PizzaIngredient.PizzaInredientType.Sauce)
            {
                topping = AnyRandomTopping();
            }
        }

        return topping;
    }

    private static PizzaIngredient.PizzaInredientType AnyRandomTopping()
    {
        var values = System.Enum.GetValues(typeof(PizzaIngredient.PizzaInredientType));
        return (PizzaIngredient.PizzaInredientType)values.GetValue(Random.Range(0, values.Length));
    }

    public List<PizzaIngredient.PizzaInredientType> AllNeededIngredients()
    {
        gameManager = GetGameManager();
        if (gameManager.GetGameState().CurrentStage == GameManager.GameStage.end) return new List<PizzaIngredient.PizzaInredientType>() { PizzaIngredient.PizzaInredientType.SpecialMeat };

        var result = new List<PizzaIngredient.PizzaInredientType>();

        var openOrders = allOrders.Where(a => a.isOrderComplete == false);
        foreach (var order in openOrders)
        {
            result.AddRange(order.ingredients);
        }

        return result;
    }

    public void CreateOrder()
    {
        timeUntilNextOrder = timeBetweenOrders + Random.Range(-orderRandomness, orderRandomness);

        var latestOrder = new PizzaOrder() { orderId = pizzaOrderCounter, ingredients = new List<PizzaIngredient.PizzaInredientType>() { PizzaIngredient.PizzaInredientType.Cheese, PizzaIngredient.PizzaInredientType.Sauce } };
        var numToppings = Random.Range(0, gameManager.MaxToppingsThisStage() + 1); // max is exclusive
        for (var i = 0; i < numToppings; i++)
        {
            latestOrder.ingredients.Add(RandomTopping());
        }
        allOrders.Add(latestOrder);
        pizzaOrderCounter++;

        if (OnNewOrder != null) OnNewOrder.Invoke(latestOrder.orderId);
    }

    public void OrderComplete(Pizza p)
    {
        var ingredients = p.GetToppings();
        var oo = FindOpenOrder(ingredients);

        if (oo != null)
        {
            oo.isOrderComplete = true;
            if (OnOrderComplete != null) OnOrderComplete.Invoke(oo.orderId);
        }
        else
        {
            Debug.LogError("Couldn't find that open order!");
        }
    }

    public int NumOpenOrders()
    {
        return allOrders.Where(a => a.isOrderComplete == false).Count();
    }

    public PizzaOrder FindOpenOrder(List<PizzaIngredient.PizzaInredientType> ingredients)
    {
        return allOrders.Where(a => a.Match(ingredients) && a.isOrderComplete == false).FirstOrDefault();
    }

    public PizzaOrder FindOrderById(int id)
    {
        return allOrders.Where(a => a.orderId == id).FirstOrDefault();
    }

    public void PizzaReviewed(int orderId, bool wasGoodReview, ReviewGrade grade)
    {
        var feedbackItems = new Dictionary<string, Color>();
        var money = basePizzaFee;

        switch (grade)
        {
            case ReviewGrade.correct:
                money *= rewardForRightButton;
                feedbackItems.Add("Correct Reponse", Color.green);
                if (OnCorrectOrder != null) OnCorrectOrder.Invoke();
                break;

            case ReviewGrade.incorrect:
                money *= penaltyForWrongButton;
                feedbackItems.Add("Incorrect Reponse", Color.yellow);
                if (OnIncorrectOrder != null) OnIncorrectOrder.Invoke();
                break;

            case ReviewGrade.missed:
                money *= penaltyForMissedReview;
                feedbackItems.Add("Missed Response!", Color.red);
                break;
        }

        // A small bonus for a good review
        if (wasGoodReview == true)
        {
            money *= rewardForGoodReview;
        }

        var o = FindOrderById(orderId);
        if (o != null)
        {
            var numToppings = o.ingredients.Count() - 2; // sauce and cheese by default
            var toppingsFee = numToppings * perToppingFee;
            if (numToppings > 0) feedbackItems.Add("Toppings! $" + toppingsFee, Color.cyan);
            money += toppingsFee;

            o.moneyEarned = (int)money;
            gameManager.AddMoney(o.moneyEarned);
            StartCoroutine(ProvideFeedbackCo(feedbackItems));
        }

    }

    IEnumerator ProvideFeedbackCo(Dictionary<string, Color> FeedbackItems)
    {
        foreach (var kvp in FeedbackItems)
        {
            var thing = Instantiate(UserFeedbackPrefab);
            var ufs = thing.GetComponent<UserFeedback>();
            if (ufs != null) {
                ufs.SetText(kvp.Key, kvp.Value);
            }
            yield return new WaitForSeconds(DelayBetweenFeedbackItems);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GetGameManager();

        allOrders = new List<PizzaOrder>();
    }

    GameManager GetGameManager()
    {
        if (gameManager != null) return gameManager;

        gameManager = FindObjectOfType<GameManager>();
        return gameManager;
    }

    public void TrashedPizza()
    {
        gameManager.AddMoney(PizzaTrashPenalty);
    }
}
