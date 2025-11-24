using System.Collections;
using System.Collections.Generic;
using FishNet.Demo.AdditiveScenes;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private NetworkObject ball;
    private Rigidbody2D rb;
    private SpriteRenderer pointDirection;
    private Camera playerCamera;
    private Vector3 _direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            playerCamera = Camera.main;
            pointDirection = GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            gameObject.GetComponent<PlayerController>().enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        ProcessMovement();
        MovePointer();
        ShootBall();
    }

    // TODO:
    // Change contoller to bias most recent key pressed
    /// <summary>
    /// Processes input and movement for the player.
    /// </summary>
    private void ProcessMovement()
    {
        Vector3 inputVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) inputVector += Vector3.up;
        else if (Input.GetKey(KeyCode.S)) inputVector += Vector3.down;
        if (Input.GetKey(KeyCode.A)) inputVector += Vector3.left;
        else if (Input.GetKey(KeyCode.D)) inputVector += Vector3.right;

        inputVector.Normalize();

        MovePlayer(inputVector);
    }

    private void MovePlayer(Vector3 moveDirection)
    {
        this.rb.velocity = moveDirection * speed;
    }

    /// <summary>
    /// Points the pointDirection sprite towards the mouse while rotating around the player sprite.
    /// </summary>
    private void MovePointer()
    {
        // get the direction of the mouse relative to player object
        _direction = (playerCamera.ScreenToWorldPoint(Input.mousePosition) - this.transform.position).normalized;
        // get the angle of the mouse direction relative to the player object
        float angle = Mathf.Atan2(_direction.y, _direction.x);
        // set pointer position on circle radius around player object corresponding to angle
        pointDirection.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        // rotate the pointer to point to the mouse
        pointDirection.transform.rotation = Quaternion.Euler(0, 0, (angle * Mathf.Rad2Deg) + 45);
    }

    private void ShootBall()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            SpawnBall();
        }
    }

    [ServerRpc]
    private void SpawnBall()
    {
        NetworkObject ballObj = Instantiate(ball, transform.position, Quaternion.identity);
        Spawn(ballObj);
    }
}
