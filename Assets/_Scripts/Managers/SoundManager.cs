using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region [Singleton]
    public static SoundManager Instance { get; private set; }

    private void SingleTon()
    {
        Instance = this;
    }

    private void EmptySingleton()
    {
        if (Instance != null)
            Instance = null;
    }
    #endregion

    [Header("SoundManager Config")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioClip[] buttonFXArray;
    [SerializeField] AudioClip uiPopupFX;

    private float fxVolume = 1f;

    private void Awake()
    {
        SingleTon();

        bgmSource = GetComponentInChildren<AudioSource>();
    }

    private void Start()
    {
        SoundEvents.Instance.OnPlayUIPopupFx += SoundEvents_OnPlayUIPopupFx;
        SoundEvents.Instance.OnPlayButtonFx += SoundEvents_OnPlayButtonFx;
    }

    private void OnDestroy()
    {
        SoundEvents.Instance.OnPlayUIPopupFx -= SoundEvents_OnPlayUIPopupFx;
        SoundEvents.Instance.OnPlayButtonFx -= SoundEvents_OnPlayButtonFx;
        EmptySingleton();
    }

    private void SoundEvents_OnPlayUIPopupFx(object sender, System.EventArgs e)
    {
        PlayUIPopupFx(fxVolume);
    }

    private void SoundEvents_OnPlayButtonFx(object sender, System.EventArgs e)
    {
        PlayButtonFx(fxVolume);
    }

    private void PlayUIPopupFx(float volume)
    {
        Vector2 position = Camera.main.transform.position;
        AudioSource.PlayClipAtPoint(uiPopupFX, position, volume);
    }

    private void PlayButtonFx(float volume)
    {
        Vector2 position = Camera.main.transform.position;
        AudioClip buttonFX = buttonFXArray[Random.Range(0, buttonFXArray.Length)];

        AudioSource.PlayClipAtPoint(buttonFX, position, volume);
    }
}
