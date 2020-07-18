using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    //public variables
    public int foodCapacity;
    public GameObject foodPrefab;
    public GameObject plane;

    //private variables
    private int foodCount;
    //private Random random;

    void Start()
    {
        ScatterFood();
    }

    public void ScatterFood()
    {
        float sizeX = plane.transform.localScale.x-0.5f;
        float sizeZ = plane.transform.localScale.z-0.5f;

        for (int i = 0; i < foodCapacity - 1; i++)
        {
            float posX = Random.Range(0, sizeX)*10;
            float posZ = Random.Range(0, sizeZ)*10;

            Instantiate(foodPrefab, new Vector3(posX-22, 0, posZ-22), Quaternion.identity);
        }
    }

    void Update()
    {

    }
}
