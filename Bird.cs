using System.Collections; //access to collections like List<T>
using System.Collections.Generic;
using UnityEngine; //access to Unity classes (MonoBehaviour, Vector3, Camera, GameObject)

//Contains the behavior of each bird: motion, seek, seperation behavior, physical constraints
public class Bird : MonoBehaviour
{
    public Vector3 position;       // Position of Bird, vector3 is a Unity struct of 3 values(x,y,z),connected to: transform.position
    public Vector3 velocity;       // Velocity of Bird, direction+speed, updates the bird's position: dx/dt
    private Vector3 acceleration;  // Acceleration of Bird, dv/dt

    public float maxSpeed = 20f;    // Maximum speed 
    public float maxAcceleration = 10f; // Maximum acceleration
    public float separationRadius = 2f; // Radius within the Birds avoid each other

    private Camera mainCamera;

    private void Start()
    {
        acceleration = Vector3.zero;
        velocity = Random.insideUnitSphere; // Random initial velocity inside a unit sphere (r=1)
        position = transform.position; //Synchronizes the bird's position with its transform.position
        mainCamera = Camera.main; // will get the camera with the mainCamera tag
    }

    //This code part is for seek behavior towars the mouse
    private void Update() // runs once per frame and determines the bird's behavior
    {
        position = transform.position; //current position of each Bird

        // Get the mouse position in world space
        Vector3 mousePosition = GetMouseWorldPosition();

        // Calculate the direction to the mouse, seek algorithm
        Vector3 desiredAcceleration = (mousePosition - position).normalized * maxAcceleration; // (E-S)normalized * maxacceleration:dv/dt,normalization so the vector only represents direction (without considering distance)
        Vector3 seek = desiredAcceleration - velocity; // in order to reach the right direction eventually, in each frame, vector (x,y,z)

        // seek force to align the seek vector with the acceleration
        seek = Vector3.ClampMagnitude(seek, maxAcceleration); //checks length of seek vector and compares it to maxAcceleration. If seek > maxAcceleration, the method scales down the vector to match the maximum allowed magnitude. If the magnitude is already less than maxAcceleration, it leaves the vector unchanged.
        acceleration = seek; //bird uses the new seek vector as the force that accelerates it toward the mouse position

        // separation behavior
        Vector3 separation = Separation(BirdsManager.Instance.Birds) * 1.5f; //calculates a repulsion vector to avoid overlapping with nearby birds, accesses the Birds list
        acceleration += separation; //adds the separation force to the current acceleration vector, total force that will be applied to the bird in the next frame

        // Clamp acceleration to maxAcceleration
        acceleration = Vector3.ClampMagnitude(acceleration, maxAcceleration); //ensures that acc is within a predifined limit

        // Update position
        UpdatePosition();
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Get the position of the mouse in world space
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = 10f; // Ensure a 10 units distance from the camera
        return mainCamera.ScreenToWorldPoint(mouseScreenPos); //Converts 2D screen coordinates into a 3D world
    }

    private void UpdatePosition() //ensures that birds are above ground - y>0
    {
    // Update velocity and position based on acceleration
    velocity += acceleration * Time.deltaTime;
    velocity = Vector3.ClampMagnitude(velocity, maxSpeed); // Ensure max speed is respected
    position += velocity * Time.deltaTime;

    // Ensure that the bird's y position doesn't go below 0
    if (position.y < 1)
    {
        position.y = 1; // Set y to 0 (or a small value if you want it above ground)
    }

    // Update the bird's position
    transform.position = position;
    }


    private Vector3 Separation(List<Bird> Birds) //The method returns a Vector3, which represents the fleeing force and takes a list of all birds as input
    {
        Vector3 flee = Vector3.zero;
        int count = 0;

        foreach (Bird other in Birds) // Iterate through each Bird in the flock
        {
        // Only consider other Birds that are close to this Bird
        float distance = Vector3.Distance(position, other.position); // Calculate Euclidean distance between current Bird and another

        if (other != this && distance < separationRadius) // Don't compare the Bird with itself, only nearby Birds, sepRadius=2
            {
                // Calculate the direction vector from this Bird to the other
                Vector3 diff = position - other.position; // Calculate the direction (DS), points away from the other bird (S-E)
                diff.Normalize();  // Normalize to get direction only

                // Apply the inverse distance to weight the force
                //separation force prevents colliding by pushing birds away from each other when they are too close
                float weight = 1 / distance;  // Stronger force for closer Birds, 1/eukleidian distance

                // Apply the weighted separation force
                flee += diff * weight; //Adds the weighted separation force to the flee vector
                count++; //how many nearby birds contributed to the separation force
            }
        }

        // If there are nearby Birds, average the separation force
        if (count > 0)
        {
            flee = flee / count; // Divide each component of the flee vector by the count,If there are multiple birds in the separation radius, the total flee vector will have contributions from each of those nearby birds
        }

        // If the separation vector is non-zero, apply the force
        if (flee.magnitude > 0)
        {
            flee = flee.normalized * maxAcceleration - velocity;  // Adjust acceleration based on maxAcceleration and current velocity
            flee = Vector3.ClampMagnitude(flee, maxAcceleration);  // Clamp the flee vector to the max acceleration value
        }

        return flee;

    }
}
