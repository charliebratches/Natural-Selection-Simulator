using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static NSEnums;

public class Individual : MonoBehaviour
{
    public int id;
    public float size;
    public float speed;
    public float sense;
    public bool AteToday;
    public int TimesEatenToday;
    public bool BornToday;
    public float energy;
    private bool HasBeenEatenToday;
    public bool male;
    public Diet diet;
    float totalEnergyCost;
    private bool scanning;

    private int frames = 0;


    public void NewIndividual(int memberId, float startSize, float startSpeed, float startSense, Diet startingDiet)
    {
        id = memberId;
        size = startSize;
        speed = startSpeed;
        sense = startSense;
        AteToday = false;
        TimesEatenToday = 0;
        BornToday = false;
        HasBeenEatenToday = false;
        male = UnityEngine.Random.Range(0, 2) != 0;
        diet = startingDiet;
        energy = 10000;
        scanning = false;

        //set wander speed and navMesh speed
        gameObject.GetComponentInChildren<Wander>().speed = speed * 50;
        gameObject.GetComponentInChildren<NavMeshAgent>().speed = speed * 50;

        //calc energy cost

        double speedCost = (speed*5);
        double sizeCost = (size*5);
        double senseCost = (sense*5);
        totalEnergyCost = (float)(Math.Pow(sizeCost, 2) * Math.Pow(speedCost, 2) + senseCost);


        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        //var currentColor = renderer.material.color;
        renderer.material.color = new Color(size, speed, sense);

        gameObject.transform.localScale = new Vector3(size*2, size*2, size*2);
    }

    public void EatEnvironmentalFood()
    {
        AteToday = true;
        TimesEatenToday++;
        StartWandering();
        scanning = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(energy > 0)
        {
            InvokeRepeating("DrainEnergyTimeStep", 1, 1.0f);
        }
    }

    void DrainEnergyTimeStep()
    {
        energy -= totalEnergyCost;
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        if (energy > 0)
        {
            if (gameObject.GetComponentInChildren<Wander>().enabled && !scanning)
            {
                ScanForFood();
            }
        }
        else
        {
            StopWandering();
            GetComponent<NavMeshAgent>().enabled = false;
        }
    }

    void ScanForFood()
    {
        Collider[] farHitColliders = Physics.OverlapSphere(gameObject.transform.position, (float)sense*10);
        Collider[] nearHitColliders = Physics.OverlapSphere(gameObject.transform.position, 5f);
        scanning = true;
        int i = 0;
        while (i < farHitColliders.Length)
        {
            if (farHitColliders[i].gameObject.tag == "Food" && gameObject.GetComponentInChildren<Wander>().enabled)
            {
                //walk towards food
                StopWandering();
                WalkTowardsGoal(farHitColliders[i].gameObject.transform);
                //StartWandering();
                break;
            }
            else
            {
                i++;
            }
        }
    }

    void StopWandering()
    {
        gameObject.GetComponentInChildren<Wander>().enabled = false;
    }

    public void StartWandering()
    {
        gameObject.GetComponentInChildren<Wander>().enabled = true;
    }

    void WalkTowardsGoal(Transform goalTransform)
    {
        NavMeshAgent agent = gameObject.GetComponentInChildren<NavMeshAgent>();
        agent.destination = goalTransform.position;
    }
}
