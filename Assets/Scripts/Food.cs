using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Detect collisions during wander.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Individual")
        {
            other.gameObject.GetComponent<Individual>().EatEnvironmentalFood();
            Remove();
        }
    }
}
