using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingLineOfBlocks : MonoBehaviour
{
    [SerializeField] private float vanishTimer = .5f;
    [SerializeField] private GameObject miniBlockPrefab;
    
    private List<GameObject> miniBlocks;
    private bool vanishing;

    void Start()
    {
        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        int size = (int) boxCollider2D.size.x;

        miniBlocks = new List<GameObject>();
        for(int i = 0; i < size * 2; i++)
        {
            Vector2 position = transform.position;
            position += Vector2.left * ((size / 2) + .25f);
            position += Vector2.right * size * ((float)i / (size * 2));

            GameObject block = Instantiate(miniBlockPrefab, position, Quaternion.identity, transform);
            miniBlocks.Add(block);
        }
        miniBlockPrefab.SetActive(false);
    }

    public void VanishLines()
    {
        if (vanishing) return;
        vanishing = true;
        StartCoroutine(VanishLinesAfter());
    } 

    private IEnumerator VanishLinesAfter()
    {
        yield return new WaitForSeconds(vanishTimer);

        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        boxCollider2D.enabled = false;

        foreach(GameObject block in miniBlocks)
        {
            block.GetComponent<Animator>().SetTrigger("Vanish");
        }

        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }
}
