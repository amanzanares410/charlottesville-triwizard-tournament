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
    [SerializeField] private Animator animator;
    [SerializeField] private float scoreMultiplier = 10f;

    [SerializeField] private float playerSpeed;
    private float gravity;
    private Vector3 movementDirection = Vector3.forward;
    private Vector3 playerVelocity;

    private PlayerInput playerInput;
    private InputAction jumpAction;
    private InputAction slideAction;
    private bool sliding = false;
    private bool hasTurned = false;
    private CharacterController controller;

    private float score = 0;

    private int slidingAnimationId;

    [SerializeField] private UnityEvent<Vector3> turnEvent;

    [SerializeField] private UnityEvent<int> gameOverEvent;
    [SerializeField] private UnityEvent<int> scoreUpdateEvent;

        [SerializeField] private float laneWidth = 1f;
        [SerializeField] private int numLanes = 3;
        private int currentLane = 1;
        private Vector3 targetLane;
        private InputAction moveAction;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
        slidingAnimationId = Animator.StringToHash("Sliding");
        jumpAction = playerInput.actions["Jump"];
        slideAction = playerInput.actions["Slide"];
            moveAction = playerInput.actions["Move"];

    }

    // Listen for player input
    private void OnEnable() {
        slideAction.performed += PlayerSlide;
        jumpAction.performed += PlayerJump;
            moveAction.performed += PlayerMove;
    }

    // Stop listening for player input
    private void OnDisable() {
        slideAction.performed -= PlayerSlide;
        jumpAction.performed -= PlayerJump;
            moveAction.performed -= PlayerMove;
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
    }

        private (float?, Vector3?) CheckTurn()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if (type == TileType.LEFT)
                {
                    return (-1, tile.pivot.position);
                }
                else if (type == TileType.RIGHT)
                {
                    return (1, tile.pivot.position);
                }
                else if (type == TileType.SIDEWAYS)
                {
                    return (1, tile.pivot.position);
                }
            }
            return (null, null);

        }

       private void Turn(float turnValue, Vector3 turnPosition) {

        Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
        controller.enabled = false;
        transform.position = tempPlayerPosition;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
        transform.rotation = targetRotation;    
        movementDirection = (transform.rotation * Vector3.forward).normalized;
        Vector3 lateralOffset = (currentLane - 1) * laneWidth * Vector3.Cross(movementDirection, Vector3.up);
        targetLane = transform.position + lateralOffset;
        controller.enabled = true;
        controller.Move(Vector3.zero);
        playerSpeed = 20;
        }

        private void ResetTurnLock()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f, turnLayer);
            if (hitColliders.Length == 0)
            {
                hasTurned = false;
            }
        }

        private void PlayerMove(InputAction.CallbackContext context) {
            float input = context.ReadValue<float>();

            Debug.Log($"Move Input: {context.ReadValue<float>()}");

            if (input < 0 && currentLane > 0)
            {
                currentLane--;
            }
            else if (input > 0 && currentLane < numLanes - 1)
            {
                currentLane++;
            }
            targetLane = new Vector3((currentLane - 1) * laneWidth, transform.position.y, transform.position.z);
           

        }
    private void PlayerSlide(InputAction.CallbackContext context) {
        if (!sliding && IsGrounded()) {
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

    private void PlayerJump(InputAction.CallbackContext context) {

        if(IsGrounded()){
            playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
            controller.Move(playerVelocity * Time.deltaTime);
        }
        
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

        controller.Move(movementDirection * playerSpeed * Time.deltaTime);

        Vector3 lateralMovement = (targetLane - transform.position).normalized;
        controller.Move(new Vector3(lateralMovement.x, 0, 0) * Time.deltaTime * 10f);


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

        (float? turnDirection, Vector3? turnPosition) = CheckTurn();
        if (turnDirection.HasValue && turnPosition.HasValue && !hasTurned)
        {
            hasTurned = true;
            float direction = turnDirection.Value;
            Vector3 targetDirection = Quaternion.AngleAxis(90 * direction, Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(direction, turnPosition.Value);
        }

        ResetTurnLock();
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

