using System.Collections.Generic;
using UnityEngine;

public class Pizza : MonoBehaviour
{
    public ParticleSystem Cookedparticles;
    public ParticleSystem BurntParticles;
    public ParticleSystem ToxicParticles;

    private SpriteRenderer spriteRenderer;
    private List<PizzaIngredient.PizzaInredientType> AddedIngredients = new List<PizzaIngredient.PizzaInredientType>();

    public enum PizzaStage { dough, sauced, toppinged, baked, burnt }
    public enum PizzaType { dough, sauced, cheese, veggie, meat, combo, wrong }

    float cookTime = 0f;
    float neededCookTime = 10f;
    float burnCookTime = 14f;

    public PizzaStage stage = PizzaStage.dough;

    GameManager gameManager;

    public int GetNumToppings()
    {
        return AddedIngredients.Count - 2;
    }

    public List<PizzaIngredient.PizzaInredientType> GetToppings()
    {
        return AddedIngredients;
    }

    public static PizzaType getPizzaType(List<PizzaIngredient.PizzaInredientType> ingredients)
    {
        bool hasCheese = false;
        bool hasVeggies = false;
        bool hasMeat = false;
        bool hasSauce = false;

        if (ingredients.Contains(PizzaIngredient.PizzaInredientType.Broccoli)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.ChiliPepper)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Mushroom)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Onion)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Pepper)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Tomato))
        {
            hasVeggies = true;
        }

        if (ingredients.Contains(PizzaIngredient.PizzaInredientType.Sauce))
        {
            hasSauce = true;
        }

        if (ingredients.Contains(PizzaIngredient.PizzaInredientType.Cheese))
        {
            hasCheese = true;
        }

        if (ingredients.Contains(PizzaIngredient.PizzaInredientType.Pepperoni)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Sardine)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.Shrimp)
            || ingredients.Contains(PizzaIngredient.PizzaInredientType.SpecialMeat)
            )
        {
            hasMeat = true;
        }

        if (ingredients.Count == 0) return PizzaType.dough;
        else
        {
            if (hasSauce == true)
            {
                if (hasVeggies && hasMeat) return PizzaType.combo;
                if (hasVeggies) return PizzaType.veggie;
                if (hasMeat) return PizzaType.meat;
                if (hasCheese) return PizzaType.cheese;
                return PizzaType.sauced;
            }
            else
            {
                return PizzaType.wrong;
            }
        }
    }

    public void AddCookTime(float t)
    {
        cookTime += t;
        if (cookTime > neededCookTime && cookTime < burnCookTime) SetBaked();
        if (cookTime > burnCookTime) SetBurnt();
    }

    void SetToppinged()
    {
        stage = PizzaStage.toppinged;
        SetToppingSprite();
    }

    void SetBaked()
    {
        stage = PizzaStage.baked;
        SetBakedSprite();

        var cp = Cookedparticles.emission;
        cp.enabled = true;
    }

    void SetBurnt()
    {
        stage = PizzaStage.burnt;
        spriteRenderer.sprite = gameManager.pizzaSpriteManager.BurnedSprite;

        // turn off cooked particles
        var cp = Cookedparticles.emission;
        cp.enabled = false;

        // turn off toxic particles
        var tp = ToxicParticles.emission;
        tp.enabled = false;

        // enable burnt particles
        var bp = BurntParticles.emission;
        bp.enabled = true;
    }

    public void AddIngredient(PizzaIngredient.PizzaInredientType newIngredient)
    {
        if (getPizzaType(AddedIngredients) != PizzaType.wrong)
        {
            AddedIngredients.Add(newIngredient);
            SetToppinged();
        }
    }

    private void SetToppingSprite()
    {
        spriteRenderer.sprite = gameManager.pizzaSpriteManager.GetPizzaSprite(AddedIngredients, false);
        if (getPizzaType(AddedIngredients) == PizzaType.wrong)
        {
            var e = ToxicParticles.emission;
            e.enabled = true;
        }
    }

    private void SetBakedSprite() {
        spriteRenderer.sprite = gameManager.pizzaSpriteManager.GetPizzaSprite(AddedIngredients, true);
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // turn off cooked particles
        var cp = Cookedparticles.emission;
        cp.enabled = false;

        // turn off toxic particles
        var tp = ToxicParticles.emission;
        tp.enabled = false;

        // enable burnt particles
        var bp = BurntParticles.emission;
        bp.enabled = false;
    }
}

