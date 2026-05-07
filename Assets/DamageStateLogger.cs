using UnityEngine;

// GECICI DIAGNOSTIC — test bittikten sonra sil.
// player_damage state'ine ekle (Animator Inspector → Add Behaviour).
public class DamageStateLogger : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"[ANIM] player_damage state ENTERED at {Time.time:F3}");
    }
}
