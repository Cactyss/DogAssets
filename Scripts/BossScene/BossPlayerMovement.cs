using System.Collections;
using UnityEngine;


public class BossPlayerMovement : MonoBehaviour
{
    [SerializeField] private Collider2D PlayerHitbox;
    [SerializeField] private BossHUDManager UI;
    [SerializeField] public Rigidbody2D body;
    [SerializeField] private BossFightManager manager;
    [SerializeField] private Animator anim;
    [SerializeField] private Collider2D ThrustHitbox;
    [SerializeField] private Collider2D SpinHitbox;

    [SerializeField] private SpriteRenderer sprite;

    [SerializeField] LayerMask StopsMovement;
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float DashSpeed;
    [SerializeField] private float RotationSpeed;
    [SerializeField] public float DashCDnum;
    public bool DashingOnCD;
    public bool CanMove;
    public const string ThrustAnimName = "P_Thrust";
    public const string SpinAnimName = "P_Spin";
    public const string StaticAnimName = "Static1";
    public int AnimState; // 0:Static, 1:Thrust, 2:Spin

    private bool FadeOutBool;
    public bool GetBelowBool;
    private bool PlayerInPositionBool;

    private void Start()
    {
        PlayerInPositionBool = false;
        DashCDnum = 2;
        MoveSpeed = 5;
        DashSpeed = 0.5f;
        RotationSpeed = 10;
        AnimState = 0;
        CanMove = true;
        DashingOnCD = false;
        ThrustHitbox.enabled = false;
        SpinHitbox.enabled = false;
        FadeOutBool = false;
        GetBelowBool = false;
    }

