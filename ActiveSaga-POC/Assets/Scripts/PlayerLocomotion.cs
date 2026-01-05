using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer; 
    
    [Header("Path Settings")]
    [Tooltip("Drag an object here to define the forward direction.")]
    [SerializeField] private Transform pathDirectionReference; 

    [Header("Movement Settings")]
    [SerializeField] private float maxRunSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 2.0f; 
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Momentum Settings")]
    [SerializeField] private float momentumDecayRate = 2.0f; 

    [Header("Momentum Filtering")]
    [Tooltip("Minimum input intensity to be considered 'Running'. Prevents drift when standing still.")]
    [SerializeField] private float minRunInputThreshold = 0.15f; 

    [Tooltip("Time in seconds you must run continuously before momentum is 'banked'.")]
    [SerializeField] private float minRunDurationToBuildMomentum = 0.5f;

    [Tooltip("Time in seconds to remember the last valid running speed.")]
    [SerializeField] private float jumpMomentumMemoryWindow = 0.5f;

    [Header("Air Control")]
    [Tooltip("Multiplier for speed while in air.")]
    [SerializeField] private float airSpeedFactor = 1.0f; 

    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    
    private float currentMomentumSpeed = 0f;

    // MEMORY VARIABLES
    private float storedRunSpeed = 0f;
    private float lastRunInputTime = -10f; 
    private float currentRunDuration = 0f; 

    // Lock flag for stationary jumps
    private bool isStationaryJumpLock = false;

    private void OnEnable()
    {
        if (jumpAnalyzer != null) jumpAnalyzer.OnJumpDetected += PerformJump;
    }

    private void OnDisable()
    {
        if (jumpAnalyzer != null) jumpAnalyzer.OnJumpDetected -= PerformJump;
    }

    void Update()
    {
        if (runAnalyzer == null || playerTransform == null || pathDirectionReference == null) return;
        // --- 1. Calculate Horizontal Speed (Momentum) ---
        if (isGrounded)
        {
            // Reset the jump lock when we are on the ground
            isStationaryJumpLock = false;

            float runIntensity = runAnalyzer.CurrentRunFactor;
            float targetSpeed = 0f;

            // --- בדיקת סקוואט ---
            // בודקים האם ה-SquatAnalyzer קיים והאם הוא מדווח שאנחנו בסקוואט
            bool isSquatting = (squatAnalyzer != null && squatAnalyzer.IsSquatting);

            if (isSquatting)
            {
                // לוגיקה של סקוואט:
                // אם השחקן מנסה לזוז (מנופף בידיים) בזמן סקוואט, ניתן לו לזוז אבל לאט.
                if (runIntensity > minRunInputThreshold)
                {
                    targetSpeed = crouchSpeed; // מהירות מוגבלת
                }
            }
            else
            {
                // לוגיקה של ריצה רגילה (כמו קודם):
                if (runIntensity > minRunInputThreshold) 
                {
                    targetSpeed = maxRunSpeed * runIntensity;
                    currentRunDuration += Time.deltaTime;

                    if (currentRunDuration >= minRunDurationToBuildMomentum)
                    {
                        storedRunSpeed = targetSpeed;
                        lastRunInputTime = Time.time;
                    }
                }
                else
                {
                    currentRunDuration = 0f;
                    targetSpeed = 0f;
                }
            }

            // החלקת מהירות (Smoothing)
            if (targetSpeed > currentMomentumSpeed)
            {
                currentMomentumSpeed = targetSpeed; 
            }
            else
            {
                currentMomentumSpeed = Mathf.Lerp(currentMomentumSpeed, targetSpeed, Time.deltaTime * momentumDecayRate);
                if (currentMomentumSpeed < 0.01f) currentMomentumSpeed = 0f;
            }
        }

        // --- 2. Determine Actual Move Speed ---
        float actualMoveSpeed = currentMomentumSpeed;
        
        if (!isGrounded)
        {
            // CRITICAL FIX: If we are locked into a stationary jump, FORCE speed to 0.
            if (isStationaryJumpLock)
            {
                actualMoveSpeed = 0f;
            }
            else
            {
                actualMoveSpeed *= airSpeedFactor;
            }
        }

        // Apply Horizontal Movement
        Vector3 horizontalMove = Vector3.zero;
        if (actualMoveSpeed > 0.001f)
        {
            Vector3 forwardDir = pathDirectionReference.forward;
            forwardDir.y = 0; 
            forwardDir.Normalize();
            horizontalMove = forwardDir * actualMoveSpeed * Time.deltaTime;
        }

        // --- 3. Vertical & Gravity ---
        ApplyGravity();
        Vector3 verticalMove = Vector3.up * verticalVelocity * Time.deltaTime;

        // --- 4. Apply Total Movement ---
        playerTransform.Translate(horizontalMove + verticalMove, Space.World);

        // --- 5. Ground Check ---
        if (playerTransform.position.y <= 0)
        {
            Vector3 pos = playerTransform.position;
            pos.y = 0;
            playerTransform.position = pos;
            
            isGrounded = true;
            verticalVelocity = 0;
        }
    }

    private void PerformJump()
    {
        // חסימה: אי אפשר לקפוץ אם נמצאים כרגע בסקוואט עמוק
        if (squatAnalyzer != null && squatAnalyzer.IsSquatting)
        {
            Debug.Log("Jump blocked because player is squatting.");
            return;
        }

        if (isGrounded)
        {
            float timeSinceLastValidRun = Time.time - lastRunInputTime;

            // CHECK: Did we have a valid run recently?
            bool isValidRunJump = (timeSinceLastValidRun <= jumpMomentumMemoryWindow);

            if (isValidRunJump)
            {
                // RUNNING JUMP
                currentMomentumSpeed = Mathf.Max(currentMomentumSpeed, storedRunSpeed);
                isStationaryJumpLock = false; 
                Debug.Log($"Running Jump! Speed: {currentMomentumSpeed}");
            }
            else
            {
                // STANDING JUMP
                currentMomentumSpeed = 0f;
                storedRunSpeed = 0f;
                isStationaryJumpLock = true; 
                Debug.Log("Standing Jump: MOVEMENT LOCKED");
            }

            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded || playerTransform.position.y > 0)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }
}