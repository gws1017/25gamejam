using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoTween_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected Tween onPointerEnterTween;
    protected Tween onPointerExitTween;
    protected Tween onButtonClickTween;

    protected virtual void Awake()
    {
        // �ʱ�ȭ �۾�
        Init();
    }

    protected virtual void OnDestroy()
    {
        // ��� Tween ����
        KillAllTweens();
    }

    protected virtual void Init() { }

    protected virtual void KillAllTweens()
    {
        // ��� Tween ����
        onPointerEnterTween?.Kill();
        onPointerExitTween?.Kill();
        onButtonClickTween?.Kill();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // ���� Ʈ���� �ִٸ� ����
        onPointerEnterTween?.Kill();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        // ���� Ʈ���� �ִٸ� ����
        onPointerExitTween?.Kill();
    }

    public virtual void OnButtonClick()
    {
        // ���� Ʈ���� �ִٸ� ����
        onButtonClickTween?.Kill();
    }

}
