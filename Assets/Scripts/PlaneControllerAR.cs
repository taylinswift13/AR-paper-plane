using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class PlaneControllerAR : MonoBehaviour
{
    public float speedForward = 3f;
    public float speedDownward = 0.01f;
    public float speedSides = 5f;
    public float rotationSpeed = 100f;
    float rotationMaxAngel = 35;
    bool takeOff = false;
    bool crashed = false;
    bool rotationDisabled = false;
    Animator animator;
    public GameObject HandSlideUp;
    public GameObject HandPressingLeft;
    public GameObject HandPressingRight;
    private Vector2 mouseStartPos;
    private bool isDragging = false;
    public bool testMode;
    public AudioClip takeOffSound;
    public AudioClip boosterSound;
    public AudioClip fallingSound;


    private void Start()
    {
        animator = GetComponent<Animator>();
        if (!PlayerPrefs.HasKey("TakeOffFinished") || testMode) HandSlideUp.SetActive(true);
    }

    void LateUpdate()
    {

        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                Vector2 mouseEndPos = Input.mousePosition;
                float swipeDistance = mouseEndPos.y - mouseStartPos.y;

                // Check if it's a slide-up gesture
                if (swipeDistance > 1 && !takeOff)
                {
                    if (!PlayerPrefs.HasKey("TakeOffFinished") || testMode)
                    {
                        GetComponent<AudioSource>().PlayOneShot(takeOffSound);
                        HandSlideUp.SetActive(false);
                        HandPressingLeft.SetActive(true);
                        HandPressingRight.SetActive(true);

                        PlayerPrefs.SetInt("TakeOffFinished", 1);
                        PlayerPrefs.Save();
                    }
                    ApplyTakeOffEffect();
                }

                isDragging = false;
            }
        }

        if (takeOff)
        {
            if (!crashed)
            {
                transform.position += new Vector3(-speedForward * Time.deltaTime, 0, 0);
                if (!rotationDisabled)
                {
                    if (Input.GetMouseButton(0))
                    {
                        float mousePosition = Input.mousePosition.x;

                        if (mousePosition < Screen.width / 2)
                        {
                            // Move left
                            transform.position -= new Vector3(0, 0, speedSides * Time.deltaTime);
                            if (!PlayerPrefs.HasKey("TurnLeftFinished") || testMode)
                            {
                                HandPressingLeft.SetActive(false);
                                PlayerPrefs.SetInt("TurnLeftFinished", 1);
                                PlayerPrefs.Save();
                            }

                            if (Mathf.Abs(transform.rotation.eulerAngles.y - 360) <= rotationMaxAngel || transform.rotation.eulerAngles.y == 0)
                            {
                                transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
                            }
                        }
                        else
                        {
                            // Move right
                            transform.position += new Vector3(0, 0, speedSides * Time.deltaTime);
                            if (!PlayerPrefs.HasKey("TurnRightFinished") || testMode)
                            {
                                HandPressingRight.SetActive(false);
                                PlayerPrefs.SetInt("TurnRightFinished", 1);
                                PlayerPrefs.Save();
                            }
                            if (Mathf.Abs(transform.rotation.eulerAngles.y) <= rotationMaxAngel)
                            {
                                transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
                            }
                        }
                    }

                    else if (Input.GetMouseButtonUp(0))
                    {
                        transform.DORotate(new Vector3(0, 0, 0), 0.3f);
                    }
                }
            }
            transform.position += new Vector3(0, -speedDownward * Time.deltaTime, 0);
        }
    }

    void TurnLeft()
    {
        if (!rotationDisabled)
        {
            transform.position -= new Vector3(0, 0, speedSides * Time.deltaTime);
            if (Mathf.Abs(transform.rotation.eulerAngles.y - 360) <= rotationMaxAngel || transform.rotation.eulerAngles.y == 0)
                transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
        }
    }

    void TurnRight()
    {
        if (!rotationDisabled)
        {
            transform.position += new Vector3(0, 0, speedSides * Time.deltaTime);
            if (Mathf.Abs(transform.rotation.eulerAngles.y) <= rotationMaxAngel)
                transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacles")
        {
            GetComponent<AudioSource>().PlayOneShot(fallingSound);
            speedForward = 0;
            speedDownward = 5;
            animator.enabled = true;
            animator.SetTrigger("isFalling");
            crashed = true;
        }
        else if (collision.gameObject.tag == "Ground")
        {
            animator.SetTrigger("stopFalling");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Booster")
        {
            GetComponent<AudioSource>().PlayOneShot(boosterSound);
            ApplyTemporaryBoost(other);
        }
        if (other.gameObject.tag == "Winning Point")
        {
            GetComponent<AudioSource>().PlayOneShot(boosterSound);
            ShowWinningMessage("You Win!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    public Text messageText;
    void ShowWinningMessage(string message)
    {
        // Assuming your UI Text component is already set up in the scene
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);

            // You can also add additional actions here, such as animations or sound effects
        }
        else
        {
            Debug.LogError("UI Text component not assigned in the inspector!");
        }
    }

    private void ApplyTemporaryBoost(Collider boosterCollider)
    {
        rotationDisabled = true;
        Destroy(boosterCollider.gameObject);

        Quaternion initialRotation = transform.rotation;
        Sequence boostSequence = DOTween.Sequence();
        boostSequence.Append(transform.DORotate(new Vector3(0, 0, -20), 0.5f).SetEase(Ease.OutQuad));
        boostSequence.Join(transform.DOJump(transform.position + new Vector3(0, 5f, 0), 2f, 1, 1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.rotation = initialRotation;
                rotationDisabled = false;
            }));
    }

    private void ApplyTakeOffEffect()
    {
        Sequence takeOffSequence = DOTween.Sequence();

        takeOffSequence.Append(transform.DOJump(transform.position + Vector3.up * 5f + new Vector3(-5, 0, 0) * 3f, 1f, 1, 2f)
            .SetEase(Ease.OutQuad));
        takeOffSequence.Append(transform.DORotate(new Vector3(0, 0, 0), 0.1f).SetEase(Ease.InOutQuad));
        takeOffSequence.OnComplete(() =>
        {
            takeOff = true;
        });
    }
}
