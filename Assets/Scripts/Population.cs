using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static NSEnums;

public class Population : MonoBehaviour
{
    //public variables
    public List<GameObject> Members;
    public int StarvedCount;
    public int globalMemberCounter;

    public int popStartSize;
    public GameObject individualPrefab;
    public GameObject plane;


    public TextMeshProUGUI populationText;
    private float mutationAmount = 0.1f; //how much to mutate each trait by
    private float speciationAmount = 0.2f; //how similar each trait must be for two members to be able to mate

    public float populationStartingSize = 0.1f;
    public float populationStartingSpeed = 0.1f;
    public float populationStartingSense = 0.1f;


    public void NewPopulation(int popStartSize, float size, float speed, float sense, Diet diet)
    {
        Members = new List<GameObject>();
        
        for (int i = 0; i < popStartSize; i++)
        {
            NewMember(size, speed, sense, diet, new Vector3(0, 0, 0));
        }
        Scatter();

        StarvedCount = 0;
    }

    public void NewMember(float size, float speed, float sense, Diet diet, Vector3 position)
    {
        var member = Instantiate(individualPrefab, position, Quaternion.identity);
        member.GetComponent<Individual>().NewIndividual(globalMemberCounter, size, speed, sense, diet);
        Members.Add(member);
        globalMemberCounter++;
    }

    public void Reproduce()
    {
        if(!Members.Exists(m => m.GetComponent<Individual>().male && m.GetComponent<Individual>().AteToday))
        {
            //return if no viable males exist
            return;
        }

        if (!Members.Exists(m => m.GetComponent<Individual>().male && m.GetComponent<Individual>().AteToday))
        {
            //return if no viable females exist
            return;
        }

        List<GameObject> offspringList = new List<GameObject>();

        IEnumerable<Individual> viableMales = Members.Where(m => m.GetComponent<Individual>().male && m.GetComponent<Individual>().AteToday).Select(m => m.GetComponent<Individual>());

        foreach (GameObject femaleMember in Members.Where(m => !m.GetComponent<Individual>().male))
        {
            Individual femaleIndividual = femaleMember.GetComponent<Individual>();

            viableMales = viableMales.Where(m => (m.size < (femaleIndividual.size+ speciationAmount) && m.size > (femaleIndividual.size - speciationAmount)) 
                && (m.speed < (femaleIndividual.speed + speciationAmount) && m.speed > (femaleIndividual.speed - speciationAmount))
                && (m.sense < (femaleIndividual.sense + speciationAmount) && m.sense > (femaleIndividual.sense - speciationAmount)));

            if(viableMales.Count() <= 0)
            {
                return;
            }

            Individual mostSuccessfulMaleIndividual;
            if (viableMales.Any())
            {
                mostSuccessfulMaleIndividual = viableMales.Aggregate((i1, i2) => i1.TimesEatenToday > i2.TimesEatenToday ? i1 : i2); //return the first male who ate the most today
            }
            else
            {
                return; //no viable males
            }

            if (!femaleIndividual.male && femaleIndividual.AteToday)
            {
                var offspring = Instantiate(individualPrefab, new Vector3(0, 0, 0), Quaternion.identity);

                //now randomly take either the male or the female's traits:
                float size = Random.Range(0, 2) != 0 ? femaleIndividual.size : mostSuccessfulMaleIndividual.size;
                float speed = Random.Range(0, 2) != 0 ? femaleIndividual.speed : mostSuccessfulMaleIndividual.speed;
                float sense = Random.Range(0, 2) != 0 ? femaleIndividual.sense : mostSuccessfulMaleIndividual.sense;

                //for now, let's just keep the female's diet in the offspring
                globalMemberCounter++;
                offspring.GetComponent<Individual>().NewIndividual(globalMemberCounter, size, speed, sense, femaleIndividual.diet);

                //set some default params on the new offspring
                offspring.GetComponent<Individual>().AteToday = false;
                offspring.GetComponent<Individual>().TimesEatenToday = 0;
                offspring.GetComponent<Individual>().BornToday = true;

                offspringList.Add(offspring);
            }
        }
        //Task t = await Mutate(offspringList);
        //await t;
        Mutate(offspringList);
        Members.AddRange(offspringList);
        populationText.text = "Pop: " + Members.Count;
    }

    public void Mutate(List<GameObject> members)
    {
        foreach (GameObject member in members)
        {
            Individual individual = member.GetComponent<Individual>();
            
            individual.speed = Random.Range(0, 2) != 0 ? individual.speed += mutationAmount : individual.speed -= mutationAmount;
            individual.size = Random.Range(0, 2) != 0 ? individual.size += mutationAmount : individual.size -= mutationAmount;
            individual.sense = Random.Range(0, 2) != 0 ? individual.size += mutationAmount : individual.size -= mutationAmount;

            var maxTraitValue = 1.9f;
            var minTraitValue = 0.1f;

            //clamp min and max values:
            if (individual.speed > maxTraitValue)
            {
                individual.speed = maxTraitValue;
            }
            else if (individual.speed < minTraitValue)
            {
                individual.speed = minTraitValue;
            }

            if (individual.size > maxTraitValue)
            {
                individual.size = maxTraitValue;
            }
            else if (individual.size < minTraitValue)
            {
                individual.size = minTraitValue;
            }

            if (individual.sense > maxTraitValue)
            {
                individual.sense = maxTraitValue;
            }
            else if (individual.sense < minTraitValue)
            {
                individual.sense = minTraitValue;
            }
        }
    }

    public void Scatter()
    {
        Shuffle();
        StartCoroutine(ScatterAnimation());
    }

    public void Shuffle()
    {
        int n = Members.Count;
        var rand = new System.Random();
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            GameObject value = Members[k];
            Members[k] = Members[n];
            Members[n] = value;
        }
    }

    IEnumerator ScatterAnimation()
    {
        float sizeX = plane.transform.localScale.x - 0.5f;
        float sizeZ = plane.transform.localScale.z - 0.5f;
        float posY = plane.transform.localScale.y; //floor

        foreach (GameObject member in Members)
        {
            member.SetActive(false);
        }

        foreach (GameObject member in Members)
        {
            float posX = Random.Range(0, sizeX) * 10;
            float posZ = Random.Range(0, sizeZ) * 10;
            yield return new WaitForSeconds(0.01f);
            member.SetActive(true);
            member.transform.position = new Vector3(posX - 22, posY, posZ - 22);
        }
    }

    public float GetSpeciationAmount()
    {
        return speciationAmount;
    }

    void Start()
    {
        NewPopulation(popStartSize, populationStartingSize, populationStartingSpeed, populationStartingSense, Diet.Herbivore);
        populationText.text = "Pop: " + Members.Count;
    }

    void Update()
    {
        
    }
}
