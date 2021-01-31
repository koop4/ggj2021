using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent (typeof (HealthController))]
public class PlayerController : MonoBehaviour {
    public int score = 0;
    public int frags = 0;
    public int assists = 0;
    public delegate void OnPlayerDeathDelegate ();
    public event OnPlayerDeathDelegate deathEvent;
    private HealthController healthController;
    public Animator animator;
    private bool grounded = true;
    private Rigidbody playerRigidbody;
    private Vector3 direction;
    public float rotateSpeed = 1;
    public float defaultSpeed = 12;
    public float movementSpeed = 1;
    public float sprintSpeedUpTime = .01f;
    public float sprintDuration = .3f;
    public float sprintSpeed = 60;
    public float sprintChargingSpeed = 3;
    public float rollCooldown = 2;
    public float timeStamp;

    public float jumpSpeed = 1;
    private float _jumpSpeed = 0;
    public ParticleSystem rollFx;
    //private Dictionary<string, int> directions = new Dictionary<string, int>() { { "N", 0 }, };

    PhotonView PV;
    private void Awake () {
        //PV = GetComponent<PhotonView>();
    }

    private void Start () {
        if (PV && !PV.IsMine) {
            Destroy (GetComponentInChildren<Camera> ().gameObject);
        }

        healthController = GetComponent<HealthController> ();
        if (!healthController) {
            healthController = gameObject.AddComponent<HealthController> ();
        }
        playerRigidbody = GetComponent<Rigidbody> ();
        //animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate () {
        if (PV && !PV.IsMine)
            return;

        Debug.Log ("transform.position.y  : " + transform.position.y);
        Debug.Log ("grounded  : " + grounded);
        if (animator != null) {
            if (this.healthController.lives <= 0) {
                Death ();
            }
            if (transform.position.y <= 1f) {
                grounded = true;
                if (animator != null) {
                    animator.SetBool ("grounded", true);

                }
            }
            if (grounded == true) {
                if (Input.GetAxisRaw ("Jump") > 0) {
                    _jumpSpeed = jumpSpeed;
                    grounded = false;
                    animator.SetBool ("grounded", false);
                    animator.SetTrigger ("Jump");

                    playerRigidbody.AddForce (new Vector3 (0, _jumpSpeed, 0), ForceMode.Impulse);

                }
            }

            if (Input.GetAxisRaw ("Attack") > 0 && animator.GetBool ("Attacking") == false) {
                animator.SetBool ("Attacking", true);
            } else {
                animator.SetBool ("Attacking", false);
            }

            if (Input.GetAxisRaw ("Interact") > 0) {
                if (timeStamp <= Time.time) {
                    timeStamp = Time.time + rollCooldown;
                    sprint ();
                    animator.SetTrigger ("Interact");
                }

            }
            Movement ();
        }

    }
    public void Death () {
        animator.SetBool ("Death", true);
        if (deathEvent != null) {
            deathEvent ();
        }
    }

    public void Movement () {
        float horizontalMove = Input.GetAxisRaw ("Horizontal");
        float verticalMove = Input.GetAxisRaw ("Vertical");

        direction = new Vector3 (horizontalMove, 0.0f, verticalMove);

        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (direction), rotateSpeed * Time.deltaTime);
        } else {
            animator.SetInteger ("Move", 0);
        }

        playerRigidbody.MovePosition (transform.position + movementSpeed * Time.deltaTime * direction);
        animator.SetInteger ("Move", 1);
    }

    private void sprint () {

        //  Invoke("stopRollFx", .5f);
        Sequence sprintSequence = DOTween.Sequence ();
        sprintSequence.Append (DOTween.To (() => movementSpeed, x => movementSpeed = x, sprintChargingSpeed, .3f).SetEase (Ease.OutQuad));
        sprintSequence.AppendCallback (() => { rollFx.Play (); });
        sprintSequence.Append (DOTween.To (() => movementSpeed, x => movementSpeed = x, sprintSpeed, sprintSpeedUpTime));
        sprintSequence.Append (DOTween.To (() => movementSpeed, x => movementSpeed = x, defaultSpeed, sprintDuration).SetEase (Ease.OutQuad));
        sprintSequence.AppendCallback (() => { rollFx.Stop (); });
    }

    private void stopSprint () {
        movementSpeed = defaultSpeed;
    }

    private void stopRollFx () {
        rollFx.Stop ();
    }

    private void LateUpdate () {
        // transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);
        //Debug.DrawRay();
    }
}