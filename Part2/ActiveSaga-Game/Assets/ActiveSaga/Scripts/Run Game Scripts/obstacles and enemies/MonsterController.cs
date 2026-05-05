using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Settings")]
    public float startDistance = 50f;
    public float monsterSpeed = 5f;

    [Header("Catch Up Settings")]
    public float maxCatchUpSpeed = 12f;     // מהירות מקסימלית כשהוא רחוק
    public float catchUpStrength = 0.15f;   // כמה מהר הוא מגיב לפער

    private bool gameOver = false;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("MonsterController: Missing playerTransform");
            return;
        }

        Vector3 startPos = playerTransform.position - new Vector3(0, 0, startDistance);
        transform.position = startPos;
    }

    private void Update()
    {
        if (gameOver || playerTransform == null) return;

        float distance = playerTransform.position.z - transform.position.z;

        // 🔥 בסיס: מהירות קבועה
        float speed = monsterSpeed;

        // 🔥 אם הוא מאחור ביותר מ-50 → מתחיל לרדוף חזק יותר
        if (distance > startDistance)
        {
            float extraDistance = distance - startDistance;

            // בוסט פרופורציונלי לפער
            float catchUpSpeed = extraDistance * catchUpStrength;

            speed += catchUpSpeed;
        }

        // 🔥 מגבלת מהירות כדי שלא ישתגע
        speed = Mathf.Clamp(speed, 0f, maxCatchUpSpeed);

        // תנועה קדימה
        transform.position += Vector3.forward * speed * Time.deltaTime;

        // בדיקת Game Over
        if (distance <= 0f)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("GAME OVER - Monster caught you!");
    }
}