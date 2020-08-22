using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FadeToBlack : MonoBehaviour
{

    public float duration = 1f;
    public Color startColor;
    public Color endColor;
    public UnityEvent OnFadeComplete;

    float elapsed;
    SpriteRenderer ren;
    bool isFadeComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        ren = GetComponent<SpriteRenderer>();
        if (ren == null) Debug.LogError("No sprite renderer?");
    }

    // Update is called once per frame
    void Update()
    {
        elapsed += Time.deltaTime;
        ren.color = Color.Lerp(startColor, endColor, elapsed / duration);
        if (isFadeComplete == false && elapsed >= duration)
        {
            isFadeComplete = true;
            if (OnFadeComplete != null) OnFadeComplete.Invoke();
        }
    }
}
