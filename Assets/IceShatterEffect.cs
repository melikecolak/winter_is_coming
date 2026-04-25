using UnityEngine;

public class IceShatterEffect : MonoBehaviour
{
    [Header("Buz Parçaları")]
    public GameObject iceShardPrefab;
    public int shardCount = 20;
    public float explosionForce = 4f;
    public float fadeDelay = 2.5f;

    [Header("Ses")]
    public AudioClip shatterSound;
    public float volume = 1f;

    public void Shatter()
{
    if (shatterSound != null)
        AudioSource.PlayClipAtPoint(shatterSound, transform.position, volume);

    // ── YENİ: Kılıcı kemikten kopar, fizikle düşür ──────────────────
    DetachChildObjects();
    // ─────────────────────────────────────────────────────────────────

    SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
    Bounds bounds;
    if (smr != null)
        bounds = smr.bounds;
    else
        bounds = new Bounds(transform.position, Vector3.one * 2f);

    for (int i = 0; i < shardCount; i++)
{
    // Karakterin etrafında rastgele bir yön seç
    Vector3 randomDir = Random.insideUnitSphere;
    randomDir.y = Mathf.Abs(randomDir.y); // Hep yukarı doğru çıksın

    // Spawn noktası: karakterin merkezi + dışarı doğru biraz
    Vector3 spawnPos = bounds.center + randomDir * Random.Range(0.1f, 0.4f);

    GameObject shard = Instantiate(iceShardPrefab, spawnPos, Random.rotation);

    Rigidbody rb = shard.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;

        // Dışarı + yukarı doğru fırlat
        Vector3 force = randomDir * explosionForce;
        force.y += Random.Range(1f, 3f); // Yukarı fırlamasını garantile
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
    }

    IceFade fade = shard.AddComponent<IceFade>();
    fade.fadeTime = fadeDelay + Random.Range(0f, 1.5f);
    Destroy(shard, 6f);
}
}

// ── YENİ METOD ───────────────────────────────────────────────────────
private void DetachChildObjects()
{
    // Tüm child Transform'ları topla (SkinnedMeshRenderer hariç)
    // Kılıç, kalkan, vs. kemiklere bağlı objeler bunlar
    foreach (Transform child in GetComponentsInChildren<Transform>())
    {
        // Kendisi veya SkinnedMeshRenderer'ın objesi ise atla
        if (child == transform) continue;
        if (child.GetComponent<SkinnedMeshRenderer>() != null) continue;

        // Sadece MeshRenderer olan objeleri kopar (kılıç gibi)
        MeshRenderer mr = child.GetComponent<MeshRenderer>();
        if (mr == null) continue;

        // Hierarchy'den kopar — world pozisyonunu koru
        child.SetParent(null, worldPositionStays: true);

        // Rigidbody yoksa ekle
        Rigidbody rb = child.GetComponent<Rigidbody>();
        if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.linearDamping = 0.3f;
        rb.angularDamping = 0.3f;

        // Hafif rastgele kuvvet ver — doğal düşsün
        Vector3 randomDir = (Vector3.up * 0.5f + Random.insideUnitSphere).normalized;
        rb.AddForce(randomDir * Random.Range(1f, 3f), ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

        // 6 saniye sonra yok et
        Destroy(child.gameObject, 6f);
    }
}
}