    void Update()
    {
        if (FadeOutBool)
        {
            if (sprite.color.a - (0.5f * Time.deltaTime) < 0)
            {
                Color color = sprite.color;
                color.a = 0;
                sprite.color = color;
                FadeOutBool = false;
            }
            else
            {
                Color color = sprite.color;
                color.a = color.a - (0.5f * Time.deltaTime);
                sprite.color = color;
            }
        }
        if (GetBelowBool)
        {
            //Debug.Log("Step One");
            if (transform.position.y > -1)
            {
                //body.velocity += new Vector2(0, -0.5f * Time.deltaTime);
                if (body.velocity.magnitude < 1)
                {
                    if (body.velocity.x > 0)
                    {
                        body.velocity = new Vector2(body.velocity.x - 0.1f, -0.5f);
                    }
                    else
                    {
                        body.velocity = new Vector2(body.velocity.x + 0.1f, -0.5f);
                    }
                }
            }
        }
        

        //Debug.Log(PlayingAnim(ThrustAnimName));
        if (CanMove)
        {
            //Point Towards Cursor
            float angle;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; 
            Vector3 direction = mousePos - transform.position;
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentRotation = transform.rotation.eulerAngles.z;
            float rotationStep = RotationSpeed * Time.deltaTime;
            float newRotation = Mathf.LerpAngle(currentRotation, angle - 90, rotationStep);
            transform.rotation = Quaternion.Euler(0, 0, newRotation);

            //Horizontal Movement
            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
            {
                if (!Physics2D.OverlapBox(transform.position + new Vector3(Input.GetAxisRaw("Horizontal") * 0.05f, 0, 0), new Vector2(0.2f, 0.2f), 0, StopsMovement))
                {
                    this.transform.position = this.transform.position + new Vector3(MoveSpeed * Input.GetAxisRaw("Horizontal") * Time.deltaTime, 0, 0);
                }
            }
            //Vertical Movement
            if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
            {
                if (!Physics2D.OverlapBox(transform.position + new Vector3(0, Input.GetAxisRaw("Vertical") * 0.05f, 0), new Vector2(0.2f, 0.2f), 0, StopsMovement))
                {
                    this.transform.position = this.transform.position + new Vector3(0, MoveSpeed * Input.GetAxisRaw("Vertical") * Time.deltaTime, 0);
                }

            }
            //Dash Parry
            if (Input.GetKeyDown(KeyCode.Space) && !DashingOnCD)
            {
                float rot;
                if (body.rotation > 0)
                {
                    rot = body.rotation;
                }
                else
                {
                    rot = body.rotation * body.rotation;
                }
                bool Dashing = false;
                if (Input.GetAxisRaw("Vertical") != 0)
                {
                    Debug.Log("v");
                    Dashing = true;
                    this.transform.position = this.transform.position + new Vector3(0, DashSpeed * Input.GetAxisRaw("Vertical"), 0);
                }
                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    Debug.Log("h");
                    Dashing = true;
                    this.transform.position = this.transform.position + new Vector3(DashSpeed * Input.GetAxisRaw("Horizontal"), 0, 0);
                }
                if (!Dashing)
                {
                    Debug.Log("d: " + transform.up + new Vector3((DashSpeed * Mathf.Cos(body.rotation)), (DashSpeed * Mathf.Sin(body.rotation)), 0) + body.rotation);
                    this.transform.position = this.transform.position + new Vector3((DashSpeed * transform.up.x), (DashSpeed * transform.up.y), 0);
                }
                StartCoroutine("Dashing");
                UI.StartDashCD();
            }
            //Left Click Attack
            if (Input.GetKeyDown(KeyCode.Mouse0) && AnimState == 0 && manager.HasStamina(manager.PlayerThrustStaminaCost))
            {
                anim.Play(ThrustAnimName);
                AnimState = 1;
                manager.UseStamina(ThrustAnimName);
                StartCoroutine("Thrusting");
            }
            //Right Click Attack
            if (Input.GetKeyDown(KeyCode.Mouse1) && AnimState == 0 && manager.HasStamina(manager.PlayerSpinStaminaCost))
            {
                anim.Play(SpinAnimName);
                AnimState = 2;
                manager.UseStamina(SpinAnimName);
                StartCoroutine("Spinning");
            }

        }
    }

    IEnumerator Dashing()
    {
        PlayerHitbox.enabled = false;
        yield return new WaitForSeconds(0.25f);
        PlayerHitbox.enabled = true;
    }

    IEnumerator Thrusting()
    {
        ThrustHitbox.enabled = true;
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => AnimDone(ThrustAnimName));
        //Debug.Log("done t");
        ThrustHitbox.enabled = false;
        AnimState = 0;
    }
    IEnumerator Spinning()
    {
        SpinHitbox.enabled = true;
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => AnimDone(SpinAnimName));
        //Debug.Log("done s");
        SpinHitbox.enabled = false;
        AnimState = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        manager.PlayerHit(collision.tag.ToString());
    }

    private bool AnimDone(string name)
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
    }

    private bool PlayingAnim(string name)
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        int nameHash = stateInfo.shortNameHash;
        var controller = anim.runtimeAnimatorController;
        foreach (var animationClip in controller.animationClips)
        {
            if (Animator.StringToHash(animationClip.name) == nameHash)
            {
                if (name.Equals(animationClip.name))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void DisableWeapons()
    {
        SpinHitbox.enabled = false;
        ThrustHitbox.enabled = false;
    }
    public void DisableEverything()
    {
        CanMove = false;
        StopAllCoroutines();
        body.velocity = Vector3.zero;
        anim.Play(StaticAnimName);
        AnimState = 0;
    }
    public void EnableEverything()
    {
        CanMove = true;
        anim.Play(StaticAnimName);
        AnimState = 0;
        DashingOnCD = false;
    }
    public void SetupPhase2()
    {
        StartCoroutine("GetBelow");
    }
    IEnumerator GetBelow()
    {
        yield return new WaitForSeconds(4);
        GetBelowBool = true;
        Debug.Log("on");
        yield return new WaitUntil(() => transform.position.y < -1 && body.velocity.magnitude < 0.5);
        GetBelowBool = false;
        PlayerInPositionBool = true;
        Debug.Log("off");
    }
    public bool PlayerInPosition()
    {
        if (PlayerInPositionBool)
        {
            body.velocity = Vector2.zero;
            return true;
        }
        else
        {
            return false;
        }
    }
}

