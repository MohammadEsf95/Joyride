using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    
    public float startSpeed = 5f;
    public float acceleration = 0.5f;
    public float maxSpeed = 100f;

    private float currentSpeed;
    void Start()
    {
        currentSpeed = startSpeed;
    }

    void Update()
    {
        currentSpeed = Mathf.Min(currentSpeed+acceleration*Time.deltaTime, maxSpeed);
        transform.position += new Vector3(currentSpeed * Time.deltaTime, 0f, 0f);
    }
}
