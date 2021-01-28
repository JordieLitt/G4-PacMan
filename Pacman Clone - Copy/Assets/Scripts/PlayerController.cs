using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public Text winText;
    public float timeToMove;
    private Vector3 oldPos;
    private Vector3 targetPos;
    public Vector3 facing;
    
    private bool isMoving;

    void Start()
    {
        winText.text = "";
    }

    // Update is called once per frame
    void FixedUpdate()
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
    private bool RaycastCheck(Vector3 direction)
    {
        Debug.DrawRay(transform.position, direction, Color.red, 1f);
        Physics2D.Raycast(transform.position, direction, 1f);

        //print(hit.collider);
        if (Physics2D.Raycast(transform.position, direction, 1f))
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
        if (other.gameObject.CompareTag("Enemy"))
        {
            winText.text = "You Lose";
            isMoving = true;
        }
    }

}
