using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Loading : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI LoadingText;

    private string sceneToLoad;
    private WaitForSeconds waitForSeconds;

    private void Start()
    {
        // LoadTextAnimation 코루틴에서 사용할 WaitForSeconds 객체를 캐싱
        waitForSeconds = new WaitForSeconds(0.5f);

        StartCoroutine(LoadScene(SceneLoader.GetTargetScene()));
    }

    private IEnumerator LoadScene(string sceneToLoad)
    {
        AsyncOperation asyncSceneLoad = SceneManager.LoadSceneAsync(sceneToLoad);

        // asyncSceneLoad가 완료될 때까지 LoadTexAnimation을 실행한다.
        yield return StartCoroutine(LoadTextAnimation(asyncSceneLoad));
    }

    private IEnumerator LoadTextAnimation(AsyncOperation asyncSceneLoad)
    {
        string[] loadingTexts = { "로딩중", "로딩중.", "로딩중..", "로딩중..." };
        int textIndex = 0;

        while (!asyncSceneLoad.isDone)
        {
            // LoadingText를 업데이트
            LoadingText.text = loadingTexts[textIndex];

            // 텍스트 인덱스를 순환 (loadingTexts 배열의 길이: 4)
            textIndex = (textIndex + 1) % loadingTexts.Length;

            yield return new WaitForSeconds(0.5f);
        }

        LoadingText.text = "Loading Complete!";
    }
}