public class TestCC2 : MonoBehaviour {

    private Animator playerAnimator;
    private CharacterController controller;

    private Vector3 camForward;

    private float Speed = 0.0f;
    //private float direction = 0f;

    private Vector3 moveDirection;

    public float playerSpeed;
    public bool isMoving;

    public bool Pushed;
    public float pushCooldown = 1.0f;

    public CameraControl gamecam;

    private int coinCount;
    public Text CoinCountText;

    public float sPwrTime;
    private float gravity;

    private bool rollButton;
    //public int sPwrTimeCD;
    //public bool hitSPwr;

    public bool attackButton;
    public bool attackButtonUp;

    public float distToGround;

    public bool grounded;
    private bool lastGrounded;

    public Renderer rodR;
    public Renderer characterR;
    public Renderer shieldR;

    public bool tooSteep = false;

    public class playerHurt
    {
        public bool hurtAble = true;

        public bool Hurt = false;

        public float hurtTime = 0f;

        public float lastHurtTime = -55f;

        public float hurtCoolDown = 1.6f;

        public bool hurting = false;

        public int health = 3;
    }

    public playerHurt hurtClass = new playerHurt();

    public class playerRoll
    {
        // Can Player Roll?
        public bool rollAble = true;

        // Are we rolling?
        public bool Rolled = false;

        [System.NonSerialized]
        public float rollCoolDown = 0.75f;

        [System.NonSerialized]
        public float rollTime = 0f;

        // Time we jumped (Used to determine for how long to apply extra jump power after jumping.)
        [System.NonSerialized]
        public float lastRollTime = 0.0f;

        [System.NonSerialized]
        public float lastRollButtonDownTime = -50f;

        [System.NonSerialized]
        public float ctrHeight;

        [System.NonSerialized]
        public float ctrCenter;

        [System.NonSerialized]
        public float rollHeight;

        [System.NonSerialized]
        public float rollCenter;
    }

    public playerRoll rollClass = new playerRoll();

    public class playerJump
    {
        // Can Player Roll?
        public bool jumpAble = true;

        // Are we rolling?
        public bool Jumped = false;

        [System.NonSerialized]
        public float jumpCoolDown = 1.0f;

        [System.NonSerialized]
        public float jumpTime = 0f;

        // Time we jumped (Used to determine for how long to apply extra jump power after jumping.)
        [System.NonSerialized]
        public float lastJumpTime = 0.0f;

        [System.NonSerialized]
        public float lastJumpButtonDownTime = -100f;
    }

    public playerJump jumpClass = new playerJump();

    public class playerAttack
    {
        // Can Player attack?
        public bool attackAble = true;

        // Have we already attacked?
        public bool Attacked = false;

        // Held attack button to activate shield
        public bool heldAttackB = false;

        public bool NearCol = false;

        [System.NonSerialized]
        public float attackCoolDown = 0.55f;

        [System.NonSerialized]
        public float attackCoolDownC1 = 0.3f;

        [System.NonSerialized]
        public int attackBCount = 0;

        [System.NonSerialized]
        public bool finishAttack = false;

        [System.NonSerialized]
        public float attackTime = 0f;

        [System.NonSerialized]
        public float lastAttackTime = 0.0f;

        [System.NonSerialized]
        public float lastAttackButtonDownTime = -75f;

        [System.NonSerialized]
        public int attackCombo = -10;

        [System.NonSerialized]
        public int attackSel = 0;

        [System.NonSerialized]
        public bool attackComboed = false;
    }

    public playerAttack attackClass = new playerAttack();

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin-Bronze"))
        {
            other.gameObject.SetActive(false);
            coinCount++;
        }

        else if (other.gameObject.CompareTag("Coin-Gold"))
        {
            other.gameObject.SetActive(false);
            coinCount += 3;
        }

