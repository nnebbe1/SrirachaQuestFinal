using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    // Resources from other classes and scripts

    [SerializeField]
    private SoundManager _soundManager;

    [SerializeField]
    private GameManager _gameManager;
    private Animator _animator;

    // Variables of the player

    [SerializeField]
    private float _speed = 600f;
    [SerializeField]
    private float _jumpingSpeed;
    [SerializeField]
    private float _extraGravity;
    [SerializeField]
    private float _coolDownTime = 2f;
    [SerializeField]
    private float _nextJumpTime = 0f;
    [SerializeField]
    private Transform _positionPlayer;
    private Rigidbody _playerRigidbody;
    private bool _hasUmbrella;
    private int _lives = 3;

    private Vector3 _startPosition = new Vector3(-257.9f, 43.7f, -13.2f);
    

    //Variables used for the camera movement

    [SerializeField]
    private Camera _cam;

    [SerializeField]
    private Transform _positionCam;

    // Siracha sauce
    [SerializeField]
    private GameObject _sauce;


    // Player Set up
    void Start()
    {
        transform.position = _startPosition;
        _playerRigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _hasUmbrella = false;
    }

    // Handels moves, airresistance jumps and animations of the player according to input
    void FixedUpdate()
    {   
        // Calculate movement directions and movement angel based on current position
        Vector3 movement = (_positionPlayer.position - _positionCam.position) / 30;
        movement.y = 0;
        Vector3 movementRight = new Vector3(-movement.z, 0f, movement.x);
        Vector3 movementLeft = new Vector3(movement.z, 0f, -movement.x);

        float targetAngel = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

        // Adds "extra" gravity to make the game less floaty and more precise
        _playerRigidbody.AddForce(0, -_extraGravity, 0);

        // Forward and backward movement
        if (Input.GetAxisRaw("Vertical") > 0)
        {   
            
            _playerRigidbody.AddForce((movement * _speed * Time.deltaTime) * 50);
            transform.rotation = Quaternion.Euler(0f, targetAngel, 0f);
        }

        if (Input.GetAxisRaw("Vertical") < 0)
        {   
            _playerRigidbody.AddForce((-1) * (movement * _speed * Time.deltaTime) * 50);
        }

        // Left and Right movement
        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            _playerRigidbody.AddForce((movementRight * _speed * Time.deltaTime) * 50);
        }

        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            _playerRigidbody.AddForce((movementLeft * _speed * Time.deltaTime) * 50);

        }
        // Jumping and jump animation
        if (Input.GetKey("space") && _nextJumpTime < Time.time)
        {
            _playerRigidbody.velocity = new Vector3(0f, _jumpingSpeed, 0f);
            _nextJumpTime = Time.time + _coolDownTime;
            _animator.SetBool("toJump", true);
            _soundManager.playSound("jump");
        }
        if (_nextJumpTime < Time.time)
        {
            _animator.SetBool("toJump", false);
        }

        // Simulate air resistance, slowing down the player
        var direction = -_playerRigidbody.velocity.normalized;
        var forceAmount = (_playerRigidbody.velocity.magnitude);
        var airResistance = new Vector3(direction.x * forceAmount, 0f, direction.z * forceAmount);
        _playerRigidbody.AddForce(airResistance);

        // If the player manages to go out of the playing area they will respawn at the beginning
        if (_positionPlayer.position.y < -100){
           _positionPlayer.position = _startPosition;
        }


    }


    // Interactions between player an interactables when collision occurs
    // Onion, ingrediences, umbrella, rain and ketchup
    // Show UI prompts, loose health, play sounds
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("onion"))
        {
            _gameManager.show_onion_prompt();

        }
        // player gets killed by mayo-rain without umbrella
        if (other.CompareTag("rain"))
        {
            _soundManager.playSound("rain");
            
            if (_hasUmbrella == false)
            {
                this.Damage(0);
            }

        }
        // Player gets 1 damage and the ketchup object gets destroyed
        if (other.CompareTag("ketchup"))
        {
            this.Damage(1);
            _soundManager.playSound("damage");
            StartCoroutine(_gameManager.k_splash());
            Destroy(other.gameObject);

        }
        else if (other.CompareTag("tomato") || other.CompareTag("garlic") || other.CompareTag("mushroom") || other.CompareTag("yogurt") || other.CompareTag("pot") || other.CompareTag("umbrella"))
        {
            _gameManager.show_press_e_prompt();
        }

    }

    // Interactions between player an interactables during collision
    // Pick up of ingrediences, talk to onion, cook at cooking pot
    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("onion"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.onionTalk();
            }

        }
        if (other.CompareTag("garlic"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.pickup_item(other.tag);
                _soundManager.playSound("collect");
            }
        }

        if (other.CompareTag("mushroom"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.pickup_item(other.tag);
                _soundManager.playSound("collect");
            }
        }

        if (other.CompareTag("yogurt"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.pickup_item(other.tag);
                _soundManager.playSound("collect");
            }
        }

        if (other.CompareTag("tomato"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.pickup_item(other.tag);
                _soundManager.playSound("collect");
            }
        }

        if (other.CompareTag("umbrella"))
        {
            if (Input.GetKey(KeyCode.E))
            {
                _gameManager.pickup_item(other.tag);
                _soundManager.playSound("collect");
                _hasUmbrella = true;
            }
        }

        if (other.CompareTag("pot"))
        {

            if (Input.GetKey(KeyCode.E))   
            {
                if (_gameManager.GetIngrediants())
                {
                    // When the game is won the bird spins and the sauce appears in the pot
                    _animator.SetBool("isWon", true);
                    _sauce.GetComponent<MeshRenderer>().enabled = true;
                    StartCoroutine(_gameManager.winGame());
                    

                }
                else
                {
                    Debug.Log("not all ingredients there yet");
                    _gameManager.not_finished_prompt();
                }
            }
        }


    }

    // Interactions between player an interactables when collision ends
    // Hide UI prompts
    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("onion"))
        {
            _gameManager.hide_onion_prompt();
        }
        else if (other.CompareTag("tomato") || other.CompareTag("garlic") || other.CompareTag("mushroom") || other.CompareTag("yogurt") || other.CompareTag("pot") || other.CompareTag("umbrella"))
        {
            _gameManager.hide_press_e_prompt();

        }
        else if (other.CompareTag("rain"))
        {
            _soundManager.playSound("background");

        }

    }

    // Function gets called with the enemy type when collision with enemy occures
    // Handels the life-loosing
    public void Damage(int type_enemy)
    {
        // Mayo-rain leads to death right away
        if(type_enemy == 0)
        {
            _lives = 0;
            StartCoroutine(_gameManager.looseHealth(_lives));
        }
        // Ketchup uses lives consecutively 
        else if (type_enemy == 1)
        {
            _lives--;
            Debug.Log(_lives);
            _animator.SetInteger("isDamaged", _lives);
            StartCoroutine(_gameManager.looseHealth(_lives));
        }

        if (_lives == 0)
        {
            _animator.SetBool("isDead", true);
            _soundManager.playSound("gameover");
            StartCoroutine(_gameManager.looseHealth(_lives));

            

        }

    }
}