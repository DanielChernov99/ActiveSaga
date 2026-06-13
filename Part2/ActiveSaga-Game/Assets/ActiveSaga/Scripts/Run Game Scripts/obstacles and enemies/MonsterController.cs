using UnityEngine;
using System;

public class MonsterController : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Visuals")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private bool hideVisualsOnCatch = true;
    [SerializeField] private bool disableCollidersOnCatch = true;

    [Header("Audio")]
    [SerializeField] private bool stopAudioOnCatch = true;

    [Header("Settings")]
    public float startDistance = 50f;
    public float monsterSpeed = 5f;

    [Header("Catch Up Settings")]
    public float maxCatchUpSpeed = 12f;
    public float catchUpStrength = 0.15f;

    private bool gameOver = false;
    private bool isChasing = false;

    private Renderer[] monsterRenderers;
    private Collider[] monsterColliders;
    private AudioSource[] monsterAudioSources;
    private Random3DAudioEmitter[] randomAudioEmitters;
    private Looping3DAudioEmitter[] loopingAudioEmitters;

    public event Action OnMonsterCaughtPlayer;

    private void Awake()
    {
        CacheVisualParts();
        CacheAudioParts();
    }

    private void Start()
    {
        SetMonsterVisible(true);
        ResetMonsterPosition();
        SetMonsterAudioActive(false);
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

        SetMonsterVisible(true);
        ResetMonsterPosition();
        SetMonsterAudioActive(true);
    }

    public void StopChase()
    {
        isChasing = false;
        SetMonsterAudioActive(false);
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

        if (hideVisualsOnCatch)
        {
            SetMonsterVisible(false);
        }

        if (stopAudioOnCatch)
        {
            SetMonsterAudioActive(false);
        }

        OnMonsterCaughtPlayer?.Invoke();
    }

    private void CacheVisualParts()
    {
        Transform targetRoot = visualRoot != null ? visualRoot.transform : transform;

        monsterRenderers = targetRoot.GetComponentsInChildren<Renderer>(true);
        monsterColliders = targetRoot.GetComponentsInChildren<Collider>(true);
    }

    private void CacheAudioParts()
    {
        monsterAudioSources = GetComponentsInChildren<AudioSource>(true);
        randomAudioEmitters = GetComponentsInChildren<Random3DAudioEmitter>(true);
        loopingAudioEmitters = GetComponentsInChildren<Looping3DAudioEmitter>(true);
    }

    private void SetMonsterVisible(bool visible)
    {
        if (monsterRenderers != null)
        {
            for (int i = 0; i < monsterRenderers.Length; i++)
            {
                if (monsterRenderers[i] != null)
                {
                    monsterRenderers[i].enabled = visible;
                }
            }
        }

        if (disableCollidersOnCatch && monsterColliders != null)
        {
            for (int i = 0; i < monsterColliders.Length; i++)
            {
                if (monsterColliders[i] != null)
                {
                    monsterColliders[i].enabled = visible;
                }
            }
        }
    }

    private void SetMonsterAudioActive(bool active)
    {
        if (randomAudioEmitters != null)
        {
            for (int i = 0; i < randomAudioEmitters.Length; i++)
            {
                if (randomAudioEmitters[i] != null)
                {
                    randomAudioEmitters[i].enabled = active;
                }
            }
        }

        if (loopingAudioEmitters != null)
        {
            for (int i = 0; i < loopingAudioEmitters.Length; i++)
            {
                if (loopingAudioEmitters[i] != null)
                {
                    loopingAudioEmitters[i].enabled = active;
                }
            }
        }

        if (!active && monsterAudioSources != null)
        {
            for (int i = 0; i < monsterAudioSources.Length; i++)
            {
                if (monsterAudioSources[i] != null)
                {
                    monsterAudioSources[i].Stop();
                }
            }
        }
    }
}