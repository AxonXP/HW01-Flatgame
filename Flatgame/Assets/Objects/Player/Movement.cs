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

    // Components
    Rigidbody2D c_rb;
    Animator c_anim;
    TimeFreezer c_tf;
    Camera c_cam;

    [SerializeField] ParticleSystem PS_skid;

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
        else if(PS_skid.isEmitting)
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
            c_anim.SetTrigger("Attack" + nextAttack);

            nextAttack++;
            if(nextAttack > 2)
                nextAttack = 0;

            attacking = true;
            StartCoroutine(AttackSequence(collision));
        }
    }

    IEnumerator AttackSequence(Collision2D collision)
    {
        c_cam.fieldOfView = 15f; // BIG ZOOM IN
        c_tf.FreezeTime(1f, 0);
        yield return new WaitForSecondsRealtime(1f);


        c_cam.fieldOfView = 50f; // BIG ZOOM OUT (SHOW COOL VISUAL)
        Vector2 enemyDirection = collision.transform.position - transform.position;
        c_cam.transform.position = (Vector3)(enemyDirection * 5) + Vector3.back * 10;

        c_tf.FreezeTime(1f, 0);
        yield return new WaitForSecondsRealtime(1f);

        c_anim.SetTrigger("AttackEnd");
        c_cam.fieldOfView = 30f; // base
        c_cam.transform.position = Vector3.back * 10;
    }
}
