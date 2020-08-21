using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerScript : MonoBehaviour
{
    private ParticleSystem myParticles;

    public PlacementSocket PizzaSocket;
    public PlacementSocket IngredientSocket;
    public bool IsUpgraded;
    public float ConsumeTime = 1.5f;
    public float UpgradedConsumeTime = 1.1f;
    public Sprite UpgradedSprite;

    bool isCurrentlyWorking = false;
    SpriteRenderer ren;

    // Start is called before the first frame update
    void Start()
    {
        myParticles = GetComponentInChildren<ParticleSystem>();
        ren = GetComponent<SpriteRenderer>();
        if (ren == null) Debug.LogError("Worker has no sprite renderer?");
        if (IsUpgraded) Upgrade();
    }

    public void Upgrade()
    {
        IsUpgraded = true;
        ren = GetComponent<SpriteRenderer>();
        ren.sprite = UpgradedSprite;
    }

    public void Activate()
    {
        if (PizzaSocket.OccupiedBy != null && IngredientSocket.OccupiedBy != null && isCurrentlyWorking == false)
        {
            var ingredient = IngredientSocket.OccupiedBy.GetComponentInChildren<PizzaIngredient>();

            var t = ConsumeTime;
            if (IsUpgraded) t = UpgradedConsumeTime;

            if (myParticles)
            {
                if (myParticles.isPlaying == true) myParticles.Stop();
                var m = myParticles.main;
                m.duration = t + 0.25f;
                myParticles.Play();
            }

            IngredientSocket.Consume(t);
            StartCoroutine(ProgressPizzaCo(t + 0.25f, ingredient.Ingredient));
        }

    }

    private IEnumerator ProgressPizzaCo(float wait, PizzaIngredient.PizzaInredientType ingredient)
    {
        var ps = PizzaSocket.OccupiedBy.GetComponentInChildren<Pizza>();
        if (ps == null) Debug.LogError("Pizza script missing?");
        var i = ps.GetComponent<Interactable>();
        isCurrentlyWorking = true;
        i.isLocked = true;

        yield return new WaitForSeconds(wait);

        ps.AddIngredient(ingredient);
        i.isLocked = false;
        isCurrentlyWorking = false;
        Activate(); // Try to activate again, in case an ingredient was placed while working
    }
}