        else if (other.gameObject.CompareTag("Speed Pwr"))
        {
            other.gameObject.SetActive(false);
            sPwrTime = 2.3f;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.normal.x) >= 0.55 || Mathf.Abs(hit.normal.z) >= 0.55)
        {
            tooSteep = true;
        }
        else
        {
            tooSteep = false;
        }

        if (hit.gameObject.CompareTag("Vines") && playerSpeed > 30)
        {
            hurtClass.Hurt = true;
        }

        else if (hit.gameObject.CompareTag("Log"))
        {
            hurtClass.Hurt = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Arch Door") || other.gameObject.CompareTag("Switch1") ||
            other.gameObject.CompareTag("Switch2") || other.gameObject.CompareTag("Switch3"))
        {
            attackClass.NearCol = true;

            if (attackClass.Attacked && attackClass.attackBCount < 15 && attackClass.attackTime <= 0 && !rollClass.Rolled)
            {
                Debug.Log("We opened the switch!");
                other.gameObject.GetComponent<Animator>().SetBool("Open", true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Arch Door") || other.gameObject.CompareTag("Switch1") ||
            other.gameObject.CompareTag("Switch2") || other.gameObject.CompareTag("Switch3"))
        {
            attackClass.NearCol = false;
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, distToGround - 2.5f);
    }

    void Start()
    {
        playerAnimator = gameObject.GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        coinCount = 0;

        Pushed = false;

        rollClass.Rolled = false;
        jumpClass.Jumped = false;
        hurtClass.hurting = false;

        //maxJumpSpeed = 6.0f;

        rollClass.ctrHeight = controller.height;
        rollClass.ctrCenter = controller.center.y;
        rollClass.rollHeight = controller.height * 0.5f;
        rollClass.rollCenter = controller.center.y * 0.5f;

        hurtClass.health = 3;

        gravity = 30.0f;

        SetCoinCount();
    }

    //////////////////////////////////////////////////////////////////////////////
    // Update is called once per frame ///////////////////////////////////////////
    /// //////////////////////////////////////////////////////////////////////////

    void Update()
    {
        distToGround = controller.bounds.extents.y;
        grounded = IsGrounded();
        //Debug.Log(grounded);
    }

    void FixedUpdate()
    {
        hurtManagement();
        moveManagement();
        jumpManagement();

        controller.Move(playerSpeed * moveDirection * Time.deltaTime);

        //Debug.Log(moveDirection);

        SetCoinCount();
        lastGrounded = grounded;
    }

    void hurtManagement()
    {
        if (!hurtClass.Hurt && !hurtClass.hurting)
        {
            //Debug.Log("We are in step 1");
            hurtClass.lastHurtTime = -100;
            hurtClass.hurtAble = true;
        }

        if (hurtClass.Hurt && hurtClass.hurtAble && !hurtClass.hurting )
        {
            //Debug.Log("We are in step 2");
            Debug.Log("Character has been hurt.");
            playerAnimator.SetBool("Hurt", true);
            hurtClass.lastHurtTime = Time.time;
            hurtClass.hurting = true;
            hurtClass.health--;
            //Debug.Log(hurtClass.health);

            if (!attackClass.heldAttackB)
            {
                StartCoroutine(Flasher());
            }
        }

        else if (hurtClass.hurting && hurtClass.hurtAble && hurtClass.lastHurtTime > 0 )
        {
            //Debug.Log("We are in step 3");
            hurtClass.hurtAble = false;
            hurtClass.hurtTime = hurtClass.hurtCoolDown;
        }

        else if ((!hurtClass.hurtAble))
        {
            if (hurtClass.hurtTime > 0)
            {
                hurtClass.hurtTime -= Time.deltaTime;
                if (hurtClass.hurtTime <= 0)
                {
                    Debug.Log("Character has returned to idle state.");
                    playerAnimator.SetBool("Hurt", false);
                    hurtClass.Hurt = false;
                    hurtClass.hurting = false;
                    hurtClass.hurtAble = true;
                }
            }
        }
    }

    void moveManagement()

    void attackManagement()

    public void rollManagement()
    {
        bool rollButton = CrossPlatformInputManager.GetButton("Roll");

        if (!rollButton && !rollClass.Rolled)
        {
            //Debug.Log("We are in step 1");
            rollClass.lastRollButtonDownTime = -50;
            rollClass.rollAble = true;
            controller.height = rollClass.ctrHeight;
            controller.center = new Vector3(controller.center.x, rollClass.ctrCenter, controller.center.z);
        }

        if (rollButton && rollClass.rollAble && !jumpClass.Jumped && !rollClass.Rolled)//&& rollClass.lastRollButtonDownTime < 0 )//&& !jumpClass.Jumped )
        {
            //Debug.Log("We are in step 2");
            playerAnimator.SetBool("Roll", true);
            //Debug.Log("Rolled");
            //ShieldAnimator.SetBool("Roll", true);

            rollClass.lastRollButtonDownTime = Time.time;
            rollClass.Rolled = true;

            controller.height = rollClass.ctrHeight;
            controller.center = new Vector3(controller.center.x, rollClass.ctrCenter, controller.center.z);
        }

        else if (rollClass.Rolled && rollClass.rollAble && rollClass.lastRollButtonDownTime > 0 && !jumpClass.Jumped)
        {
            //Debug.Log("We are in step 3");
            rollClass.rollAble = false;
            rollClass.rollTime = rollClass.rollCoolDown;
            controller.height = rollClass.rollHeight;
            controller.center = new Vector3(controller.center.x, rollClass.rollCenter, controller.center.z);
        }

        else if ((!rollClass.rollAble && !jumpClass.Jumped))
        {
            //Debug.Log("We are in step 4");

            playerAnimator.SetBool("Roll", false);
            //ShieldAnimator.SetBool("Roll", false);

            if (rollClass.rollTime > 0)
            {
                rollClass.rollTime -= Time.deltaTime;
                if (rollClass.rollTime <= 0)
                {
                    rollClass.Rolled = false;
                    rollClass.rollAble = true;
                    controller.height = rollClass.ctrHeight;
                    controller.center = new Vector3(controller.center.x, rollClass.ctrCenter, controller.center.z);
                }
            }
        }
    }

    public void jumpManagement()
    {
        bool jumpButton = CrossPlatformInputManager.GetButton("Jump");

        if (!jumpButton && !jumpClass.Jumped)
        {
            //Debug.Log("We are in step 1");
            jumpClass.lastJumpButtonDownTime = -100;
            jumpClass.jumpAble = true;
        }

        if (jumpButton && jumpClass.jumpAble && !rollClass.Rolled && !jumpClass.Jumped)//&& rollClass.lastRollButtonDownTime < 0 )//&& !jumpClass.Jumped )
        {
            //Debug.Log("We are in step 2");
            playerAnimator.SetBool("Jump", true);
            jumpClass.lastJumpButtonDownTime = Time.time;
            jumpClass.Jumped = true;
        }

        else if (jumpClass.Jumped && jumpClass.jumpAble && jumpClass.lastJumpButtonDownTime > 0 && !rollClass.Rolled)
        {
            //Debug.Log("We are in step 3");
            jumpClass.jumpAble = false;
            jumpClass.jumpTime = jumpClass.jumpCoolDown;
        }

        else if ((!jumpClass.jumpAble && !rollClass.Rolled))
        {
            //Debug.Log("We are in step 4");

            playerAnimator.SetBool("Jump", false);

            if (jumpClass.jumpTime > 0)
            {
                jumpClass.jumpTime -= Time.deltaTime;
                if (jumpClass.jumpTime <= 0)
                {
                    jumpClass.Jumped = false;
                    jumpClass.jumpAble = true;
                }
            }
        }

        if (!grounded && !jumpClass.Jumped && jumpClass.jumpAble)
        {
            moveDirection.y = -35 * gravity * Time.deltaTime;
        }
    }
  
    void SetCoinCount()
    {
        CoinCountText.text = coinCount.ToString();
    }

    IEnumerator Flasher()
    {
        while ( hurtClass.hurting )
        {
            characterR.enabled = false;
            rodR.enabled = false;
            shieldR.enabled = false;
            yield return new WaitForSeconds(.03f);

            characterR.enabled = true;
            rodR.enabled = true;
            shieldR.enabled = true;
            yield return new WaitForSeconds(.03f);
        }
    }
}