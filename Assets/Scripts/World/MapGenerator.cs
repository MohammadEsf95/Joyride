using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform player;
    
    public GameObject ceiling;
    public GameObject prevCeiling;
    
    
    public GameObject ground;
    public GameObject prevGround;
    void Start()
    {
        
    }

    void Update()
    {
        if (player.position.x > ground.transform.position.x - 10)
        {
            var tmpGround = prevGround;
            prevGround = ground;
            tmpGround.transform.position += new Vector3(60, 0);
            ground = tmpGround;
            
            var tmpCeiling = prevCeiling;
            prevCeiling = ceiling;
            tmpCeiling.transform.position += new Vector3(60, 0);
            ceiling = tmpCeiling;
        }
    }
}
