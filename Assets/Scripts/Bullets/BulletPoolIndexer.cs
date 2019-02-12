using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolIndexer : MonoBehaviour
{
    [System.Serializable]
    private struct Pool
    {
        public string name;
        public BulletPool parent;
    }
    [SerializeField] private Pool[] pools;

    public static BulletPoolIndexer instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach(Pool pool in pools)
        {
            pool.parent.Init(ID.Enemy);
        }
    }

    public BulletPool GetPool(string name)
    {
        BulletPool pool = new BulletPool();
        foreach(Pool p in pools)
        {
            if(p.name == name)
            {
                pool = p.parent;
                break;
            }
        }
        return pool;
    } 
}
