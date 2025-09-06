using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Image progressBar;

    private float targetProgress;
    private string sceneToLoad;
    private Coroutine loadingDotsCoroutine;
    private WaitForSeconds waitForSeconds;

    private float barSpeed = 1.5f;
    private float minShownTime = 2f;
    private float dotsInterval = 0.5f;  

    private void Start()
    {
        // LoadTextAnimation 코루틴에서 사용할 WaitForSeconds 객체를 캐싱
        waitForSeconds = new WaitForSeconds(0.5f);

        ResetProgressBar();
        StartCoroutine(LoadScene(SceneLoader.GetTargetScene()));
    }

    private void Update()
    {
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        float maxDelta = barSpeed * Time.deltaTime;
        progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, targetProgress, maxDelta);
    }

    private IEnumerator LoadScene(string sceneToLoad)
    {
        // asyncSceneLoad가 완료될 때까지 LoadTexAnimation을 실행한다.
        this.StartCoroutineHelper(ref loadingDotsCoroutine, LoadingDots());

        // async로 씬 로드 시작
        AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // 씬이 완전히 로드될 때까지 자동으로 씬 전환을 막음
        asyncSceneLoad.allowSceneActivation = false;

        float shownTimer = 0f;

        // 엔진이 씬 로드를 하는 동안 : 
        while (asyncSceneLoad.progress < 0.9f)
        {
            // 씬 로드 진행률을 targetProgress에 반영 (0.0 ~ 0.9) -> (0.0 ~ 1.0)
            targetProgress = Mathf.Clamp01(asyncSceneLoad.progress / 0.9f);
            shownTimer += Time.deltaTime;
            yield return null;
        }

        // 엔진이 씬 로드가 완료되었을 때 :
        targetProgress = 1f;
        while (progressBar.fillAmount < 0.999f || shownTimer < minShownTime)
        {
            shownTimer += Time.deltaTime;
            yield return null;
        }

        // 씬 전환을 허용
        asyncSceneLoad.allowSceneActivation = true;
    }

    private void ResetProgressBar()
    {
        targetProgress = 0f;
        progressBar.fillAmount = 0f;

        if (loadingText != null) 
            loadingText.text = "로딩중";
    }

    private IEnumerator LoadingDots()
    {
        string baseText = "로딩중";
        string[] frames = { "", ".", "..", "..." };
        int textIndex = 0;

        while (true)
        {
            // LoadingText를 업데이트
            loadingText.text = baseText + frames[textIndex];

            // 텍스트 인덱스를 순환 (loadingTexts 배열의 길이: 4)
            textIndex = (textIndex + 1) % frames.Length;

            yield return new WaitForSeconds(dotsInterval);
        }
    }
}