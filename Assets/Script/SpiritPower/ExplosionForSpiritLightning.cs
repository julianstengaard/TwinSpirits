using UnityEngine;
using System.Collections;

public class ExplosionForSpiritLightning : MonoBehaviour {
    private IEnumerator Explode(float explosionRadius, float damageOnExplosion) {
        yield return new WaitForSeconds(0.2f);
        Collider[] hits = Physics.OverlapSphere(gameObject.transform.position, explosionRadius, 1 << 8);
        foreach (var other in hits) {
            if (other.tag == "Enemy") {
                other.gameObject.GetComponent<BaseUnit>().TakeDamage(damageOnExplosion, gameObject);
            }
        }
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(gameObject);
    }

    public void Activate(float explosionRadius, float damageOnExplosion) {
        StartCoroutine(Explode(explosionRadius, damageOnExplosion));
    }
}
