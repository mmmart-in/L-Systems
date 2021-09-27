using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantParts : MonoBehaviour
{
    public List<GameObject> leafPrefabs = new List<GameObject>();
    public List<GameObject> flowerPrefabs = new List<GameObject>();
    public List<GameObject> fruitPrefabs = new List<GameObject>();
    public List<GameObject> logPrefabs = new List<GameObject>();
    public List<RuleSet> ruleSets = new List<RuleSet>();
    //List<Color> ?? 
    public GameObject GetLeaf()
    {
        return leafPrefabs[Random.Range(0, leafPrefabs.Count)];
    }
    public GameObject GetFlower()
    {
        return flowerPrefabs[Random.Range(0, flowerPrefabs.Count)];
    }
    public GameObject GetFruit()
    {
        return fruitPrefabs[Random.Range(0, fruitPrefabs.Count)];
    }
    public GameObject GetLog()
    {
        return logPrefabs[Random.Range(0, logPrefabs.Count)];
    }
    public RuleSet GetRuleSet()
    {
        return ruleSets[Random.Range(0, ruleSets.Count)];
    }
}
