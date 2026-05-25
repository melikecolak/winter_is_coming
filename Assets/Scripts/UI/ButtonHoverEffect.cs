using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float hoverScale     = 1.1f;
    [SerializeField] float animationSpeed = 10f;

    Vector3 targetScale;

    void Awake() => targetScale = Vector3.one;

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData _) => targetScale = Vector3.one * hoverScale;
    public void OnPointerExit(PointerEventData _)  => targetScale = Vector3.one;
}
