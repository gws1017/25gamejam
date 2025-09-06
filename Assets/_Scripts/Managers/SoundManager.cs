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

    [Header("Volumes (0..1)")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f; // applied to everything
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    // PlayerPrefs keys (edit as needed)
    private const string PLAYER_PREFS_MASTER = "volume_master";
    private const string PLAYER_PREFSPP_BGM = "volume_bgm";
    private const string PLAYER_PREFS_SFX = "volume_sfx";

    #region Getters
    // Public API for getting volumes
    public float GetMasterVolume() => masterVolume;
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    #endregion

    #region Setters
    // Public API for setting volumes (0..1)
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolumes();
    }
    #endregion

    private void Awake()
    {
        SingleTon();

        bgmSource = GetComponentInChildren<AudioSource>();

        // Load saved volumes
        LoadVolumes();

        // Apply volumes to live sources now
        ApplyVolumes();
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

    /// <summary>Play/loop background music on the managed BGM AudioSource.</summary>
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (!bgmSource || !clip) return; 

        bgmSource.clip = clip;
        bgmSource.loop = loop;

        ApplyVolumes();
        bgmSource.Play();
    }

    /// <summary>Stop the background music.</summary>
    public void StopBGM()
    {
        if (!bgmSource) return;

        bgmSource.Stop();
    }

    #region Internal Logic
    private void SoundEvents_OnPlayUIPopupFx(object sender, System.EventArgs e)
    {
        PlayUIPopupFx(sfxVolume);
    }

    private void SoundEvents_OnPlayButtonFx(object sender, System.EventArgs e)
    {
        PlayButtonFx(sfxVolume);
    }

    private void ApplyVolumes()
    {
        if (bgmSource)
        {
            // Final audible volume for BGM = master * bgm
            bgmSource.volume = masterVolume * bgmVolume;
        }
    }

    private void PlayUIPopupFx(float volume)
    {
        if (!uiPopupFX) return;

        Vector2 position = Camera.main.transform.position;
        
        PlaySFX(uiPopupFX, volume);
    }

    private void PlayButtonFx(float volume)
    {
        if (buttonFXArray.Length == 0 || buttonFXArray == null) return;

        Vector2 position = Camera.main.transform.position;
        AudioClip buttonFX = buttonFXArray[Random.Range(0, buttonFXArray.Length)];

        PlaySFX(buttonFX, volume);
    }

    /// <summary>Play a one-shot SFX at the main camera position.</summary>
    private void PlaySFX(AudioClip audipClip, float volumeScale = 1f)
    {
        if (!audipClip) return;

        Transform cameraTransform = Camera.main.transform;

        float finalVolume = Mathf.Clamp01(masterVolume * sfxVolume * volumeScale);
        AudioSource.PlayClipAtPoint(audipClip, cameraTransform.position, finalVolume);
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_MASTER, masterVolume);
        PlayerPrefs.SetFloat(PLAYER_PREFSPP_BGM, bgmVolume);
        PlayerPrefs.SetFloat(PLAYER_PREFS_SFX, sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        if (PlayerPrefs.HasKey(PLAYER_PREFS_MASTER)) 
            masterVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_MASTER, masterVolume);

        if (PlayerPrefs.HasKey(PLAYER_PREFSPP_BGM)) 
            bgmVolume = PlayerPrefs.GetFloat(PLAYER_PREFSPP_BGM, bgmVolume);

        if (PlayerPrefs.HasKey(PLAYER_PREFS_SFX)) 
            sfxVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_SFX, sfxVolume);
    }

    // Keep inspector changes in play mode consistent
    private void OnValidate()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        if (Application.isPlaying) ApplyVolumes();
    }
    #endregion
}
