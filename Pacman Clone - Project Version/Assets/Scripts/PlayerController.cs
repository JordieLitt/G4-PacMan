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
    
    private bool isMoving;
    private bool gameOver = false;

    void Start()
    {
        winText.text = "";
        AddScore(0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
        //print("Hit something");
        //print(other.gameObject.tag);
        if (other.gameObject.CompareTag("Pellet") && pelletMap != null)
        {
            //Vector3 hitPos = Vector3.zero;
            print(pelletMap.WorldToCell(transform.position));
            //need to find the positon of the tile and turn it off
            pelletMap.SetTile(pelletMap.WorldToCell(transform.position), null);

            AddScore(10);
        }else if(other.gameObject.CompareTag("Power Up"))
        {
            AddScore(200);
        }else if (other.gameObject.CompareTag("Enemy"))
        {
            winText.text = "You Lose";
            gameOver = true;
        }
    }
    void AddScore(int scoreChange)
    {
        score += scoreChange;
        scoreText.text = "Score\n" + score;
    }

}
