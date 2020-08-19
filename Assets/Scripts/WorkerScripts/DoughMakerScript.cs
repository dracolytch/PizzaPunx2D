using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoughMakerScript : MonoBehaviour
{
    private ParticleSystem myParticles;

    public PlacementSocket PizzaSocket;
    public GameObject PizzaPrefab;
    public Sprite UpgradedSprite;
    public float MakeDoughTime = 2.5f;
    public float MakeDoughUpgradedTime = 1.75f;

    public bool IsUpgraded;
    SpriteRenderer ren;

    // Start is called before the first frame update
    void Start()
    {
        myParticles = GetComponentInChildren<ParticleSystem>();
        ren = GetComponent<SpriteRenderer>();

        if (myParticles == null || ren == null)
        {
            Debug.LogError("I'm missing something important");
        }

        if (IsUpgraded) Upgrade();
    }

    public void Upgrade()
    {
        ren.sprite = UpgradedSprite;
        IsUpgraded = true;
    }

    public void Activate()
    {
        if (PizzaSocket.OccupiedBy == null)
        {
            if (myParticles) myParticles.Play();
            var time = MakeDoughTime;
            if (IsUpgraded)
            {
                time = MakeDoughUpgradedTime;
            }
            StartCoroutine(MakeDoughCo(time));
        }
    }

    private IEnumerator MakeDoughCo(float delay)
    {
        yield return new WaitForSeconds(delay);

        var pizza = Instantiate(PizzaPrefab);
        pizza.transform.position = transform.position;
        var ps = pizza.GetComponent<Holdable>();
        if (ps) ps.SetPreferredSocket(PizzaSocket);
        // Pizza auto-drops on creation
    }
}
