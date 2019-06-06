using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageTrailEffect : MonoBehaviour
{
    [SerializeField] private GameObject afterImagePrefab;

    [Header("Values")]
    [SerializeField] private float spawnDelay;

    private FadeOutEffect[] afterImages;
    private int count;

    void Start()
    {
        afterImages = new FadeOutEffect[10];
        for(int i = 0; i < afterImages.Length; i++)
        {
            GameObject spawn = Instantiate(afterImagePrefab, transform.position, Quaternion.identity);
            afterImages[i] = spawn.GetComponent<FadeOutEffect>();
            afterImages[i].enabled = false;
        }

        afterImagePrefab.SetActive(false);
    }

    public IEnumerator Call(bool facingRight)
    {
        while(true)
        {
            afterImages[count].transform.position = transform.position;
            afterImages[count].enabled = true;
            afterImages[count].CallFadeOut(facingRight);

            count = (count + 1) % afterImages.Length;

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
