using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
public class PlayerController : MonoBehaviour
{
    public Text winText;
    public Image backdrop;
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
    private GameObject Blinky;
    private GameObject Pinky;
    private GameObject Inky;
    private GameObject Clyde;
    public int pelletsLeft;
    public GameObject boxSpawn;
    
    private bool isMoving;
    private bool gameOver = false;
    private bool isPaused = false;
    float elapsedTime = 0;//used for moving

    private EnemyController ghostScript;
    private AudioSource audioSource;

    public AudioClip pelletNom;
    public AudioClip loseMusic;
    public AudioClip winMusic;
    private Animator anim;

    private bool cheatMode;

    void Awake()
    {   
        audioSource = GetComponent<AudioSource>();
        
        levelText.text = "" + level;
        AddScore(-10);
        if (SceneManager.GetActiveScene().name == "Win Screen")
        {
            gameOver = true;
            pelletsLeft = 1;
            lives = 3;
            audioSource.clip = winMusic;
            audioSource.Play();
            return;
        }

        livesText.text = "Lives";
        winText.text = "";
        backdrop.enabled = false;
        BoundsInt bounds = pelletMap.cellBounds; //count up all the pellets on the map
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = pelletMap.GetTile<Tile>(pos);
            if (tile != null)
                pelletsLeft += 1;
        }
        print(pelletsLeft);
        
        
        anim = GetComponent<Animator>();

        Blinky = GameObject.Find("Blinky Car");
        Pinky = GameObject.Find("Pinky Car");
        Inky = GameObject.Find("Inky Car");
        Clyde = GameObject.Find("Clyde Car");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            cheatMode = !cheatMode;
            if (cheatMode)
                timeToMove = 0.1f;
            else
                timeToMove = 0.25f;
        }
        if (Input.GetKeyDown(KeyCode.P))
            SceneManager.LoadScene("Level B");
        if (Input.GetKeyDown(KeyCode.O))
            SceneManager.LoadScene("Level A");
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (SceneManager.GetActiveScene().name == "Win Screen")
            {
                lives = 3;
                score = 0;
                level = 1;
                SceneManager.LoadScene("MainMenu");
            }
            
            if (!gameOver)    
            {
                isPaused = !isPaused;
                print("Pause = " + isPaused);
                if (isPaused)
                {
                    winText.text = "Paused";
                    backdrop.enabled = true;
                    Time.timeScale = 0;
                } else
                {
                    winText.text = "";
                    backdrop.enabled = false;
                    Time.timeScale = 1;
                }
            }else if (pelletsLeft == 0)
            {
                Time.timeScale = 1;
                Scene scene = SceneManager.GetActiveScene();
                if (scene.name == "Level B")
                {
                    SceneManager.LoadScene("Level A");
                } else if (scene.name == "Level A")
                {
                    SceneManager.LoadScene("Level B");
                }
            }else if (lives > 0) //gameOver
            {
                anim.SetBool("Dead",false);
                Time.timeScale = 1;
                winText.text = "";
                backdrop.enabled = false;

                Blinky.GetComponent<EnemyController>().ResetPos();
                Pinky.GetComponent<EnemyController>().ResetPos();
                Inky.GetComponent<EnemyController>().ResetPos();
                Clyde.GetComponent<EnemyController>().ResetPos();
                
                transform.position = Vector3.zero;
                audioSource.Stop();
                gameOver = false;
            } else if (SceneManager.GetActiveScene().name != "Win Screen")
            {
                Time.timeScale = 1;
                SceneManager.LoadScene("Win Screen");
            }
        }
    }
    
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
        int layerMask = LayerMask.GetMask("Walls");
        //print(hit.collider);
        if (Physics2D.Raycast(transform.position, direction, 0.6f, layerMask))
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
        elapsedTime = 0;
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
            //print(pelletMap.WorldToCell(targetPos));
            //need to find the positon of the tile and turn it off
            pelletMap.SetTile(pelletMap.WorldToCell(targetPos), null);
            pelletsLeft -= 1;
            //print(pelletsLeft);
            AddScore(10);
            audioSource.clip = pelletNom;
            audioSource.Play();

            if (pelletsLeft <= 0)
            {
                winText.text = "You Beat Level " + level + "!\nPress Space to Continue";

                audioSource.clip = winMusic;
                audioSource.Play();

                backdrop.enabled = true;
                gameOver = true;
                elapsedTime = 10000;
                level += 1;
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
                anim.SetBool("Dead",true);
                Blinky.GetComponent<EnemyController>().ResetPos();
                Pinky.GetComponent<EnemyController>().ResetPos();
                Inky.GetComponent<EnemyController>().ResetPos();
                Clyde.GetComponent<EnemyController>().ResetPos();
                elapsedTime = 10000;
                AddLives(-1);
            } else
            {
                AddScore(500);
                other.gameObject.GetComponent<EnemyController>().Eaten();
            }
        }
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
        scoreText.text = "" + score;
        if (livesScore > 5000)
        {
            AddLives(1);
            livesScore -= 5000;
        }
    }
    void AddLives(int livesChange)
    {
        if (livesChange > 0 && lives > 4) //should stop the player from getting more than 5 lives
        {
            return;
        }else
        {
            lives += livesChange;
        }
        
        //livesText.text = "Lives\n" + lives;
        string lifecount;
        GameObject obj;
        if (livesChange > 0)
        {
            lifecount = "Life " + lives;
            obj = GameObject.Find(lifecount);
            obj.GetComponent<Image>().enabled = true;
        } else //lose a life
        {
            lifecount = "Life " + (lives+1);
            obj = GameObject.Find(lifecount);
            obj.GetComponent<Image>().enabled = false;
        }

        if (lives < 1)
        {
            audioSource.clip = loseMusic;
            audioSource.Play();

            Time.timeScale = 0;
            gameOver = true;
            winText.text = "Game Over\nPress Space to End";
            backdrop.enabled = true;
        } else if (livesChange < 0)
        {
            audioSource.clip = loseMusic;
            audioSource.Play();

            Time.timeScale = 0;
            gameOver = true;
            winText.text = "You got Caught\nPress Space to Continue";
            backdrop.enabled = true;
        }
        
    }

}
