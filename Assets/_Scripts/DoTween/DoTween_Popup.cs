using DG.Tweening;
using System;
using UnityEngine;

public class DoTween_Popup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private float popupFadeDuration = 0.25f;
    [SerializeField] private float popupScaleDuration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        StartHidden();
    }

    private void OnDestroy()
    {
        // Kill all tweens to prevent memory leaks
        canvasGroup.DOKill();
        rectTransform.DOKill();
    }

    private void StartHidden()
    {
        ResetState();
        gameObject.SetActive(false);
    }

    private void ResetState()
    {
        canvasGroup.alpha = 0f;
        rectTransform.localScale = Vector3.zero;
    }

    public void Show(Action callBack)
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

    public void Hide(Action callBack)
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
