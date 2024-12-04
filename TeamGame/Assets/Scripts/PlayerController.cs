using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace TempleRun.Player {
[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterSelectionData selectionData;
    //[SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private float initialPlayerSpeed = 4f;
    [SerializeField] private float maximumPlayerSpeed = 25f;
    [SerializeField] private float playerSpeedIncrease = .05f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float initialGravityValue = -9.81f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask turnLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private AnimationClip slideAnimationClip;
    [SerializeField] private AnimationClip runAnimationClip;
    [SerializeField] private Animator animator;
    [SerializeField] private float scoreMultiplier = 10f;

    [SerializeField] private AudioSource runningAudio;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip jumpAudio;
    [SerializeField] private AudioClip slideAudio;
    [SerializeField] private float playerSpeed;
    private float gravity;
    private Vector3 movementDirection = Vector3.forward;
    private Vector3 playerVelocity;

    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction slideAction;
    private InputAction turnAction;
    private bool sliding = false;
    private CharacterController controller;

    private float score = 0;

    private int slidingAnimationId;

    [SerializeField] private UnityEvent<Vector3> turnEvent;

    [SerializeField] private UnityEvent<int> gameOverEvent;
    [SerializeField] private UnityEvent<int> scoreUpdateEvent;


    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        slidingAnimationId = Animator.StringToHash("Sliding");
        turnAction = playerInput.actions["Turn"];
        jumpAction = playerInput.actions["Jump"];
        slideAction = playerInput.actions["Slide"];

    }

    // Listen for player input
    private void OnEnable() {
        slideAction.performed += PlayerSlide;
        jumpAction.performed += PlayerJump;
        turnAction.performed += PlayerTurn;
    }

    // Stop listening for player input
    private void OnDisable() {
        slideAction.performed -= PlayerSlide;
        jumpAction.performed -= PlayerJump;
        turnAction.performed -= PlayerTurn;
    }

    private void Start() {
        playerSpeed = initialPlayerSpeed;
        gravity = initialGravityValue;
        //if (selectionData.selectedCharacterSprite != null)
        if (true)
        {
            //characterRenderer.sprite = selectionData.selectedCharacterSprite;
            Debug.Log(selectionData.selectedCharacterName);
        }

        if (runningAudio != null)
        {
            runningAudio.loop = true;
            runningAudio.Play();
        }
    }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {

                GameOver();
                return;
            }
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition.Value);

        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1) ||
                    (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;

        }

       private void Turn(float turnValue, Vector3 turnPosition) 
       {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            controller.enabled = false;
            transform.position = tempPlayerPosition;
            controller.enabled = true;

            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            movementDirection = transform.forward.normalized;
       }

    private void PlayerSlide(InputAction.CallbackContext context) {
        if (!sliding && IsGrounded()) {
            PlaySound(slideAudio);
            StartCoroutine(Slide());
        }
    }
    
    private IEnumerator Slide() {
        sliding = true;
        Vector3 originalControllerCenter = controller.center;
        Vector3 newControllerCenter = originalControllerCenter;

        controller.height /= 2;
        newControllerCenter.y -= controller.height / 2;
        controller.center = newControllerCenter;
        // Play the sliding animation
        animator.Play("slidingAnimation", 0);
        yield return new WaitForSeconds(slideAnimationClip.length / animator.speed);
        // Set the character controller collider back to normal after sliding.
        controller.height *= 2;
        controller.center = originalControllerCenter;
        sliding = false;
    }

private void PlayerJump(InputAction.CallbackContext context)
{
   if(IsGrounded()){
        PlaySound(jumpAudio);
        playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -2f);
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
}

    

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private void Update() {
        if (!controller.enabled)
        {
        Debug.LogWarning("CharacterController was disabled, enabling now.");
        controller.enabled = true;
        }


        if (!IsGrounded(20f))
        {
            GameOver();
            return;
        }

        //Update
      
        score += scoreMultiplier * Time.deltaTime;
        scoreUpdateEvent.Invoke((int)score);

        controller.Move(transform.forward * playerSpeed * Time.deltaTime);


        if (IsGrounded() && playerVelocity.y < 0){
        playerVelocity.y = 0f;
            
        }

        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (playerSpeed < maximumPlayerSpeed)
        {
            playerSpeed += Time.deltaTime * playerSpeedIncrease;
            gravity = Mathf.Clamp(initialGravityValue - playerSpeed, -50f, -9.81f);

            if (animator.speed < 1.25f)
            {
                animator.speed = Mathf.Lerp(animator.speed, 1 + (playerSpeed / maximumPlayerSpeed), Time.deltaTime);

            }
        }
    }

    private bool IsGrounded(float length = .2f){
        Vector3 raycastOriginFirst = transform.position;
        raycastOriginFirst.y -= controller.height / 2f;
        raycastOriginFirst.y += 0.1f;


        Vector3 raycastOriginSecond = raycastOriginFirst;
        raycastOriginFirst -= transform.forward * .2f;
        raycastOriginSecond += transform.forward * .2f;

        
        
        if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) || 
        Physics.Raycast(raycastOriginSecond, Vector3.down, out RaycastHit hit2, length, groundLayer)) {
            return true;
        }

        return false;

    }

        private void GameOver()
        {
            Debug.Log("Game Over");
            gameOverEvent.Invoke((int)score);
            gameObject.SetActive(false);

            if (runningAudio != null)
            {
                runningAudio.Stop();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                GameOver();
            }
        }
    }
}

