using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Endgame : MonoBehaviour
{
    public GameObject Player;
    public List<AudioClip> Hits;
    public Camera CameraToShake;
    public int NumHits = 3;
    public float TimeBetweenHits = 0.2f;
    public float TimeUntilFadeOut = 25f;

    public UnityEvent OnEndGame;
    public UnityEvent OnMurderComplete;
    public UnityEvent OnEpilogComplete;

    AudioSource sfx;
    Vector3 cameraHome;

    private void Awake()
    {
        sfx = GetComponent<AudioSource>();
    }

    private void Start()
    {
        cameraHome = CameraToShake.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnEndGame != null) OnEndGame.Invoke();
    }

    public void StartEnd()
    {
        StartCoroutine(StartEndCo());
    }

    public IEnumerator StartEndCo()
    {
        Destroy(Player);

        yield return new WaitForSeconds(0.5f);

        for (var i = 0; i < NumHits; i++)
        {
            sfx.PlayOneShot(Hits[Random.Range(0, Hits.Count)]);
            CameraToShake.transform.Translate(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
            yield return StartCoroutine(ReturnToCenterCo(CameraToShake.gameObject, TimeBetweenHits - 0.05f, cameraHome));
        }

        yield return StartCoroutine(ReturnToCenterCo(CameraToShake.gameObject, 0.05f, cameraHome));
        if (OnMurderComplete != null) OnMurderComplete.Invoke();

        yield return StartCoroutine(WaitforEpilogCo(TimeUntilFadeOut));
    }

    public IEnumerator ReturnToCenterCo(GameObject target, float duration, Vector3 position)
    {
        var elapsed = 0f;
        var startPosition = target.transform.position;
        var endPosition = position;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            yield return null;
        }
    }

    public IEnumerator WaitforEpilogCo(float wait)
    {
        yield return new WaitForSeconds(wait);
        if (OnEpilogComplete != null) OnEpilogComplete.Invoke();
    }

    public void LoadTitleScene()
    {
        SceneManager.LoadScene(0);
    }
}
