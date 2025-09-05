using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Intro : MonoBehaviour
{
    [Header("Intro Config")]
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private TextMeshProUGUI pressSpacebarText;
    [SerializeField] private float typeDelay = 0.05f;
    [SerializeField] private float dotDelay = 0.3f;

    private int currentTextIndex = 0;
    private bool moveToNextText = false;
    private bool isTyping = false;
    private bool isWaiting = false;

    private Coroutine TypeIntroTextCoroutine;
    private Coroutine WaitToTypeNextIntroTextCoroutine;
    private WaitForSeconds wait_typeDelay;
    private WaitForSeconds wait_dotDelay;

    private string[] currentIntroTexts = new string[4];
    private readonly string[] introTexts =
    {
        "세상에 존재할 수 있는 모든 재앙이\n한 순간에 몰려왔다",
        "좀비, 외계인, 로봇, 변종 생명체까지...\n서로 다른 존재들이 한 도시를 집어 삼키고,\n인류는 속수무책으로 무너졌다",
        "하지만, 아직 희망은 있다",
    };

    private void OnEnable()
    {
        wait_typeDelay = new WaitForSeconds(typeDelay);
        wait_dotDelay = new WaitForSeconds(dotDelay);
    }

    private void Start()
    {
        HidePressSpacebarText();
        StartIntroScene();
    }

    private void Update()
    {
        ContinueIntroScene();
    }

    #region Internal Logic
    private void StartIntroScene()
    {
        // Start the first intro text typing
        this.StartCoroutineHelper(ref TypeIntroTextCoroutine, TypeIntroText(currentTextIndex));
    }

    private void ContinueIntroScene()
    {
        MoveToNextText();

        bool canContinue = moveToNextText && !isTyping && currentTextIndex < introTexts.Length;

        if (canContinue)
            this.StartCoroutineHelper(ref TypeIntroTextCoroutine, TypeIntroText(currentTextIndex));

        // Load the game scene if all intro texts have been displayed   
        else if (currentTextIndex >= introTexts.Length)
            LoadGameScene();
    }

    private void MoveToNextText()
    {
        // Check for space key press to move to the next text
        if (IsSpacebarPressed())
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

    private void LoadGameScene()
    {
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private IEnumerator TypeIntroText(int currentIntroTextIndex)
    {
        moveToNextText = false;
        isTyping = true;

        introText.text = string.Empty;

        foreach (char introTextchar in introTexts[currentIntroTextIndex])
        {
            introText.text += introTextchar;
            yield return wait_typeDelay;
        }

        isTyping = false;

        // waiting for the next text
        isWaiting = true;
        SetCurrentIntroTextArray(currentIntroTextIndex);
        WaitingToTypeNextText();
        ShowPressSpacebarText();
    }

    private void SetCurrentIntroTextArray(int index)
    {
        string baseText = introTexts[index].TrimEnd('.');

        currentIntroTexts[0] = baseText;
        currentIntroTexts[1] = baseText + ".";
        currentIntroTexts[2] = baseText + "..";
        currentIntroTexts[3] = baseText + "...";
    }

    private void WaitingToTypeNextText()
    {
        this.StartCoroutineHelper(ref WaitToTypeNextIntroTextCoroutine, WaitToTypeNextIntroText());
    }

    private IEnumerator WaitToTypeNextIntroText()
    {
        int textIndex = 0;

        while (isWaiting)
        {
            // LoadingText를 업데이트
            introText.text = currentIntroTexts[textIndex];

            // 텍스트 인덱스를 순환
            textIndex = (textIndex + 1) % currentIntroTexts.Length;

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