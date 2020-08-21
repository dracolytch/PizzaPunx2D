using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PizzaIngredient;

[System.Serializable]
public class IngredientTouple
{
    public PizzaInredientType key;
    public GameObject value;
}

public class IngredientSpawner : MonoBehaviour
{
    public float CheckFrequency = 1.0f;
    public List<IngredientTouple> prefabDict;


    GameManager gameManager;
    PlacementSocket relatedSocket;
    float elapsed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        relatedSocket = GetComponent<PlacementSocket>();
        if (relatedSocket == null)
        {
            Debug.LogError("Placement soced needed on the ingredient spawner");
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;

        if (elapsed >= CheckFrequency)
        {
            elapsed = 0f;
            SpawnIngredient();
        }
    }

    public void SpawnIngredient()
    {
        if (relatedSocket.OccupiedBy == null)
        {
            var thingToSpawn = PickThingToSpawn();
            if (thingToSpawn.HasValue)
            {
                var entries = prefabDict.Where(a => a.key == thingToSpawn).Count();

                if (entries != 0)
                {
                    var food = Instantiate(prefabDict.Where(a => a.key == thingToSpawn).First().value);
                    food.transform.position = transform.position;
                }
                else
                {
                    Debug.LogError($"Tried to spawn {thingToSpawn.ToString()} but it wasn't in the dictionary");
                }
            }
        }
    }

    public PizzaInredientType? PickThingToSpawn()
    {
        if (gameManager.currentGameStage == GameManager.GameStage.end) return PizzaInredientType.SpecialMeat;

        var allThings = FindObjectsOfType<PizzaIngredient>();
        var allNeededIngredients = gameManager.orderManager.AllNeededIngredients();

        foreach (var thing in allThings)
        {
            if (allNeededIngredients.Contains(thing.Ingredient))
            {
                allNeededIngredients.Remove(thing.Ingredient);
            }
        }

        if (allNeededIngredients.Count > 0)
        {
            return allNeededIngredients[Random.Range(0, allNeededIngredients.Count)];
        }
        return null;
    }
}
