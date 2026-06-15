using UnityEngine;

public class CrystalCollectible : MonoBehaviour
{
    public float rotationSpeed = 80f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCrystal();
        }

        Destroy(gameObject);
    }
}
