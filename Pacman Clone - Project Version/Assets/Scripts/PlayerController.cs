using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
public class PlayerController : MonoBehaviour
{
    public Text winText;
    public Text scoreText;
    private int score= 0;
    public float timeToMove;
    public Tilemap pelletMap;
    private Vector3 oldPos;
    private Vector3 targetPos;
    public Vector3 facing;
    public GameObject[] ghosts;
    public int pelletsLeft;
    
    private bool isMoving;
    private bool gameOver = false;

    private EnemyController ghostScript;

    void Start()
    {
        winText.text = "";
        AddScore(0);
        
        BoundsInt bounds = pelletMap.cellBounds; //count up all the pellets on the map
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = pelletMap.GetTile<Tile>(pos);
            if (tile != null)
                pelletsLeft += 1;
        }
        print (pelletsLeft);
        ghosts = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject ghost in ghosts)
            {
                ghostScript = ghost.GetComponent<EnemyController>();
            }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.x) > 10)
            TeleportFlip();
        
        if (!gameOver)
        {
            if (Input.GetKey(KeyCode.W) && !isMoving && RaycastCheck(Vector3.up) == false)
                StartCoroutine(MovePlayer(Vector3.up));

            if (Input.GetKey(KeyCode.A) && !isMoving && RaycastCheck(Vector3.left) == false)
                StartCoroutine(MovePlayer(Vector3.left));

            if (Input.GetKey(KeyCode.S) && !isMoving && RaycastCheck(Vector3.down) == false)
                StartCoroutine(MovePlayer(Vector3.down));

            if (Input.GetKey(KeyCode.D) && !isMoving && RaycastCheck(Vector3.right) == false)
                StartCoroutine(MovePlayer(Vector3.right));
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    private bool RaycastCheck(Vector3 direction)
    {
        Debug.DrawRay(transform.position, direction, Color.red, 0.6f);
        //Physics2D.Raycast(transform.position, direction, 1f);

        //print(hit.collider);
        if (Physics2D.Raycast(transform.position, direction, 0.6f))
        {
            //print(true);
            return true;
        }
        //print(false);
        return false;
    }


    private IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;
        float elapsedTime = 0;
        facing = direction; 

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameOver)
            return;
        //print("Hit something");
        //print(other.gameObject.tag);
        if (other.gameObject.CompareTag("Pellet") && pelletMap != null)
        {
            //print(pelletMap.WorldToCell(transform.position));
            //need to find the positon of the tile and turn it off
            pelletMap.SetTile(pelletMap.WorldToCell(transform.position), null);
            pelletsLeft -= 1;
            //print(pelletsLeft);
            AddScore(10);
            if (pelletsLeft <= 0)
            {
                winText.text = "You Win!";
                gameOver = true;
            }

        }else if(other.gameObject.CompareTag("Power Up"))
        {
            AddScore(200);
            Destroy(other.gameObject);
            
            foreach(GameObject ghost in ghosts)
            {
                ghostScript.VulnerableState();
            }
        }else if (other.gameObject.CompareTag("Enemy"))
        {
            if (ghostScript.Vulnerable == false)
            {
                winText.text = "You Lose";
                gameOver = true;
            } else
            {
                AddScore(500);
                other.gameObject.GetComponent<EnemyController>().Eaten();
            }
        }
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
    void AddScore(int scoreChange)
    {
        score += scoreChange;
        scoreText.text = "Score\n" + score;
    }

}
