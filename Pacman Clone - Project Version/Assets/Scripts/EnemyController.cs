using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector3 targetPos;
    public int targetOffset;
    public bool scaredyCat;
    public GameObject waypoint;
    public GameObject boxWaypoint;
    private float chaseTimer;
    public float maxChaseTimer;
    public float maxScatterTimer;
    public float startPercent;
    public bool Vulnerable = false; //for when pacman eats a power pellet
    private float vulTimer;
    public float maxVulTimer;
    private bool chasing = true;
    private Vector3 scatterPos;
    public float timeToMove;
    private Vector3 oldPos;
    private Vector3 oldDirection = new Vector3(0,0,0);
    private Vector3 newPos;
    private bool isMoving;
    private GameObject player;
    private PlayerController playerScript;
    private int maxPellets;
    private bool eaten = false;
    private float eatenTimer;

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
            print("Found Script");
        playerScript = player.GetComponent<PlayerController>();

        scatterPos = waypoint.transform.position;
        chaseTimer = maxChaseTimer;
        maxPellets = playerScript.pelletsLeft;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        
        if (Vulnerable == false)
        {
            chaseTimer -= Time.deltaTime;
        } else //vulnerable is true
        {
            vulTimer -= Time.deltaTime;
        }

        if (chaseTimer <= 0)
        {
            chasing = !chasing; //swap between chase and scatter
            oldDirection *= -1; //turn everyone around
            if (chasing)
            {
                maxChaseTimer += 5;
                chaseTimer = maxChaseTimer;//reset timer
            }else
            {
                maxScatterTimer *= 0.9f;
                chaseTimer = maxScatterTimer;//scatter timer is shorter as per the original
            } 
        }

        if (eaten == true && transform.position == boxWaypoint.transform.position)
        {
            eaten = false;
            GetComponent<CircleCollider2D>().enabled = true;
            //animation state change back to normal
        }

        if (Vulnerable == true && vulTimer < 0)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            Vulnerable = false;
        }
        //print(playerScript.pelletsLeft);
        if (playerScript.pelletsLeft > (maxPellets * startPercent)) //stops ghosts from moving until the player has eaten a certain percentage of pellets
            return;

        if (isMoving) //can't change direction mid-move
            return;
        
        if (Mathf.Abs(transform.position.x) > 10)
            TeleportFlip();
        
        if (eaten)
        {
            targetPos = boxWaypoint.transform.position;
        }else if (chasing && !Vulnerable)
        {
            targetPos = player.transform.position + (playerScript.facing*targetOffset); //get the player's current location
            if (scaredyCat && Vector3.Distance(targetPos,transform.position) < 4) //run away if the scardycat gets too close
                targetPos = scatterPos;
        } else //scatter mode or Vulnerable mode
        {
            targetPos = scatterPos;
        }
        //start picking a direction
        Vector3 newDirection = new Vector3(0,0,0);
        float distance = 1000000;
        float rotation = 0;
        if (oldDirection != Vector3.up && RaycastCheck(Vector3.up) == false) //checking if we're going backwards, then if there's a wall in the way
        {
            //print("Can go Up");
            if (Vector3.Distance(targetPos,transform.position + Vector3.up) <= distance) //comparing our future position's distance
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.up);
                newDirection = Vector3.up;
                rotation = 180;
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
                rotation = 270;
            }
        }
        if (oldDirection != Vector3.down && RaycastCheck(Vector3.down) == false) 
        {
            //print("Can go Down");
            if (Vector3.Distance(targetPos,transform.position + Vector3.down) <= distance) 
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.down);
                newDirection = Vector3.down;
                rotation = 0;
            }
        }
        if (oldDirection != Vector3.right && RaycastCheck(Vector3.right) == false) 
        {
            //print("Can go Right");
            if (Vector3.Distance(targetPos,transform.position + Vector3.right) <= distance) 
            {
                distance = Vector3.Distance(targetPos,transform.position + Vector3.right);
                newDirection = Vector3.right;
                rotation = 90;
            }
        }
        //print(newDirection);
        //rotate based on facing
        transform.rotation = Quaternion.Euler(0,0,rotation);
        //finally move in the chosen direction
        //if (!gameOver)
        {
            StartCoroutine(MoveEnemy(newDirection));
        }
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
    void TeleportFlip()
    {
        Vector3 temp = transform.position;
        if (temp.x > 0)
            temp.x -= 1;
        else
            temp.x += 1;
        temp.x *= -1;
        transform.position = temp;
    }
    public void VulnerableState()
    {
        Vulnerable = true;
        vulTimer = maxVulTimer;
        GetComponent<SpriteRenderer>().color = Color.blue; // change this to an animation state change
    }

    public void Eaten()
    {
        eaten = true;
        GetComponent<CircleCollider2D>().enabled = true;
        //add another animation state change here
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
