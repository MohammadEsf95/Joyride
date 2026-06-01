using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    
    public float startSpeed = 5f;
    public float acceleration = 0.5f;
    public float maxSpeed = 100f;

    private float currentSpeed;
    private float _distanceTraveled;

    public float DistanceTraveled => _distanceTraveled;

    void Start()
    {
        currentSpeed = startSpeed;
    }

    void Update()
    {
        currentSpeed = Mathf.Min(currentSpeed+acceleration*Time.deltaTime, maxSpeed);
        float deltaX = currentSpeed * Time.deltaTime;
        transform.position += new Vector3(deltaX, 0f, 0f);
        _distanceTraveled += deltaX;
    }
}
