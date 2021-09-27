using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LSystemTurtle : MonoBehaviour
{
    const int MAKE_PERCENTILE = 100;
    const float SCENE_SCALE_MULTIPLIER = 2.4f;
    #region Inspector Values

    [SerializeField] private RuleSet RuleSet;
    [Range(0, 5)]
    [SerializeField] private int maxGenerations;
    [Range(0, 5)]
    [SerializeField] private int minGenerations;
    [SerializeField] private string nextSentence;

    [Header("Tree")]
    [Range(0.0F, 60.0F)]
    [SerializeField] private float maxAngleVariance;
    [SerializeField] private float startSizeVariance;
    [SerializeField] private float lengthVariance;
    [SerializeField] private float scaleVariance;

    [Header("Leaves")]
    [Range (0.1F, 2f)]
    [SerializeField] private float leafSizeVariance = 1.0f;
    [SerializeField] private int maxLeavesPerBranch;
    [Range(-2.0F, 2.0F)]
    [SerializeField] private float leafTilt;
    [SerializeField] private float leafRotationVariance;
    [SerializeField] private float leafTiltVariance;

    //Prefabs
    [Header("Tree Parts")]
    [SerializeField] private GameObject treeParentPrefab;
    [SerializeField] private GameObject leafParentPrefab;
    [SerializeField] private GameObject branch;
    [SerializeField] private GameObject leaf;
    private GameObject flower;
    private GameObject fruit;
    //Container for different types of tree parts, used to fetch random models
    [SerializeField] private PlantParts plantPartsHolder;

    #endregion

    //Object instantiation
    private GameObject trunkParent;
    private GameObject leafParent;
    private GameObject plantInstance;
    private Vector3 lastBranchPosition;
    private Vector3 startPosition = Vector3.zero;
    private Quaternion startRotation;
    private Quaternion lastLeafRotation;

    //Default values
    private float minAngleBetweenLeaves = 60f;
    private float treeRotation = 360f;
    private float yAxisAngle = 120f;
    private float length = 1f;
    private float branchSizeMultiplier = 0.8f;
    private float treeStartSize = 1f;
    private float flowerSpawnRate = 0.12f;
    private float generationEffectOnSize = 0.25f;


    private Dictionary<char, string> ruleSet = new Dictionary<char, string>();
    private Stack<BranchInfo> branchStack = new Stack<BranchInfo>();
    private string finalSentence;

    //Combine mesh
    private CombineInstance[] leafMeshArray; 
    private CombineInstance[] meshArray;
    private int counter = 0;
    private int numOfLogMeshes;
    private int leafCounter = 0;
    private int numOfLeafMeshes;

    private void Awake()
    {
        startRotation = transform.rotation;
        leafTiltVariance = leafTiltVariance / MAKE_PERCENTILE;
        leafRotationVariance = leafRotationVariance/ MAKE_PERCENTILE;
        InitRuleSet();
    }
    private void InitRuleSet()
    {
        #region Fallback, Obsolete ruleset
        /*if (ruleSet.Count < 1)
        {
            //X → F+[[X]-X]-F[-FX]+X, (F → FF)
            ruleSet.Add('F', "FX");
            ruleSet.Add('-', "-");
            ruleSet.Add('+', "+");
            ruleSet.Add('X', "[*+FX][+FX][/+F-FX]");

            /*
            //Buske
            ruleSet.Add('X', "[*+FX][+FX][/+F-FX]");
            ruleSet.Add('F', "FX")             
        }*/
        #endregion
        RuleSet.EnableRuleSet();
    }

    private string ExpandString(int generations)
    {    
        for(int i = 0; i < generations; i++)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in nextSentence)
            {
                sb.Append(RuleSet.dic.ContainsKey(c) && RuleSet.dic[c].Length > 0  ? RuleSet.dic[c] : c.ToString());       
            }         
            nextSentence = sb.ToString();
        }
        //Count size of mesh arrays
        foreach (char c in nextSentence)
            if (c == 'F')
                numOfLogMeshes++;
            else if (c == 'X' ||c == 'L')
                numOfLeafMeshes += maxLeavesPerBranch;

        //Allocating a bit too much space but the leaf array cant be handled in any other way (known to me)
        meshArray = new CombineInstance[numOfLogMeshes];
        leafMeshArray = new CombineInstance[numOfLeafMeshes];

        return nextSentence;
    }
    private void ResetValues()
    {
        nextSentence = RuleSet.axiom;
        transform.rotation = startRotation;
        transform.position = startPosition;
        branchStack.Clear();

        //Combine mesh holders / parent objects
        plantInstance = new GameObject(RuleSet.name);
        plantInstance.transform.position = transform.position;
        trunkParent = Instantiate(treeParentPrefab, plantInstance.transform);
        leafParent = Instantiate(leafParentPrefab, plantInstance.transform);

        //If possible, use RuleSet's values
        length = (RuleSet.length != 0f ? RuleSet.length : length) + Random.Range(1, 1 + lengthVariance);
        yAxisAngle = RuleSet.yAxis != 0f ? RuleSet.yAxis : yAxisAngle;
        branchSizeMultiplier = RuleSet.branchSizeMultiplier != 0f ? RuleSet.branchSizeMultiplier : branchSizeMultiplier;
        treeStartSize = (RuleSet.treeStartSize != 0f ? RuleSet.treeStartSize : treeStartSize) + Random.Range(1, 1 + startSizeVariance);
        
        //Get random available models
        leaf = plantPartsHolder.GetLeaf();
        branch = plantPartsHolder.GetLog();
        flower = plantPartsHolder.GetFlower();
        fruit = plantPartsHolder.GetFruit();

        //Apply inspector values
        transform.localScale = new Vector3(treeStartSize, length, treeStartSize);

        //Combine mesh counter
        counter = 0;
        leafCounter = 0;
        numOfLogMeshes = 0;
        numOfLeafMeshes = 0;
    }
    public GameObject GenerateRandom()
    {
        RuleSet = plantPartsHolder.GetRuleSet();
        Generate();
        return trunkParent;
    }
    public void Generate()
    {
        int generations = RandomizeGenerations();
        //Debug.Log("Generations: " + generations);

        RuleSet.EnableRuleSet();
        ResetValues();

        finalSentence = ExpandString(generations);

        for(int i = 0; i < finalSentence.Length; i++)
        {           
            ProcessChar(finalSentence[i], i);
        }

        SetRotationAndScale(generations);       
        MeshCombine();
        AddCollider();
    }

    private void ProcessChar(char c, int i)
    {
       
        switch (c)
        {
            #region Instantiation Switch Cases

            case 'F':
                SpawnLog();               
                break;
            case 'X':               
                //CreateLeaves
                int numOfLeaves = Random.Range(1, maxLeavesPerBranch);
                float anglePerLeaf = 360 / numOfLeaves > minAngleBetweenLeaves ? minAngleBetweenLeaves : 360 / numOfLeaves;
                //Last leaf rotation should only be set for the first leaf to be placed
                lastLeafRotation = SpawnLeaf(0, anglePerLeaf);
                for(int j = 0; j < numOfLeaves; j++)
                {
                    SpawnLeaf(j, anglePerLeaf);
                }
                break;
                //OBS DUPLICATED CODE
                //Most RuleSets already depend on X generating leaves, but i would rather use a separate char 'L' to better control their spawn points and frequency -
                //This however, requires some refactoring of the rulesets themselves, and there's no time to do that right now. 
            case 'L':
                //CreateLeaves
                int _numOfLeaves = Random.Range(1, maxLeavesPerBranch);
                float _anglePerLeaf = 360 / _numOfLeaves > minAngleBetweenLeaves ? minAngleBetweenLeaves : 360 / _numOfLeaves;

                for (int j = 1; j < _numOfLeaves; j++)
                {
                    SpawnLeaf(j, _anglePerLeaf);
                }
                //Need another rule to be empty. Named it L because i planned to make this the (L)eaf instantiation, and X be kept empty instead
                break;
            case 'O':
                SpawnFlower();
                break;
           /* case 'G':
                SpawnFruit();
                break;*/
            #endregion
            #region Rotation Switch Cases
            //'+' & '-' rotate around Z-axis
            case '+':
                transform.Rotate(Vector3.forward * (RuleSet.branchAngle + Random.Range(-maxAngleVariance, maxAngleVariance)));
                break;

            case '-':
                transform.Rotate(Vector3.back * (RuleSet.branchAngle + Random.Range(-maxAngleVariance, maxAngleVariance)));             
                break;

                //'*' & '/' rotate around Y-axis
            case '*':
                transform.Rotate(Vector3.up * (yAxisAngle + Random.Range(-maxAngleVariance, maxAngleVariance)));
                break;

            case '/':
                transform.Rotate(Vector3.down * (yAxisAngle + Random.Range(-maxAngleVariance, maxAngleVariance)));
                break;
            #endregion

            //'[' & ']' open and close branch respectively
            case '[':
                branchStack.Push(new BranchInfo()
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    scale = transform.localScale
                }) ;
                transform.localScale = new Vector3(transform.localScale.x * branchSizeMultiplier, transform.localScale.y, transform.localScale.z * branchSizeMultiplier);
                break;

            case ']':
                BranchInfo ti = branchStack.Pop();
                transform.localScale = ti.scale;
                transform.position = ti.position;
                transform.rotation = ti.rotation;
                break;
        }
    }
    private void SpawnFruit()
    {
        //only sometimes spawn fruit
        //only spawn fruit on branches which point.. down somewhat? Dot(transform.rotation, Vector3.down) > 0;
    }
    //Flower meshes are not combined and therefore can have "unique" colors
    private void SpawnFlower()
    {
        if (Random.Range(0f, 1f) < flowerSpawnRate)
        {
            GameObject flowerInstance = Instantiate(flower, plantInstance.transform);
            flowerInstance.transform.position = transform.position;
            flowerInstance.transform.rotation = lastLeafRotation;
            flowerInstance.GetComponent<MeshRenderer>().material.color = RandomizeColor(RuleSet.flowerColor);
        }
    }
    private void SpawnLog()
    {
        GameObject branchInstance = Instantiate(branch, trunkParent.transform);
        branchInstance.transform.position = transform.position;
        branchInstance.transform.rotation = transform.rotation;
        branchInstance.transform.localScale = transform.localScale;
        transform.Translate(Vector3.up * length * 2f);

        lastBranchPosition = branchInstance.transform.position;
        //Combine meshes               
        AddLogMeshToCombine(branchInstance);
    }
    private Quaternion SpawnLeaf(int j, float anglePerLeaf)
    {
        GameObject leafInstance = Instantiate(leaf, trunkParent.transform);
        leafInstance.transform.position = transform.position;
        leafInstance.transform.localScale *= treeStartSize + Random.Range(1, leafSizeVariance);
        leafInstance.transform.rotation = RotateLeaf(j, anglePerLeaf, leafInstance);
        
        //Combine Mesh
        AddLeafMeshToCombine(leafInstance);

        return leafInstance.transform.rotation;
    }
    private Quaternion RotateLeaf(int j, float anglePerLeaf, GameObject leafInstance)
    {
        //Rotation
        Vector3 firstLeafDirection = lastBranchPosition - leafInstance.transform.position;
        //Point in SOME direction, even if the direction vector turns out to be zero
        firstLeafDirection = firstLeafDirection == Vector3.zero ? new Vector3(1f, 0, 1f) : firstLeafDirection;

        //This rotation will only apply to leaves AFTER the first one at the position
        Vector3 zAxis = new Vector3(firstLeafDirection.x, 0, firstLeafDirection.z).normalized;
        float theta = Mathf.Deg2Rad * (anglePerLeaf * j);
        float x = zAxis.x * Mathf.Cos(theta) - zAxis.z * Mathf.Sin(theta);
        float z = zAxis.x * Mathf.Sin(theta) + zAxis.z * Mathf.Cos(theta);

        //Axises of the final rotation
        Vector3 forwardDirection = new Vector3(x, 0, z) + ((leafTilt + Random.Range(-leafTiltVariance, leafTiltVariance)) * Vector3.up);
        Vector3 upwardDirection = Vector3.up + Vector3.up * Random.Range(-leafRotationVariance, leafRotationVariance);

        return Quaternion.LookRotation(forwardDirection, upwardDirection);
    }
    private Color RandomizeColor(Color color)
    {
        float colorVariance = RuleSet.colorVariance;
        float r = color.r + Random.Range(-colorVariance, colorVariance);
        float g = color.g + Random.Range(-colorVariance, colorVariance);
        float b = color.b + Random.Range(-colorVariance, colorVariance);
        return new Color(r, g, b);
    }
        

    //Combine trunk and leaf submeshes and color them according to RuleSet
    private void MeshCombine()
    {
        Mesh treeParentMesh = trunkParent.GetComponent<MeshFilter>().mesh;
        treeParentMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        treeParentMesh.CombineMeshes(meshArray, true, true);
        trunkParent.GetComponent<MeshRenderer>().material.color = RandomizeColor(RuleSet.trunkColor) ;

        Mesh leafParentMesh = leafParent.GetComponent<MeshFilter>().mesh;
        leafParentMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        leafParentMesh.CombineMeshes(leafMeshArray, true, true);
        leafParent.GetComponent<MeshRenderer>().material.color = RandomizeColor(RuleSet.leafColor);
    }
    private void AddLogMeshToCombine(GameObject instance)
    {
        meshArray[counter].mesh = instance.GetComponentInChildren<MeshFilter>().mesh;
        meshArray[counter].transform = instance.transform.GetChild(0).transform.localToWorldMatrix;
        Destroy(instance);
        counter++;
    }
    private void AddLeafMeshToCombine(GameObject instance)
    {
        leafMeshArray[leafCounter].mesh = instance.GetComponentInChildren<MeshFilter>().mesh;
        leafMeshArray[leafCounter].transform = instance.transform.GetChild(0).transform.localToWorldMatrix;
        Destroy(instance);
        leafCounter++;
    }
    private void AddCollider()
    {
        BoxCollider newColl = trunkParent.AddComponent<BoxCollider>();
        newColl.isTrigger = true;
        newColl.size *= 0.9f;
    }
    private int RandomizeGenerations()
    {
        int min = RuleSet.minGenerations > 0 ? RuleSet.minGenerations : minGenerations;
        int max = RuleSet.maxGenerations > 0 ? RuleSet.maxGenerations : maxGenerations;
        return Random.Range(min, max + 1);
    }
    private void SetRotationAndScale(int generations)
    {
        plantInstance.transform.Rotate(new Vector3(0, 1, 0) * Random.Range(0f, treeRotation));
        plantInstance.transform.localScale *= Random.Range(1 - scaleVariance, 1 + scaleVariance);
        // The multiplication value below could be a constant, but the number of possible generations are far from homogenous,
        //so it is instead multiplied by value of generations/maxGenerations, to preserve some sort of consistency between RuleSets. 
        //plantInstance.transform.localScale *= 1f + ((generations / maxGenerations) * generationEffectOnSize );
        plantInstance.transform.localScale /= SCENE_SCALE_MULTIPLIER;
    }
}

