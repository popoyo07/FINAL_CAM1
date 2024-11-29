using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;

    // Boost setting
    public float moveSpeed;
    public bool running = false;
    public Image BoostBar;
    public float Boost, MaxBoost;
    public float RunCost;
    public float ChargeRate;
    private Coroutine recharge;


    AudioSource m_AudioSource;
    Animator m_Animator;
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource>();
        // Initialize Boost system
        if (MaxBoost <= 0) MaxBoost = 100f; // Default value if not set
        Boost = MaxBoost;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        // Check for spacebar press and adjust moveSpeed
        if (Input.GetKey(KeyCode.Space) && Boost > 0)
        {
            running = true;
            //Debug.Log("pressing space");
        }
        else
        {
            running = false;
        }

        if (running)
        {
            if (recharge != null)
            {
                StopCoroutine(recharge); // Stop recharge if running
                recharge = null;
            }

            moveSpeed = 1.5f;
            //Debug.Log("running");

            Boost -= RunCost * Time.deltaTime;
            Boost = Mathf.Clamp(Boost, 0, MaxBoost);
            BoostBar.fillAmount = Boost / MaxBoost;
        }
        else
        {
            moveSpeed = 0f;
            // Start recharging Boost if not already recharging
            if (recharge == null)
            {
                recharge = StartCoroutine(RechargeBoost());
            }
        }

        // Apply movement
        Vector3 movement = m_Movement * moveSpeed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop();
        }

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
    }
    void OnAnimatorMove()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation(m_Rotation);
    }

    private IEnumerator RechargeBoost()
    {
        yield return new WaitForSeconds(1f);
        while (Boost < MaxBoost)
        {
            Boost += ChargeRate * Time.deltaTime; // Recharge boost slowly
            Boost = Mathf.Clamp(Boost, 0, MaxBoost);
            BoostBar.fillAmount = Boost / MaxBoost;
            yield return null; // Wait until the next frame
        }

        recharge = null; // Mark coroutine as finished
    }
}
