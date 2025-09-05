using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu_CharactersAnimation : MonoBehaviour
{
    [Header("NPC Images")]
    [SerializeField] private Image mainNPCImage;
    [SerializeField] private Image subNPCImage;

    private RectTransform mainNPCRectTransform;
    private Vector2 mainNPCOriginalPosition;
    private Tween mainNPCbreathingTween;
    private int mainNPC_YOffset = 10;

    // SubNPC Breathing Animation Config
    private RectTransform subNPCRectTransform;
    private Vector2 subNPCOoriginalPosition;
    private Tween subNPCbreathingTween;
    private int subNPC_YOffset = 7;

    private void Start()
    {
        NPCInit();

        StartNPCBreathingAnimation();
    }

    private void NPCInit()
    {
        // NPC RectTransform 설정
        if (mainNPCImage != null)
            mainNPCRectTransform = mainNPCImage.GetComponent<RectTransform>();

        if (subNPCImage != null)
            subNPCRectTransform = subNPCImage.GetComponent<RectTransform>();

        // NPC Original Position 설정
        if (mainNPCRectTransform != null)
            mainNPCOriginalPosition = mainNPCRectTransform.anchoredPosition;

        if (subNPCRectTransform != null)
            subNPCOoriginalPosition = subNPCRectTransform.anchoredPosition;

        Debug.Log(subNPCOoriginalPosition);
    }

    private void StartNPCBreathingAnimation()
    {
        if (mainNPCRectTransform == null) return;
        if (subNPCRectTransform == null) return;

        // Tween 비워주고 시작
        // StopNPCBreathingAnimation();    

        // 메인 NPC 호흡 애니메이션 시작
        mainNPCbreathingTween = mainNPCRectTransform.DOAnchorPosY(mainNPCOriginalPosition.y - mainNPC_YOffset, 1.5f).
                                                     SetEase(Ease.InOutSine).
                                                     SetLoops(-1, LoopType.Yoyo);

        // 서브 NPC 호흡 애니메이션 시작
        subNPCbreathingTween = subNPCRectTransform.DOAnchorPosY(subNPCOoriginalPosition.y - subNPC_YOffset, 1f).
                                                     SetEase(Ease.InOutSine).
                                                     SetLoops(-1, LoopType.Yoyo);
    }

    private void StopNPCBreathingAnimation()
    {
        // 메인 NPC 호흡 애니메이션 정지
        if (mainNPCbreathingTween != null)
        {
            mainNPCbreathingTween.Kill();
            mainNPCbreathingTween = null;
        }

        // 서브 NPC 호흡 애니메이션 정지
        if (subNPCbreathingTween != null)
        {
            subNPCbreathingTween.Kill();
            subNPCbreathingTween = null;
        }

        // NPC 위치 초기화
        if (mainNPCRectTransform != null)
            mainNPCRectTransform.anchoredPosition = mainNPCOriginalPosition;

        if (subNPCRectTransform != null)
            subNPCRectTransform.anchoredPosition = subNPCOoriginalPosition;
    }

}
