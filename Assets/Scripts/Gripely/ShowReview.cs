using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GameManager;

public class ShowReview : MonoBehaviour
{
    public enum ReviewStatus { revealing, showing, closing, closed };
    public enum ReviewGrade { correct, incorrect, missed };

    public UnityEvent OnThank;
    public UnityEvent OnPlacate;
    public UnityEvent OnShowReview;

    public string InitialText;

    public Text textToAnimate;
    public float animationDuration = 1f;
    public float closedHeight = 0.5f;
    public float openHeight = 116f;
    public float closedPosition = 2.9f;
    public float openPosition = 0f;

    ReviewStatus currentStatus;
    ReviewData currentReview;
    ReviewData nextReview;
    GameManager gameManager;

    Queue<int> reviewsToComplete;

    GameStage currentGameStage = GameStage.early;
    Dictionary<GameStage, List<Review>> pizzaReviews;

    public void Thank()
    {
        switch (currentStatus)
        {
            // One is still coming in, do nothing
            case ReviewStatus.closed:
            case ReviewStatus.closing:
            case ReviewStatus.revealing:
                break;

            // Thank
            case ReviewStatus.showing:
                CloseReview(currentReview.isGoodReview, true);
                if (OnThank != null) OnThank.Invoke();
                break;
        }
    }

    Review GetRandomReview()
    {
        var r = pizzaReviews[currentGameStage];
        return r[Random.Range(0, r.Count)];
    }

    public void Placate()
    {
        switch (currentStatus)
        {
            // One is still coming in, do nothing
            case ReviewStatus.closed:
            case ReviewStatus.closing:
            case ReviewStatus.revealing:
                break;

            // Placate
            case ReviewStatus.showing:
                CloseReview(currentReview.isGoodReview, false);
                if (OnPlacate != null) OnPlacate.Invoke();
                break;
        }
    }

    private void Awake()
    {
        currentStatus = ReviewStatus.closed;
    }

    private void Start()
    {
        reviewsToComplete = new Queue<int>();
        reviewsToComplete.Enqueue(int.MinValue);

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) Debug.LogError("No game manager found!");

        DisplayReview(-1, InitialText, true);
        var textFile = Resources.Load<TextAsset>("reviews");
        var reviews = JsonConvert.DeserializeObject<Dictionary<int, Review>>(textFile.text);
        var keepers = reviews.Values.Where(a => a.doNotUse == 0 && a.needsChecking == 0);
        pizzaReviews = new Dictionary<GameStage, List<Review>>();

        pizzaReviews.Add(GameStage.early, keepers.Where(a => a.gameStage == 0).ToList());
        pizzaReviews.Add(GameStage.mid, keepers.Where(a => a.gameStage == 1).ToList());
        pizzaReviews.Add(GameStage.late, keepers.Where(a => a.gameStage == 2).ToList());
    }

    public void DisplayNewReview(int orderId)
    {
        reviewsToComplete.Enqueue(orderId);
        DisplayReview(GetRandomReview());
    }

    public void DisplayReview(Review r)
    {
        DisplayReview(0, r.content, r.isGood == 1);
    }

    void DisplayReview(int id, string text, bool isGood)
    {
        switch (currentStatus)
        {
            // Display the review
            case ReviewStatus.closed:
            case ReviewStatus.closing:
                nextReview = new ReviewData() { isGoodReview = isGood, reviewId = id, reviewText = text };
                break;

            // One is still coming in, do nothing
            case ReviewStatus.revealing:
                break;

            // Player missed it. Close up
            case ReviewStatus.showing:
                CloseReview(currentReview.isGoodReview, null);
                nextReview = new ReviewData() { isGoodReview = isGood, reviewId = id, reviewText = text };
                break;
        }
    }

    private void AnimateOpening()
    {
        if (currentStatus == ReviewStatus.closed)
        {
            currentReview = nextReview;
            nextReview = null;
            StartCoroutine(AnimateOpeningCo(currentReview.reviewText));
        }
    }

    private void AnimateClosing()
    {
        if (currentStatus == ReviewStatus.showing)
        {
            StartCoroutine(AnimateClosingCo());
        }
    }

    IEnumerator AnimateOpeningCo(string content)
    {
        if (OnShowReview != null) OnShowReview.Invoke();
        currentStatus = ReviewStatus.revealing;
        var elapsed = 0f;
        var rt = textToAnimate.GetComponent<RectTransform>();
        textToAnimate.text = content;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Lerp(closedHeight, openHeight, elapsed / animationDuration));
            rt.position = new Vector3(rt.position.x, Mathf.Lerp(closedPosition, openPosition, elapsed / animationDuration));
            
            yield return null;
        }
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, openHeight);

        currentStatus = ReviewStatus.showing;
    }

    IEnumerator AnimateClosingCo()
    {
        currentStatus = ReviewStatus.closing;

        var elapsed = 0f;
        var rt = textToAnimate.GetComponent<RectTransform>();

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Lerp(openHeight, closedHeight, elapsed / animationDuration));
            rt.position = new Vector3(rt.position.x, Mathf.Lerp(openPosition, closedPosition, elapsed / animationDuration));

            yield return null;
        }
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, closedHeight);

        currentStatus = ReviewStatus.closed;
    }

    public ReviewGrade CloseReview(bool isGoodReview, bool? grade)
    {
        AnimateClosing();

        var result = ReviewGrade.missed;

        if (grade.HasValue)
        {
            if (isGoodReview == grade.Value) result = ReviewGrade.correct;
            else result = ReviewGrade.incorrect;
        }

        var orderId = reviewsToComplete.Dequeue();

        gameManager.orderManager.PizzaReviewed(orderId, isGoodReview, result);

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentStatus == ReviewStatus.closed && nextReview != null )
        {
            AnimateOpening();
        }

        if (Input.GetKey(KeyCode.Z))
        {
            Thank();
        }
        else
        {
            if (Input.GetKey(KeyCode.X))
            {
                Placate();
            }
        }
    }
}
