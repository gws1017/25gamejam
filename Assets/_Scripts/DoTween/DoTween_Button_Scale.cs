using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoTween_Button_Scale : DoTween_Button
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private RectTransform buttonTransform;

    [Header("Settings")]
    [SerializeField] private float scaleUpFactor = 1.05f;
    [SerializeField] private float tweenDuration = 0.2f;
    [SerializeField] private Ease easeType = Ease.OutBack;

    private Vector3 originalScale;

    protected override void Awake()
    {
        Init();
    }

    protected override void Init()
    {
        // ��ư ������Ʈ �׷�
        if (!button) button = GetComponent<Button>();
        if (!buttonTransform) buttonTransform = GetComponent<RectTransform>();

        // ���� ������ ����
        originalScale = buttonTransform.localScale;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        // ��ư�� ���콺 ���� �� ������ ��
        onPointerEnterTween = buttonTransform.DOScale(originalScale * scaleUpFactor, tweenDuration).
                                              SetEase(easeType).
                                              SetUpdate(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // ��ư���� ���콺 �ƿ� �� ���� �����Ϸ� ����
        onPointerExitTween = buttonTransform.DOScale(originalScale, tweenDuration).
                                             SetEase(easeType).
                                             SetUpdate(true);
    }

    public override void OnButtonClick()
    {
        base.OnButtonClick();

        // Over 0.2 seconds, make the UI element quickly expand by about 15%, wobble 10 times with full bounce, then return to its normal size
        // and do it even if the game is paused
        onButtonClickTween =  buttonTransform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 10, 1).
                                              SetUpdate(true);
    }
}
