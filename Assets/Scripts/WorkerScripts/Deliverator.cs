using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deliverator : MonoBehaviour
{
    public float targetSpeed = 0.05f;
    public List<PlacementSocket> sourceSockets;
    public PlacementSocket destinationSocket;
    public float homeY = 3.25f;
    public float deliveryY = 0.75f;
    public Animator anim;
    public Holdable currentHolding;
    public Transform holdLocation;
    public bool IsUpgraded;
    public float UpgradeSpeedMultiplier = 1.2f;
    public Texture2D UpgradedSpriteSheetTexture;

    private SpriteSwapper swapper;
    private float speed = 0;
    private Vector2 direction = Vector2.zero;

    private enum MovementState { entering, waiting, deliverating, idle }

    MovementState currentState = MovementState.entering;
    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) Debug.LogError("Game manager missing? What??");
        anim = GetComponent<Animator>();
        if (destinationSocket == null) Debug.LogError("Destination socket needs to be populated");
        swapper = GetComponent<SpriteSwapper>();
        if (swapper == null) Debug.LogError("Sprite swapper missing?");

        if (IsUpgraded) Upgrade();
    }

    public void Upgrade()
    {
        swapper = GetComponent<SpriteSwapper>();
        if (swapper == null) Debug.LogError("Sprite swapper missing?");

        IsUpgraded = true;
        swapper.spriteSheetTexture = UpgradedSpriteSheetTexture;
        swapper.ReloadSpritesheet();
    }

    void Update()
    {
        var isMoving = false;
        direction.y = 0;

        var speedMultiplier = 1f;
        if (IsUpgraded) speedMultiplier = UpgradeSpeedMultiplier;

        switch (currentState)
        {
            case MovementState.entering:
                anim.SetInteger("Direction", 2);
                direction.y += 1;
                isMoving = true;
                if (transform.position.y > homeY) currentState = MovementState.waiting;
                break;

            case MovementState.deliverating:
                anim.SetInteger("Direction", 0);
                direction.y -= 1;
                isMoving = true;
                if (transform.position.y < deliveryY) currentState = MovementState.idle;
                break;

            case MovementState.idle:
                // interactable that is holdable is released
                // Should be able to always do this
                currentHolding.Drop();
                currentHolding = null;
                currentState = MovementState.entering;
                break;

            case MovementState.waiting:
                var deliverySocket = GetCookedPizzaWithOrder();
                if (deliverySocket != null)
                {
                    var holdable = deliverySocket.OccupiedBy.GetComponent<Holdable>();
                    if (holdable != null)
                    {
                        currentHolding = holdable;
                        currentHolding.Pickup();
                    }
                    else
                    {
                        Debug.Log("Pizza isn't holdable?");
                    }
                    currentState = MovementState.deliverating;
                }
                break;
        }

        if (isMoving)
        {
            speed = targetSpeed;
        }
        else
        {
            anim.SetInteger("Direction", -1); // Goin' nowhere
            speed = 0;
        }

        anim.SetFloat("Speed", speed);
        this.transform.position += new Vector3(0, direction.y * speed * speedMultiplier, 0);

        if (currentHolding != null)
        {
            var holdPosition = holdLocation.position;
            holdPosition.x += direction.x * 0.2f;
            holdPosition.y += direction.y * 0.2f;
            currentHolding.transform.position = holdPosition;
        }
    }

    PlacementSocket GetCookedPizzaWithOrder()
    {
        foreach (var ss in sourceSockets)
        {
            var isCookedPizza = false;
            var isOrderForThisPizza = false;

            if (ss.OccupiedBy != null)
            {
                var pizza = ss.OccupiedBy.GetComponent<Pizza>();
                if (pizza == null)
                {
                    Debug.LogError("Something's wrong, no pizza script?");
                }

                if (pizza.stage == Pizza.PizzaStage.baked) isCookedPizza = true;
                var order = gameManager.orderManager.FindOpenOrder(pizza.GetToppings());
                if (order != null) isOrderForThisPizza = true;
            }

            if (isCookedPizza && isOrderForThisPizza)
            {
                return ss;
            }
        }

        return null;
    }
}
