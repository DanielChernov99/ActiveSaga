using UnityEngine;
using System;

public class MonsterController : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Settings")]
    public float startDistance = 50f;
    public float monsterSpeed = 5f;

    [Header("Catch Up Settings")]
    public float maxCatchUpSpeed = 12f;
    public float catchUpStrength = 0.15f;

    private bool gameOver = false;
    private bool isChasing = false;

    public event Action OnMonsterCaughtPlayer;

    private void Start()
    {
        ResetMonsterPosition();
    }

    private void Update()
    {
        if (!isChasing || gameOver || playerTransform == null)
        {
            return;
        }

        float distance = playerTransform.position.z - transform.position.z;

        float speed = monsterSpeed;

        if (distance > startDistance)
        {
            float extraDistance = distance - startDistance;
            float catchUpSpeed = extraDistance * catchUpStrength;

            speed += catchUpSpeed;
        }

        speed = Mathf.Clamp(speed, 0f, maxCatchUpSpeed);

        transform.position += Vector3.forward * speed * Time.deltaTime;

        if (distance <= 0f)
        {
            CatchPlayer();
        }
    }

    public void BeginChase()
    {
        gameOver = false;
        isChasing = true;
        ResetMonsterPosition();
    }

    public void StopChase()
    {
        isChasing = false;
    }

    public void ResetMonsterPosition()
    {
        if (playerTransform == null)
        {
            Debug.LogError("MonsterController: Missing playerTransform");
            return;
        }

        Vector3 startPos = playerTransform.position - new Vector3(0, 0, startDistance);
        transform.position = startPos;
    }

    private void CatchPlayer()
    {
        if (gameOver)
        {
            return;
        }

        gameOver = true;
        isChasing = false;

        Debug.Log("GAME OVER - Monster caught you!");

        OnMonsterCaughtPlayer?.Invoke();
    }
}