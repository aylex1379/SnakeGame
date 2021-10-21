using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundLooper : MonoBehaviour
{
    [SerializeField] Transform currentGround;
    [SerializeField] Transform[] grounds;
    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, currentGround.position);
        if (dist < 5f)
        {   

            for(int i = 0; i < grounds.Length; i++)
            {
                if(currentGround != grounds[i])
                {   
                    grounds[i].position = currentGround.position + new Vector3(0, 0, 200f);
                    currentGround = grounds[i];
                    break;
                }
            }
        }
    }
}
