using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;
    public float moveSpeed = 1f;
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
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        // Check for spacebar press and adjust moveSpeed
        if (Input.GetKey(KeyCode.Space))
        {
            running = true;
            moveSpeed = 5f;
            Boost -= RunCost * Time.deltaTime;

            if (Boost < 0) Boost = 0;

            BoostBar.fillAmount = Boost / MaxBoost;

            // Stop recharging if running
            if (recharge != null)
            {
                StopCoroutine(recharge);
                recharge = null;
            }
        }
        else
        {
            running = false;
            moveSpeed = 1f;

            // Start recharge if not already running
            if (recharge == null)
            {
                recharge = StartCoroutine(RechargeBoost());
            }
        }

        // Apply movement
        Vector3 movement = m_Movement * moveSpeed * Time.deltaTime;
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);

        // Determine if the player is walking
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        // Handle audio playback
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

        // Rotate towards movement direction
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
        // Wait 1 second before starting recharge
        yield return new WaitForSeconds(1f);

        // Gradually recharge Boost
        while (Boost < MaxBoost)
        {
            Boost += ChargeRate * Time.deltaTime; // Recharge based on time
            if (Boost > MaxBoost) Boost = MaxBoost; // Clamp Boost to MaxBoost
            BoostBar.fillAmount = Boost / MaxBoost; // Update BoostBar
            yield return null; // Wait for the next frame
        }

        recharge = null; // Reset the coroutine reference when finished
    }

}
