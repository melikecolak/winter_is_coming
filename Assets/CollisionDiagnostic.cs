using UnityEngine;
using UnityEngine.AI;

// GECICI DIAGNOSTIC — test bittikten sonra sil.
// Sadece bir NPC instance'ina ekle. Player'a 2.5m yaklasinca loglama baslar.
public class CollisionDiagnostic : MonoBehaviour
{
    const float LOG_DIST    = 2.5f;
    const float SPHERE_SIZE = 0.12f;

    NavMeshAgent        agent;
    Rigidbody           rb;
    CapsuleCollider     cap;
    Animator            anim;
    Transform           playerTransform;
    CharacterController playerCC;

    // onceki frame degerleri — delta hesabi icin
    Vector3 prevTf;
    Vector3 prevNextPos;
    Vector3 prevRbPos;
    Vector3 prevAnimPos;

    int fixedFrame;
    int updateFrame;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb    = GetComponent<Rigidbody>();
        cap   = GetComponent<CapsuleCollider>();
        anim  = GetComponentInChildren<Animator>();

        var playerGO    = GameObject.FindGameObjectWithTag("Player");
        playerTransform = playerGO?.transform;
        playerCC        = playerGO?.GetComponent<CharacterController>();

        prevTf      = transform.position;
        prevNextPos = agent != null ? agent.nextPosition : Vector3.zero;
        prevRbPos   = rb    != null ? rb.position        : Vector3.zero;
        prevAnimPos = Vector3.zero;
    }

    // ── FixedUpdate: fizik-zamani snapshot ──────────────────────────────
    void FixedUpdate()
    {
        if (playerTransform == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > LOG_DIST) return;

        fixedFrame++;

        bool desync = rb != null &&
                      Vector3.Distance(rb.position, transform.position) > 0.005f;
        float desyncMag = rb != null
            ? Vector3.Distance(rb.position, transform.position)
            : 0f;

        Debug.Log(
            $"[FIX #{fixedFrame:D4}] " +
            $"tf={V(transform.position)} | " +
            $"rb.pos={V(rb != null ? rb.position : Vector3.zero)} | " +
            $"rb.vel={V(rb != null ? rb.linearVelocity : Vector3.zero)} | " +
            $"kinematic={rb?.isKinematic} | " +
            $"DESYNC={desync}({desyncMag:F3}m) | " +
            $"agent.nextPos={V(agent.nextPosition)} | " +
            $"agent.updatePos={agent.updatePosition} | " +
            $"agent.stopped={agent.isStopped} | " +
            $"agent.vel={V(agent.velocity)} | " +
            $"agent.desiredVel={V(agent.desiredVelocity)} | " +
            $"dist={dist:F3} | " +
            $"cc.vel={V(playerCC != null ? playerCC.velocity : Vector3.zero)} | " +
            $"cc.grounded={playerCC?.isGrounded}"
        );
    }

    // ── Update: kaynak bazli delta izleme ───────────────────────────────
    void Update()
    {
        if (playerTransform == null || agent == null) return;
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > LOG_DIST) return;

        updateFrame++;

        // Her kaynaktan gelen delta
        Vector3 tfDelta      = transform.position      - prevTf;
        Vector3 nextPosDelta = agent.nextPosition      - prevNextPos;
        Vector3 rbDelta      = rb != null
                               ? rb.position - prevRbPos
                               : Vector3.zero;
        Vector3 animDelta    = anim != null
                               ? anim.deltaPosition
                               : Vector3.zero;

        bool npcMoved     = tfDelta.magnitude > 0.0005f;
        bool nextPosMoved = nextPosDelta.magnitude > 0.0005f;

        // tf hareket ettiyse her zaman logla
        if (npcMoved || nextPosMoved)
        {
            // Hangi kaynaktan geldigini cikar:
            // nextPos == tf delta  → agent suruyor
            // nextPos != tf delta  → baska bir kaynak var
            bool agentDriving = Vector3.Distance(tfDelta, nextPosDelta) < 0.001f;

            Debug.LogWarning(
                $"[UPD #{updateFrame:D4}] " +
                $"tf.delta={V(tfDelta)}({tfDelta.magnitude:F4}m) | " +
                $"nextPos.delta={V(nextPosDelta)}({nextPosDelta.magnitude:F4}m) | " +
                $"rb.delta={V(rbDelta)}({rbDelta.magnitude:F4}m) | " +
                $"anim.deltaPos={V(animDelta)}({animDelta.magnitude:F4}m) | " +
                $"AGENT_DRIVING={agentDriving} | " +
                $"anim.rootMotion={anim?.applyRootMotion} | " +
                $"agent.dest={V(agent.destination)} | " +
                $"agent.remDist={agent.remainingDistance:F3} | " +
                $"agent.pathStatus={agent.pathStatus} | " +
                $"agent.isOnNavMesh={agent.isOnNavMesh} | " +
                $"agent.stopped={agent.isStopped} | " +
                $"agent.autoBraking={agent.autoBraking}"
            );
        }

        prevTf      = transform.position;
        prevNextPos = agent.nextPosition;
        prevRbPos   = rb != null ? rb.position : Vector3.zero;
        prevAnimPos = animDelta;
    }

    // ── Gizmos ──────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (cap != null)
        {
            // CapsuleCollider fiziksel siniri — kirmizi (scale dahil)
            Gizmos.color = Color.red;
            Vector3 worldCenter = transform.TransformPoint(cap.center);
            float   worldRadius = cap.radius * Mathf.Max(
                transform.lossyScale.x, transform.lossyScale.z);
            float halfH = Mathf.Max(0f,
                cap.height * transform.lossyScale.y * 0.5f - worldRadius);
            Vector3 up  = transform.up * halfH;
            Gizmos.DrawWireSphere(worldCenter + up, worldRadius);
            Gizmos.DrawWireSphere(worldCenter - up, worldRadius);
        }

        // transform.position — yesil
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, SPHERE_SIZE);

        // agent.nextPosition — sari (play modunda)
        if (Application.isPlaying && agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(agent.nextPosition, SPHERE_SIZE * 1.3f);
        }

        // rb.position — mavi (play modunda, desync gorunur olmali)
        if (Application.isPlaying && rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rb.position, SPHERE_SIZE * 0.8f);
        }

        // Player'a beyaz cizgi + mesafe
        if (playerTransform != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, playerTransform.position);
#if UNITY_EDITOR
            float d = Vector3.Distance(transform.position, playerTransform.position);
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                Vector3.Lerp(transform.position, playerTransform.position, 0.5f),
                $"{d:F2}m");
#endif
        }
    }

    static string V(Vector3 v) => $"({v.x:F3},{v.y:F3},{v.z:F3})";
}
