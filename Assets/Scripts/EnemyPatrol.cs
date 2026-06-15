using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public Transform player;
    public float speed = 3f;
    public float catchDistance = 1.6f;

    private Transform target;

    void Start()
    {
        target = pointB;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        Patrol();
        CheckPlayerDistance();
    }

    void Patrol()
    {
        if (pointA == null || pointB == null)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
        {
            target = target == pointA ? pointB : pointA;
        }
    }

    void CheckPlayerDistance()
    {
        if (player == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver("Você foi capturado pelo inimigo!");
            }
        }
    }
}
