using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Explosion : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(MainLoop());
    }


    private IEnumerator MainLoop()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
