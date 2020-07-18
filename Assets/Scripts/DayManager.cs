using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    public int dayCount;
    public Population pop;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI populationText;
    public Timer timer;
    public Environment environment;
    public GameObject individualPrefab;
    public GameObject speciesScenePrefab;
    public GameObject canvasObject;
    public GameObject rawImagePrefab;

    private float speciationAmount;
    public void StartDay()
    {
        //check for async bug:
        var foundObjects = FindObjectsOfType<Individual>().Where(i => i.id >= 0);
        if(foundObjects.Count() != pop.Members.Count)
        {
            pop.Members = new List<GameObject>();
            foreach(Individual obj in foundObjects)
            {
                pop.Members.Add(obj.gameObject);
            }
        }

        CheckForSpecies();

        dayCount++;
        dayText.text = "Day " + dayCount;
        populationText.text = "Pop: " + pop.Members.Count;
        pop.Scatter();
        environment.ScatterFood();
        timer.timeLeft = 10;
        timer.enableTimer = true; //restart timer

        foreach(GameObject member in pop.Members)
        {
            member.GetComponent<Individual>().BornToday = false;
            member.GetComponent<Individual>().TimesEatenToday = 0;
            member.GetComponent<Individual>().AteToday = false;
            member.GetComponent<Individual>().energy = 100000;
            member.GetComponent<Individual>().StartWandering();
            member.GetComponent<NavMeshAgent>().enabled = true;
            member.GetComponent<Wander>().enabled = true;
        }
    }
    public IEnumerator EndDay()
    {
        pop.Members.All(m => { m.GetComponent<NavMeshAgent>().enabled = false; m.GetComponent<Wander>().enabled = false; return true; });
        for (int i = pop.Members.Count; i-- > 0;)
        {
            var member = pop.Members[i];

            if (!member.GetComponent<Individual>().AteToday)
            {
                MeshRenderer renderer = member.GetComponent<MeshRenderer>();
                renderer.material.color = Color.red;
                yield return new WaitForSeconds(10/pop.Members.Count);
                Destroy(member); //kill individuals that didn't eat today
                pop.Members.RemoveAt(i); //then remove from the list
                populationText.text = "Pop: " + pop.Members.Count;
            }
        }

        pop.Reproduce();
        yield return new WaitForSeconds(2f);
        StartDay();
    }

    void CheckForSpecies()
    {
        speciationAmount = pop.GetSpeciationAmount();

        IEnumerable<Individual> members = pop.Members.Select(m => m.GetComponent<Individual>());

        List<IEnumerable<Individual>> matingGroups = new List<IEnumerable<Individual>>();
        List<IEnumerable<Individual>> speciesList = new List<IEnumerable<Individual>>();

        speciesList = RecursiveSpeciationCheck(members, speciesList);

        //Destroy existing UI renderTextures
        GameObject[] renderTextures = GameObject.FindGameObjectsWithTag("SpeciesRenderTexture");
        GameObject[] speciesScenes = GameObject.FindGameObjectsWithTag("SpeciesScene");
        foreach (GameObject renderTexture in renderTextures)
        {
            Destroy(renderTexture);
        }
        //Destory existing render scenes
        foreach (GameObject speciesScene in speciesScenes)
        {
            Destroy(speciesScene);
        }

        //Destory all dummy creature instances
        foreach (GameObject member in GameObject.FindGameObjectsWithTag("Individual"))
        {
            if(member.GetComponent<Individual>().id < 0)
            Destroy(member);
        }

        int count = 0;
        foreach (IEnumerable<Individual> spec in speciesList)
        {
            if (speciesList.Any())
            {
                CreateAverageIndividualFromMatingGroup(spec, speciesList.Count, count);
                count++;
            }
        }

    }
    private List<IEnumerable<Individual>> RecursiveSpeciationCheck(IEnumerable<Individual> members, List<IEnumerable<Individual>> speciesList)
    {
        List<IEnumerable<Individual>> matingGroups = new List<IEnumerable<Individual>>();

        foreach (Individual member in members)
        {
            IEnumerable<Individual> viableMates;

            viableMates = members.Where(m => (m.size < (member.size + speciationAmount) && m.size > (member.size - speciationAmount))
                && (m.speed < (member.speed + speciationAmount) && m.speed > (member.speed - speciationAmount))
                && (m.sense < (member.sense + speciationAmount) && m.sense > (member.sense - speciationAmount)));
            if (viableMates.Any())
            {
                matingGroups.Add(viableMates);
            }

        }

        IEnumerable<Individual> largestMatingGroup;
        if (matingGroups.Any())
        {
            largestMatingGroup = matingGroups.Aggregate((i1, i2) => i1.Count() > i2.Count() ? i1 : i2);
            if (largestMatingGroup.Any())
            {
                speciesList.Add(largestMatingGroup);
                var otherSpecies = members.Except(largestMatingGroup);
                if (otherSpecies.Any())
                {
                    RecursiveSpeciationCheck(otherSpecies, speciesList);
                }
            }
        }
        
        return speciesList;
    }

    private void CreateAverageIndividualFromMatingGroup(IEnumerable<Individual> group, int groupCount, int count)
    {
        RenderTexture renderTexture;
        renderTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        renderTexture.Create();
        var sceneInstance = Instantiate(speciesScenePrefab, new Vector3(-((count + 1) * 50), -((count+1) * 50), -((count + 1) * 50)), Quaternion.identity);
        sceneInstance.GetComponentsInChildren<Camera>().First().targetTexture = renderTexture;

        var individualInstance = Instantiate(individualPrefab, sceneInstance.transform.position, Quaternion.identity);
        float averageSize = group.Select(i => i.size).Average();
        float averageSpeed = group.Select(i => i.speed).Average();
        float averageSense = group.Select(i => i.sense).Average();
        individualInstance.GetComponent<Individual>().NewIndividual(-1, averageSize, averageSpeed, averageSense, group.First().diet);
        individualInstance.GetComponent<NavMeshAgent>().enabled = false;
        individualInstance.GetComponent<Wander>().enabled = false;

        var imageTransform = canvasObject.GetComponentInChildren<RawImage>().transform;

        var image = Instantiate(rawImagePrefab,
            new Vector3(imageTransform.position.x,
                        imageTransform.position.y - (150*count),
                        imageTransform.position.z), 
            Quaternion.identity);
        image.transform.parent = canvasObject.transform;
        image.GetComponent<RawImage>().texture = renderTexture;
        var label = image.GetComponentInChildren<TextMeshProUGUI>();
        label.text = $"Species {count+1}\nPop: {group.Count()}\nSize: {averageSize.ToString("0.00")}\nSpeed: {averageSpeed.ToString("0.00")}\nSense: {averageSense.ToString("0.00")}";
    }

    //Obsolete. Using recursive check for speciation instead. Might wanna keep this code around though
    private void CheckForLargestMatingGroups(IEnumerable<Individual> members, List<IEnumerable<Individual>> matingGroups, List<IEnumerable<Individual>> speciesList)
    {
        foreach (GameObject go in pop.Members)
        {
            Individual member = go.GetComponent<Individual>();
            IEnumerable<Individual> viableMates;

            viableMates = members.Where(m => (m.size < (member.size + speciationAmount) && m.size > (member.size - speciationAmount))
                && (m.speed < (member.speed + speciationAmount) && m.speed > (member.speed - speciationAmount))
                && (m.sense < (member.sense + speciationAmount) && m.sense > (member.sense - speciationAmount)));
            matingGroups.Add(viableMates);

        }

        IEnumerable<Individual> largestMatingGroup = matingGroups.Aggregate((i1, i2) => i1.Count() > i2.Count() ? i1 : i2);

        speciesList.Add(largestMatingGroup);

        var otherSpecies = members.Except(largestMatingGroup);
        if (otherSpecies.Any())
        {
            speciesList.Add(otherSpecies);
        }

        //IEnumerable<Individual> largestMatingGroup = matingGroups.Max(mg => mg.Count());
        for (int i = matingGroups.Count(); i-- > 0;)
        {
            IEnumerable<Individual> currentMatingGroup = matingGroups[i];

            for (int j = i; j < matingGroups.Count() - 1; j++)
            {
                if (i != j)
                {
                    IEnumerable<Individual> otherMatingGroup = matingGroups[j];

                    List<int> currentGroupIds = currentMatingGroup.Select(m => m.id).ToList();
                    List<int> otherGroupIds = otherMatingGroup.Select(m => m.id).ToList();

                    if (currentGroupIds.All(otherGroupIds.Contains) && currentGroupIds.Count == otherGroupIds.Count)
                    {
                        matingGroups.RemoveAt(j);
                    }
                }
            }
        }

        if (matingGroups.Count() > 1)
        {
            matingGroups.RemoveAt(0);
        }
    }

    void Start()
    {
        dayCount = 1;
    }

    void Update()
    {
        
    }
}
