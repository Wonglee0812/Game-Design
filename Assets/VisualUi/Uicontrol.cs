using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

// [VI] Thêm ISelectHandler và IDeselectHandler để nhận tín hiệu từ Gamepad
// [EN] Add ISelectHandler and IDeselectHandler to receive Gamepad signals
public class Uicontrol : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler // <--- MỚI / NEW
{
    [Header("Animation Settings")]
    [SerializeField] private float animDuration = 0.2f;
    [SerializeField] private float hoverScaleMultiplier = 1.2f;

    private Vector3 normalScale;
    private bool isSelected;

    void Awake()
    {
        normalScale = transform.localScale;
    }

    // --- LOGIC CHUNG CHO CẢ CHUỘT VÀ TAY CẦM / COMMON LOGIC FOR MOUSE & GAMEPAD ---

    private void PlayEnterAnimation()
    {
        if (isSelected) return;
        isSelected = true;
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        transform.DOScale(normalScale * hoverScaleMultiplier, animDuration).SetEase(Ease.OutBack);
        transform.DOPunchRotation(new Vector3(0, 0, 5), 0.15f, 20, 1);
    }

    private void PlayExitAnimation()
    {
        if (!isSelected) return;
        isSelected = false;
        transform.DOKill();
        transform.localRotation = Quaternion.identity;
        transform.DOScale(normalScale, animDuration).SetEase(Ease.OutBack);
    }

    // --- INTERFACES ---

    // [VI] Khi dùng chuột di qua / [EN] When mouse hovers
    public void OnPointerEnter(PointerEventData eventData) => PlayEnterAnimation();
    public void OnPointerExit(PointerEventData eventData) => PlayExitAnimation();

    // [VI] Khi dùng Gamepad chọn nút / [EN] When Gamepad selects the button
    public void OnSelect(BaseEventData eventData) => PlayEnterAnimation();
    public void OnDeselect(BaseEventData eventData) => PlayExitAnimation();

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(normalScale, animDuration).SetEase(Ease.OutBack);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(normalScale * hoverScaleMultiplier, animDuration).SetEase(Ease.OutBack);
    }
}