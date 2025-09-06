using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        InitializeSliders();
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSliders()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = SoundManager.Instance.GetMasterVolume();

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = SoundManager.Instance.GetBGMVolume();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = SoundManager.Instance.GetSFXVolume();
    }

    private void SubscribeToEvents()
    {
        // 볼륨 변경 리스너 추가 - 슬라이더 조절하면 바로 적용
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void UnsubscribeFromEvents()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    // 볼륨 변경 이벤트 핸들러들 - 슬라이더 움직이면 즉시 적용
    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);

        // SFX 테스트를 위해 간단한 사운드 재생 (SFX가 있는 경우)
        // SoundManager.Instance.PlaySFX(SFXName.ButtonClick); // 예시
    }

    // 외부에서 슬라이더 값을 업데이트할 때 사용
    public void UpdateSliders()
    {
        InitializeSliders();
    }
}


