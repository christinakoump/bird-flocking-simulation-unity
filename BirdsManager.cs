using System.Collections; //working with collections like List<T>
using System.Collections.Generic;
using UnityEngine; //access to Unity-specific classes (MonoBehaviour, Vector3, Camera, GameObject)


public class BirdsManager : MonoBehaviour // MonoBehaviour, BirdsManager is a Unity script and can interact with Unity methods: Start(), Update()
{
    public static BirdsManager Instance; // A static reference to ensure there is only one BirdsManager instance,a static variable is shared among all instances of a class
    public List<Bird> Birds; //list of all birds

    public GameObject BirdPrefab;  // Drag the Bird prefab here
    public int BirdCount = 50;

    private void Awake() //before the game starts
    {
        // Singleton pattern for BirdsManager
        if (Instance == null) //it means that there is the only one that exists
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); //destroys the existant instance to ensure there is only one manager
        }
    }

    private void Start()
    {
        // Spawn initial Birds
        Birds = new List<Bird>(); //initializes a new empty list that can hold Bird objects

        for (int i = 0; i < BirdCount; i++)
        {
            // Spawn Birds in a 3D space (within a smaller spherical area)
            Vector3 spawnPosition = Random.insideUnitSphere * 5f; // 5f is the radius of the flock area
            GameObject BirdObject = Instantiate(BirdPrefab, spawnPosition, Quaternion.identity);
            Birds.Add(BirdObject.GetComponent<Bird>());
        }
    }

}
