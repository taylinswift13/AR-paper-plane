using UnityEngine;
using System.Collections.Generic;

public class ObjectGenerator : MonoBehaviour
{
    public List<GameObject> Trees;
    public List<GameObject> Rocks;
    public List<GameObject> Flowers;
    public List<GameObject> Mushrooms;
    public List<GameObject> Bushes;
    public List<GameObject> Grasses;
    public List<GameObject> Clouds;

    public int TreeAmount;
    public int RockAmount;
    public int FlowerAmount;
    public int MushroomAmount;
    public int BushAmount;
    public int GrassAmount;
    public int CloudAmount;

    public float LeftEdge;
    public float RightEdge;
    public float FrontEdge;
    public float BackEdge;

    public float LowEdge; // Minimum y-axis value for cloud height
    public float UpEdge;  // Maximum y-axis value for cloud height

    public GameObject Parent;

    void Start()
    {
        GenerateObjects(Trees, TreeAmount, Parent);
        GenerateObjects(Rocks, RockAmount, Parent);
        GenerateObjects(Flowers, FlowerAmount, Parent);
        GenerateObjects(Mushrooms, MushroomAmount, Parent);
        GenerateObjects(Bushes, BushAmount, Parent);
        GenerateObjects(Grasses, GrassAmount, Parent);
        GenerateClouds(Clouds, CloudAmount, Parent);
    }

    void GenerateObjects(List<GameObject> objectList, int amount, GameObject parent)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 position = GetRandomPosition();

            if (!IsOverlapping(position))
            {
                if (objectList.Count > 0)
                {
                    GameObject objPrefab = objectList[Random.Range(0, objectList.Count)];
                    GameObject instantiatedObj = Instantiate(objPrefab, parent.transform.position + position, Quaternion.identity);

                    // Set the instantiated object as a child of the specified parent
                    if (parent != null)
                        instantiatedObj.transform.parent = parent.transform;
                }
                else
                {
                    Debug.LogError("Object list is empty. Make sure to assign prefabs to the object lists in the inspector.");
                }
            }
        }
    }

    void GenerateClouds(List<GameObject> cloudList, int amount, GameObject parent)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 position = GetRandomPositionForCloud();

            if (!IsOverlapping(position))
            {
                if (cloudList.Count > 0)
                {
                    GameObject cloudPrefab = cloudList[Random.Range(0, cloudList.Count)];
                    GameObject instantiatedCloud = Instantiate(cloudPrefab, new Vector3(position.x, Random.Range(LowEdge, UpEdge), position.z), Quaternion.identity);

                    // Set the instantiated cloud as a child of the specified parent
                    if (parent != null)
                        instantiatedCloud.transform.parent = parent.transform;
                }
                else
                {
                    Debug.LogError("Cloud list is empty. Make sure to assign cloud prefabs to the object lists in the inspector.");
                }
            }
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(FrontEdge, BackEdge);
        float z = Random.Range(LeftEdge, RightEdge);

        // Ensure that the instantiated object is placed at the ground level (y = 0)
        return new Vector3(x, 0f, z);
    }

    Vector3 GetRandomPositionForCloud()
    {
        float x = Random.Range(FrontEdge, BackEdge);
        float z = Random.Range(LeftEdge, RightEdge);

        return new Vector3(x, 0f, z);
    }

    bool IsOverlapping(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 3f); // Adjust the radius based on your object size

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Ground"))
            {
                return false; // Return true to indicate overlapping with "Ground"
            }
        }

        return true; // Return false if no overlap with "Ground" is found
    }
}
