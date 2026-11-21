using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject bottle;
    public GameObject waterParticle;
    private GameObject activeBottle;
    private float radius = 50f;

    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        activeBottle = Instantiate(bottle, SelectSpawn(), Quaternion.identity);
    }

    public void RespawnBottle()
    {
        Instantiate(waterParticle, activeBottle.transform.position, Quaternion.identity);
        activeBottle.transform.position = SelectSpawn();
    }

    private Vector3 SelectSpawn()
    {
        float x = Random.Range(-radius, radius);
        float z = Random.Range(-radius, radius);

        Vector3 spawnPosition = transform.position + new Vector3(x, 10, z);
        return spawnPosition;
    }
}
