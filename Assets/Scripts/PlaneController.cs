using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PlaneController : MonoBehaviour
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

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !takeOff)
        {
            ApplyTakeOffEffect();
        }
        if (takeOff)
        {
            if (!crashed)
            {
                transform.position += new Vector3(-speedForward * Time.deltaTime, 0, 0);

                if (!rotationDisabled)
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        transform.position -= new Vector3(0, 0, speedSides * Time.deltaTime);
                        if (Mathf.Abs(transform.rotation.eulerAngles.y - 360) <= rotationMaxAngel || transform.rotation.eulerAngles.y == 0)
                            transform.Rotate(new Vector3(0, -rotationSpeed, 0) * Time.deltaTime);
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        transform.position += new Vector3(0, 0, speedSides * Time.deltaTime);
                        if (Mathf.Abs(transform.rotation.eulerAngles.y) <= rotationMaxAngel)
                            transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);

                    }
                    else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
                    {
                        transform.DORotate(new Vector3(0, 0, 0), 0.3f);
                    }
                }
            }
            transform.position += new Vector3(0, -speedDownward * Time.deltaTime, 0);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacles")
        {
            speedForward = 0;
            speedDownward = 2;
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
            ApplyTemporaryBoost(other);
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
