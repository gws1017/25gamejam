using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*****************************************************************************************
 * 파일: UI_Ending.cs
 * 역할: 스토리 모드 엔딩 컷씬 (UI_Intro.cs와 동일한 타이핑 연출 구조)
 * 에디터 세팅:
 *   - endingText / pressSpacebarText: 인트로와 동일하게 TMP 텍스트 연결
 *   - endingImage: 문구별 삽화를 보여줄 Image (선택 사항, 비워두면 그림 없이 텍스트만 진행)
 *   - endingSprites: endingTexts와 같은 순서/개수로 스프라이트 등록 (비어있는 인덱스는 무시됨)
 * 동작:
 *   - 인트로와 동일하게 스페이스바로 한 문장씩 타이핑 진행
 *   - 모든 문구가 끝나면 MainMenuScene으로 이동 (인트로는 GameScene으로 이동하는 것과 대칭)
 *****************************************************************************************/

public class UI_Ending : MonoBehaviour
{
    [Header("Ending Config")]
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private TextMeshProUGUI pressSpacebarText;
    [SerializeField] private Image endingImage;
    [SerializeField] private float typeDelay = 0.05f;
    [SerializeField] private float dotDelay = 0.3f;

    private int currentTextIndex = 0;
    private bool moveToNextText = false;
    private bool isTyping = false;
    private bool isWaiting = false;

    private Coroutine TypeEndingTextCoroutine;
    private Coroutine WaitToTypeNextEndingTextCoroutine;
    private WaitForSeconds wait_typeDelay;
    private WaitForSeconds wait_dotDelay;

    private string[] currentEndingTexts = new string[4];

    // 인트로("재앙이 몰려왔다... 인류는 무너졌다... 하지만 희망은 있다")를 회수하는 엔딩 문구.
    // 보스 고유명사는 넣지 않음(추후 보스 종류가 추가/변경될 수 있음). 플레이어에게 말을 거는 톤.
    private readonly string[] endingTexts =
    {
        "마지막까지 버티던 어둠조차,\n결국 당신 앞에 무릎을 꿇었고,",
        "늘 곁에서 함께해준 작은 빛도,\n이제야 편히 눈을 감습니다",
        "무너졌던 거리마다,\n사람들이 하나둘 고개를 내밉니다",
        "세상은 아직 폐허투성이지만 —\n이제, 다시 걸어갈 수 있기를",
    };

    // endingTexts와 동일한 순서/개수로 등록. 아직 그림이 없으면 비워둬도 됨(그 인덱스는 그냥 스킵).
    [SerializeField] private Sprite[] endingSprites;

    private void OnEnable()
    {
        wait_typeDelay = new WaitForSeconds(typeDelay);
        wait_dotDelay = new WaitForSeconds(dotDelay);
    }

    private void Start()
    {
        HidePressSpacebarText();
        StartEndingScene();
    }

    private void Update()
    {
        ContinueEndingScene();
    }

    #region Internal Logic
    private void StartEndingScene()
    {
        this.StartCoroutineHelper(ref TypeEndingTextCoroutine, TypeEndingText(currentTextIndex));
    }

    private void ContinueEndingScene()
    {
        MoveToNextText();

        bool canContinue = moveToNextText && !isTyping && currentTextIndex < endingTexts.Length;

        if (canContinue)
            this.StartCoroutineHelper(ref TypeEndingTextCoroutine, TypeEndingText(currentTextIndex));

        // 모든 엔딩 문구를 다 보여줬다면 메인메뉴로 이동
        else if (currentTextIndex >= endingTexts.Length)
            LoadMainMenuScene();
    }

    private void MoveToNextText()
    {
        if (IsSpacebarPressed() && !isTyping)
        {
            moveToNextText = true;
            currentTextIndex++;

            isWaiting = false;
            HidePressSpacebarText();
        }
    }

    private bool IsSpacebarPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    private void LoadMainMenuScene()
    {
        SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
    }

    private IEnumerator TypeEndingText(int currentEndingTextIndex)
    {
        moveToNextText = false;
        isTyping = true;

        SetCurrentImage(currentEndingTextIndex);

        endingText.text = string.Empty;

        foreach (char endingTextChar in endingTexts[currentEndingTextIndex])
        {
            endingText.text += endingTextChar;
            yield return wait_typeDelay;
        }

        isTyping = false;

        isWaiting = true;
        SetCurrentEndingTextArray(currentEndingTextIndex);
        WaitingToTypeNextText();
        ShowPressSpacebarText();
    }

    private void SetCurrentImage(int index)
    {
        if (endingImage == null) return;
        if (endingSprites == null || index < 0 || index >= endingSprites.Length) return;

        Sprite sprite = endingSprites[index];
        if (sprite == null) return; // 아직 등록 안 된 그림은 이전 화면 유지

        endingImage.sprite = sprite;
        endingImage.enabled = true;
    }

    private void SetCurrentEndingTextArray(int index)
    {
        string baseText = endingTexts[index].TrimEnd('.');

        currentEndingTexts[0] = baseText;
        currentEndingTexts[1] = baseText + ".";
        currentEndingTexts[2] = baseText + "..";
        currentEndingTexts[3] = baseText + "...";
    }

    private void WaitingToTypeNextText()
    {
        this.StartCoroutineHelper(ref WaitToTypeNextEndingTextCoroutine, WaitToTypeNextEndingText());
    }

    private IEnumerator WaitToTypeNextEndingText()
    {
        int textIndex = 0;

        while (isWaiting)
        {
            endingText.text = currentEndingTexts[textIndex];
            textIndex = (textIndex + 1) % currentEndingTexts.Length;

            yield return wait_dotDelay;
        }
    }

    private void ShowPressSpacebarText()
    {
        pressSpacebarText.gameObject.SetActive(true);
    }

    private void HidePressSpacebarText()
    {
        pressSpacebarText.gameObject.SetActive(false);
    }
    #endregion
}
