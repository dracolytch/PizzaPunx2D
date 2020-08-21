using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastCallMovement : MonoBehaviour
{
    public Vector3 Speed;
    public float totalTime = 5f;
    float elapsed;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(MoveUpCo());
    }


    IEnumerator MoveUpCo()
    {
        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            transform.Translate(Speed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
