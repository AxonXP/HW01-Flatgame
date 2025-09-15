using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ParticleSystem deathParticle;

    public void SpawnDeathParticle(float angle)
    {
        deathParticle.transform.position = transform.position;
        deathParticle.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var PS = Instantiate(deathParticle,transform.position,Quaternion.identity);
        PS.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
