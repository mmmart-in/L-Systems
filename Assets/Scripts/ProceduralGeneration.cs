//Mattias Larsson mala7086
using System.Collections.Generic;
using UnityEngine;

public class PlantPlacer : MonoBehaviour
{
    [SerializeField] private LSystemTurtle LTurtle;
    [SerializeField] private GameObject Player;
    [SerializeField] private float singlePlantSpawnMargin;
    private GameObject currentPlant;
    private float heightMargin = -0.5f;
    private RaycastHit hit;
    private BoxCollider coll;

    private int maxNumOfRecursions = 10;
    private int iterations = 0;
    private int originalSpawnHeight = 200; 

    [SerializeField]private LayerMask groundLayer;

    [SerializeField]private float minimumX, maximumX, minimumZ, maximumZ;

    [SerializeField]
    [Tooltip("Hur många plantor som spawnas")]
    private int plantAmount; 



    private void Awake()
    {
        //coll = 
        //SpawnPlants();
        SpawnPlantIndividual();
    }

    private void SpawnPlants()
    {
        for (int i = 0; i < plantAmount; i++)
        {
            SpawnPlantIndividual();
        }
    }
    private void SpawnPlantIndividual()
    {
        iterations = 0;
        GameObject instance = LTurtle.GenerateRandom();
        currentPlant = instance;
        coll = instance.GetComponent<BoxCollider>();
        PrePositionRandomiser(instance);
        //instance.transform.parent.transform.position = PositionRandomiser(instance);
    }
    private void MoveCurrentPlant()
    {
        currentPlant.transform.parent.transform.position = AdjustYValue(currentPlant);
    }

    private void PrePositionRandomiser(GameObject instance)
    {
        float x = Random.Range(minimumX, maximumX);
        float z = Random.Range(minimumZ, maximumZ);
        Transform plantInstance = instance.transform.parent;
        plantInstance.position = new Vector3(x, originalSpawnHeight, z);

        //Debug.Log("Parent position before adjusting y: " + instance.transform.parent.transform.position);

    }

    private Vector3 PositionRandomiser(GameObject instance)
    {
        float x = Random.Range(minimumX, maximumX);
        float z = Random.Range(minimumZ, maximumZ);
        GameObject plantParent = instance.transform.parent.gameObject;
        plantParent.transform.position = new Vector3(x, originalSpawnHeight, z);
       
        Vector3 pos = AdjustYValue(instance);
        
        return pos;
    }
   
    private Vector3 AdjustYValue(GameObject trunkParent)
    {
        iterations++;

        //If we cannot find a position for it, destroy the plant
        if (iterations >= maxNumOfRecursions)
        {
            Destroy(trunkParent.transform.parent.gameObject);
            return Vector3.zero;
        }

        //Control value
        float y = -5f;
        coll = trunkParent.GetComponent<BoxCollider>();


        if (Physics.BoxCast(trunkParent.transform.position, coll.bounds.size / 2, Vector3.down, out hit, Quaternion.identity, originalSpawnHeight + 20, groundLayer))
        {         
            if (hit.collider.CompareTag("Plant"))
            {
                Debug.Log("Oops, hit " + hit.collider.gameObject.transform.parent.name + ", fetching new pos!");
                return PositionRandomiser(trunkParent);
            }
            else if (hit.point.y < 0)
            {
                Debug.Log("Invalid position");
            }
            else
            {
                Debug.Log("Placing object at hit point" + hit.point.y);
                return new Vector3(trunkParent.transform.position.x, hit.point.y + heightMargin, trunkParent.transform.position.z);
            }
        }
        else
        {
            Debug.LogError("BoxCast hit nothing");
        }

        return new Vector3(trunkParent.transform.position.x, y, trunkParent.transform.position.z);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) //Test
        {
            SpawnPlantIndividual();
        }
        if (Input.GetKeyDown(KeyCode.B)) //Test
        {
            MoveCurrentPlant();
        }
    }


    #region Failed SphereCast & BoxCast

    /*if (Physics.BoxCast(coll.bounds.center, coll.bounds.size, Vector3.down, out hit, Quaternion.identity, OriginalSpawnHeight + 20))
    {
        //Stack overflow guard 
        if (iterations >= maxNumOfRecursions)
            Destroy(trunkParent);

        if (hit.collider.CompareTag("Plant"))
        {
            Debug.Log("Oops, hit a plant, fetching new pos!");
            iterations++;
            return AdjustYValue(trunkParent);
        }
        else if (hit.point.y < 0)
        {
            Debug.Log("Invalid position");
        }
        else
        {
            Debug.Log("Placing object at hit point" + hit.point.y);
            return hit.point.y + heightMargin;
        }
    }
    else
    {
        Debug.LogError("BoxCast hit nothing");
        Debug.Log("Yvalue of trunkparent is: " + trunkParent.transform.position.y);
    }

    return -5f;
    float radius = Mathf.Max(coll.bounds.size.x, coll.bounds.size.z) / 2;
    if (Physics.SphereCast(coll.center, radius, Vector3.down, out hit, OriginalSpawnHeight * 2))
    {
        //Stack overflow guard 
        if (iterations >= maxNumOfRecursions)
        {
            return -5f;
        }
        Debug.Log("Hit collider: " + hit.collider.gameObject.name);
        if (hit.collider.CompareTag("Plant"))
        {
            Debug.Log("Oops, hit a plant, fetching new pos!");
            iterations++;
            return AdjustYValue(trunkParent);
        }
        else if (hit.point.y < 0)
            Debug.Log("Invalid position");
        else
        {
            Debug.Log("Placing object at hit point" + hit.point.y);
            return hit.point.y + heightMargin;
        }
    }
    Debug.LogError("SphereCast hit nothing");
    return -5f;
}*/
    #endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(coll.bounds.center, Vector3.down * originalSpawnHeight);
        Gizmos.DrawWireCube(new Vector3(coll.bounds.center.x, originalSpawnHeight, coll.bounds.center.z) + Vector3.down * originalSpawnHeight, coll.bounds.size / 2);

    }

}
