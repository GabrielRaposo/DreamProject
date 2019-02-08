using System.Collections;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int quantity;

    private GameObject[] pool;
    private int index;

    public void Init(ID id)
    {
        pool = new GameObject[quantity];
        for(int i = 0; i < quantity; i++)
        {
            pool[i] = Instantiate(bulletPrefab, transform.position, Quaternion.identity, transform);
            pool[i].GetComponent<Hitbox>().id = id;
            pool[i].SetActive(false);
        }
    }

    public GameObject Get()
    {
        GameObject bullet = pool[index];
        index = (index + 1) % quantity;

        return bullet;
    }

    public void Return(GameObject bullet)
    {
        bullet.SetActive(false);
        bullet.transform.position = transform.position;
    }
}
