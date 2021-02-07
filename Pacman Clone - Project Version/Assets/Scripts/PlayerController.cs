using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    public Text winText;
    public Text scoreText;
    public Text levelText;
    public Text livesText;
    public static int level = 1;
    private static int score = 0;
    private static int livesScore;
    public static int lives = 3;
    public float timeToMove;
    public Tilemap pelletMap;
    private Vector3 oldPos;
    private Vector3 targetPos;
    public Vector3 facing;
    public GameObject Blinky;
    public GameObject Pinky;
    public GameObject Inky;
    public GameObject Clyde;
    public int pelletsLeft;
    
    private bool isMoving;
    private bool gameOver = false;

    private EnemyController ghostScript;

    void Awake()
    {
        BoundsInt bounds = pelletMap.cellBounds; //count up all the pellets on the map
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = pelletMap.GetTile<Tile>(pos);
            if (tile != null)
                pelletsLeft += 1;
        }
        print (pelletsLeft);

        winText.text = "";
        levelText.text = "Level\n"+ level;
        livesText.text = "Lives\n"+ lives;

        AddScore(0);
        
        Blinky = GameObject.Find("Blinky Car");
        Pinky = GameObject.Find("Pinky Car");
        Inky = GameObject.Find("Inky Car");
        Clyde = GameObject.Find("Clyde Car");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.x) > 10)
            TeleportFlip();
        
        if (!gameOver)
        {
            if (Input.GetKey(KeyCode.W) && !isMoving && RaycastCheck(Vector3.up) == false)
            {
                StartCoroutine(MovePlayer(Vector3.up));
                transform.rotation = Quaternion.Euler(0,0,90);
            }

            if (Input.GetKey(KeyCode.A) && !isMoving && RaycastCheck(Vector3.left) == false)
            {
                StartCoroutine(MovePlayer(Vector3.left));
                transform.rotation = Quaternion.Euler(0,0,180);
            }

            if (Input.GetKey(KeyCode.S) && !isMoving && RaycastCheck(Vector3.down) == false)
            {
                StartCoroutine(MovePlayer(Vector3.down));
                transform.rotation = Quaternion.Euler(0,0,270);
            }

            if (Input.GetKey(KeyCode.D) && !isMoving && RaycastCheck(Vector3.right) == false)
            {
                StartCoroutine(MovePlayer(Vector3.right));
                transform.rotation = Quaternion.Euler(0,0,0);
            }
        } else //gameOver is true
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (lives > 0)
                {
                    ResetLevel();
                }
            }
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
            print(pelletMap.WorldToCell(targetPos));
            //need to find the positon of the tile and turn it off
            pelletMap.SetTile(pelletMap.WorldToCell(targetPos), null);
            pelletsLeft -= 1;
            print(pelletsLeft);
            AddScore(10);
            if (pelletsLeft <= 0)
            {
                winText.text = "You Beat Level " + level + "!";
                gameOver = true;
                level += 1;
                StartCoroutine(Pause());
                SceneManager.LoadScene("Level A");
            }

        }else if(other.gameObject.CompareTag("Power Up"))
        {
            AddScore(200);
            Destroy(other.gameObject);
            
            Blinky.GetComponent<EnemyController>().VulnerableState();
            Pinky.GetComponent<EnemyController>().VulnerableState();
            Inky.GetComponent<EnemyController>().VulnerableState();
            Clyde.GetComponent<EnemyController>().VulnerableState();
            
        }else if (other.gameObject.CompareTag("Enemy"))
        {
            if (other.gameObject.GetComponent<EnemyController>().Vulnerable == false)
            {
                AddLives(-1);
            } else
            {
                AddScore(500);
                other.gameObject.GetComponent<EnemyController>().Eaten();
            }
        }
    }
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(3);
    }
    void ResetLevel()
    {
        //reset pacman and ghosts to starting positions and wait for input
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
        livesScore += scoreChange;
        scoreText.text = "Score\n" + score;
        if (livesScore > 10000)
        {
            AddLives(1);
            livesScore -= 10000;
        }
    }
    void AddLives(int livesChange)
    {
        lives += livesChange;
        livesText.text = "Lives\n" + lives;
        if (lives < 1)
        {
            gameOver = true;
            winText.text = "You Lose\nPress Space to Continue";
        }
        
    }

}
