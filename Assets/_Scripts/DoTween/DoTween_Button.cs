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
        // 초기화 작업
        Init();
    }

    protected virtual void OnDestroy()
    {
        // 모든 Tween 중지
        KillAllTweens();
    }

    protected virtual void Init() { }

    protected virtual void KillAllTweens()
    {
        // 모든 Tween 중지
        onPointerEnterTween?.Kill();
        onPointerExitTween?.Kill();
        onButtonClickTween?.Kill();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // 이전 트윈이 있다면 중지
        onPointerEnterTween?.Kill();
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        // 이전 트윈이 있다면 중지
        onPointerExitTween?.Kill();
    }

    public virtual void OnButtonClick()
    {
        // 이전 트윈이 있다면 중지
        onButtonClickTween?.Kill();
    }

}