using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Modifiers
    [Range(0,100)] public float speed = 10;
    [Range(0, 100)] public float acceleration = 10;
    [Range(0, 100)] public float decceleration = 10;

    public float crazyRunThreshold;

    // States (variables changed within code)
    Vector2 moveDirection;
    bool attacking;
    bool skidding;
    int nextAttack = 0;

    bool bigZoom;
    bool triBreaks;

    // Components
    Rigidbody2D c_rb;
    Animator c_anim;
    TimeFreezer c_tf;
    Camera c_cam;

    [SerializeField] ParticleSystem PS_skid;
    [SerializeField] ParticleSystem PS_Spike;
    [SerializeField] GameObject PS_BigSpike;
    [SerializeField] ParticleSystem PS_HitParticlesBAD;
    [SerializeField] GameObject PS_PostitTri;
    [SerializeField] ParticleSystem PS_PostitBreak;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ComponentGrab();
    }

    void ComponentGrab()
    {
        c_anim = GetComponent<Animator>();
        c_rb = GetComponent<Rigidbody2D>();
        c_tf = GetComponent<TimeFreezer>();

        c_cam = FindAnyObjectByType<Camera>();
    }

    public void OnMove(InputValue value) // using send message system
    {
        moveDirection = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        MovementUpdate();
        AnimationUpdate();
    }

    private void Update()
    {
        if (!attacking)
        {
            c_cam.transform.position = transform.position + Vector3.forward * -10;
        }
    }

    void AnimationUpdate()
    {
        c_anim.SetBool("Still", Mathf.Abs(c_rb.linearVelocity.magnitude) < 0.1);
        c_anim.SetBool("Moving", moveDirection != Vector2.zero);
        c_anim.SetBool("Crazy", (c_rb.linearVelocity.magnitude > crazyRunThreshold));

        skidding = Vector2.Dot(moveDirection, c_rb.linearVelocity) < 0 || (moveDirection.magnitude == 0 && c_rb.linearVelocity.magnitude > 0.1);
        c_anim.SetBool("Skidding", skidding);

        if (!skidding)
        {
            PS_skid.Stop();
            transform.eulerAngles = moveDirection.x > 0 ? Vector3.zero : Vector3.up * 180;
        }
        else if(!PS_skid.isEmitting)
        {
            PS_skid.Play();
        }
    }

    void MovementUpdate()
    {
        //Basic left and right movement
        float targetSpeed = moveDirection.x * speed;
        float speedDif = targetSpeed - c_rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = speedDif * accelRate;
        c_rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        //Basic up and down movement
        targetSpeed = moveDirection.y * speed;
        speedDif = targetSpeed - c_rb.linearVelocity.y;
        accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        movement = speedDif * accelRate;
        c_rb.AddForce(movement * Vector2.up, ForceMode2D.Force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && attacking == false)
        {
            attacking = true;
            StartCoroutine(AttackSequence(collision));
        }
    }

    IEnumerator AttackSequence(Collision2D collision)
    {
        c_anim.SetTrigger("Attack" + nextAttack);

        nextAttack++;
        if (nextAttack > 2)
            nextAttack = 0;
        Vector2 enemyDirection = collision.transform.position - transform.position;
        var IndexPoint = transform.position + (Vector3)(enemyDirection / 2);
        var dir = collision.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        print(angle);


        // PS SET UP FOR BIG ZOOM
        PS_Spike.transform.position = transform.position + (Vector3)(enemyDirection / 2);
        //bigZoom = true;
        PS_Spike.gameObject.SetActive(true);
        PS_PostitTri.gameObject.SetActive(true);
        PS_PostitTri.transform.position = IndexPoint;
        PS_PostitTri.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        c_cam.fieldOfView = 15f; // BIG ZOOM IN
        c_tf.FreezeTime(1f, 0);
        yield return new WaitForSecondsRealtime(1f);

        // CAMERASET UP FOR ZOOM OUT
        c_cam.fieldOfView = 40f; // BIG ZOOM OUT (SHOW COOL VISUAL)
        c_cam.transform.position = transform.position + Vector3.forward * -10 + ((Vector3)(enemyDirection * 3) + Vector3.back * 10);

        c_tf.FreezeTime(1f, 0);
        //POSTIT TRI BREAKS
        yield return new WaitForSecondsRealtime(1f);
        PS_PostitTri.gameObject.SetActive(false);
        //bigZoom = false;
        //triBreaks = true;
        PS_PostitBreak.transform.position = IndexPoint;
        PS_PostitBreak.gameObject.SetActive(true);
        PS_PostitBreak.Play();
        PS_PostitBreak.transform.rotation = Quaternion.AngleAxis(angle - 45, Vector3.forward);
        PS_BigSpike.transform.position = IndexPoint;
        PS_BigSpike.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        PS_BigSpike.gameObject.SetActive(true);

        c_tf.FreezeTime(1f, 0);
        yield return new WaitForSecondsRealtime(1f);

        PS_HitParticlesBAD.transform.position = IndexPoint;

        PS_HitParticlesBAD.transform.rotation = Quaternion.AngleAxis(angle-45, Vector3.forward);

        
        PS_HitParticlesBAD.gameObject.SetActive(true);
        PS_HitParticlesBAD.Play();

        c_tf.FreezeTime(2f, 0);
        yield return new WaitForSecondsRealtime(2f);

        //triBreaks = false;
        c_anim.SetTrigger("AttackEnd");
        c_cam.fieldOfView = 30f; // base
        c_cam.transform.position = transform.position + Vector3.forward * -10;
        PS_Spike.Stop();
        PS_HitParticlesBAD.Stop();
        PS_PostitBreak.gameObject.SetActive(false);
        PS_BigSpike.gameObject.SetActive(false);
        PS_HitParticlesBAD.gameObject.SetActive(false);
        PS_Spike.gameObject.SetActive(false);
        attacking = false;
    }
}
