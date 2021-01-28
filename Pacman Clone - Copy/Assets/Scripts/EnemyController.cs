using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector3 targetPos;
    public int targetOffset;
    public bool scaredyCat;
    public GameObject waypoint;
    public float chaseTimer;
    private bool chasing = true;
    private Vector3 scatterPos;
    public float timeToMove;
    private Vector3 oldPos;
    private Vector3 oldDirection = new Vector3(0,0,0);
    private Vector3 newPos;
    
    private bool isMoving;
    private GameObject player;
    private PlayerController playerScript;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();

        scatterPos = waypoint.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        chaseTimer -= Time.deltaTime;
        if (chaseTimer <= 0)
        {
            chasing = !chasing; //swap between chase and scatter
            oldDirection *= -1; //turn everyone around
            if (chasing)
                chaseTimer = 20f;//reset timer
            else
                chaseTimer = 5f;//scatter timer is shorter as per the original
        }

        if (isMoving) //can't change direction mid-move
            return;
        
        if (chasing)
        {
            targetPos = player.transform.position + (playerScript.facing*targetOffset); //get the player's current location
            if (scaredyCat && Vector3.Distance(targetPos,transform.position) < 4) //run away if the scardycat gets too close
                targetPos = scatterPos;
        } else //scatter mode
        {
            targetPos = scatterPos;
        }
        //start picking a direction
        Vector3 newDirection = new Vector3(0,0,0);
        float distance = 1000000;
        if (oldDirection != Vector3.up && RaycastCheck(Vector3.up) == false) //checking if we're going backwards, then if there's a wall in the way
        {
            //print("Can go Up");
            if (Vector3.Distance(targetPos,transform.position + Vector3.up) <= distance) //comparing our future position's distance
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.up);
                newDirection = Vector3.up;
            }
        }
        //now check the same for the other 3 directions
        if (oldDirection != Vector3.left && RaycastCheck(Vector3.left) == false) 
        {
            //print("Can go left");
            if (Vector3.Distance(targetPos,transform.position + Vector3.left) <= distance) 
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.left);
                newDirection = Vector3.left;
            }
        }
        if (oldDirection != Vector3.down && RaycastCheck(Vector3.down) == false) 
        {
            //print("Can go Down");
            if (Vector3.Distance(targetPos,transform.position + Vector3.down) <= distance) 
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.down);
                newDirection = Vector3.down;
            }
        }
        if (oldDirection != Vector3.right && RaycastCheck(Vector3.right) == false) 
        {
            //print("Can go Right");
            if (Vector3.Distance(targetPos,transform.position + Vector3.right) <= distance) 
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.right);
                newDirection = Vector3.right;
            }
        }
        //print(newDirection);
        //finally move in the chosen direction
        StartCoroutine(MoveEnemy(newDirection));
    }
    private bool RaycastCheck(Vector3 direction)
    {
        Debug.DrawRay(transform.position, direction, Color.red, 0.6f);
        //Physics2D.Raycast(transform.position, direction, 1f);

        //print(hit.collider);
        int layerMask = ~9;
        if (Physics2D.Raycast(transform.position, direction, 0.6f, layerMask))
        {
            //print(true);
            return true;
        }
        //print(false);
        return false;
    }
    private IEnumerator MoveEnemy(Vector3 direction)
    {
        isMoving = true;
        float elapsedTime = 0; 
        oldDirection = direction * -1; //flip it to check if we go backwards later

        oldPos = transform.position;
        targetPos = oldPos + direction;

        while(elapsedTime < timeToMove)
        {
            transform.position = Vector3.Lerp(oldPos,targetPos,(elapsedTime / timeToMove));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        isMoving = false; 
    }
}
