using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pooler : MonoBehaviour
{

    [SerializeField] private GameObject pooledObject;

    public int startAmount = 10;
    public List<GameObject> pool;
    private GameObject parentObject;

    public GameObject PooledObject
    {
        get
        {
            return pooledObject;
        }
        set
        {
            pooledObject = value;
            pool = new List<GameObject>();
            if (parentObject != null) Destroy(parentObject);
            parentObject = new GameObject(pooledObject.name + " Pool");
            parentObject.transform.parent = this.transform;
            parentObject.transform.localPosition = Vector3.zero;

            GameObject newObj;
            for (int i = 0; i < startAmount; i++)
            {
                newObj = Instantiate(PooledObject, parentObject.transform);
                newObj.SetActive(false);
                pool.Add(newObj);
            }

        }
    }

    private void Awake()
    {
        if(pooledObject != null)
        {
            PooledObject = pooledObject;
        }
    }
    public GameObject Get(bool getActivated = true)
    {
        if (pool == null) pool = new List<GameObject>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                pool[i].SetActive(getActivated);
                return pool[i];
            }
        }

        if (parentObject == null)
        {
            parentObject = new GameObject(PooledObject.name + " Pool");
            parentObject.transform.parent = this.transform;
            parentObject.transform.localPosition = Vector3.zero;
        }

        GameObject newObj = Instantiate(PooledObject, parentObject.transform);
        newObj.SetActive(getActivated);
        pool.Add(newObj);
        return newObj;
    }

}