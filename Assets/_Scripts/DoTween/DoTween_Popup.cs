using DG.Tweening;
using System;
using UnityEngine;

public class DoTween_Popup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] protected float popupFadeDuration = 0.25f;
    [SerializeField] protected float popupScaleDuration = 0.3f;
    [SerializeField] protected Ease showEase = Ease.OutBack;
    [SerializeField] protected Ease hideEase = Ease.InBack;

    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    protected Vector3 originalScale;

    protected void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        StartHidden();
    }

    protected void OnDestroy()
    {
        // Kill all tweens to prevent memory leaks
        canvasGroup.DOKill();
        rectTransform.DOKill();
    }

    protected void StartHidden()
    {
        ResetState();
        gameObject.SetActive(false);
    }

    protected void ResetState()
    {
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        // Reset state instantly
        ResetState();

        // Animate fade in and scale up
        canvasGroup.DOFade(1f, popupFadeDuration).
                    SetEase(showEase);

        rectTransform.DOScale(originalScale, popupScaleDuration).
                      SetEase(showEase).
                      SetUpdate(true);
    }

    public virtual void Show(Action callBack)
    {
        // Invoke callback if provided
        callBack?.Invoke();
        gameObject.SetActive(true);

        // Reset state instantly
        ResetState();

        // Animate fade in and scale up
        canvasGroup.DOFade(1f, popupFadeDuration).
                    SetEase(showEase);

        rectTransform.DOScale(originalScale, popupScaleDuration).
                      SetEase(showEase).
                      SetUpdate(true);
    }

    public virtual void Hide(Action callBack)
    {
        // Animate fade out and scale down
        canvasGroup.DOFade(0f, popupFadeDuration).
                    SetUpdate(true);

        rectTransform.DOScale(Vector3.zero, popupScaleDuration).
                      SetEase(hideEase).
                      SetUpdate(true).
                      OnComplete(() =>
                      {
                          gameObject.SetActive(false);

                          callBack?.Invoke();

                          // Reset state after hiding
                          ResetState(); 
                      });  
    }
}
