using UnityEngine;
using UnityEngine.UI;

// Ayarlar paneli: ses seviyesi ve tam ekran. Ayarlar PlayerPrefs ile kaydedilir.
public class settingsMenu : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    // PlayerPrefs'te ayarların kaydedildiği anahtar isimleri
    private const string VolumeKey = "MasterVolume";
    private const string FullscreenKey = "Fullscreen";
    private const float DefaultVolume = 0.8f;

    // Panel her açıldığında kontroller kayıtlı değerleri göstersin
    private void OnEnable()
    {
        volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(VolumeKey, DefaultVolume));
        fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(FullscreenKey, 1) == 1);
    }

    // Slider -> On Value Changed -> Dynamic float altından seç
    public void SetVolume(float value)
    {
        AudioListener.volume = value; // Oyundaki tüm seslerin ana seviyesi (0 = sessiz, 1 = tam)
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    // Toggle -> On Value Changed -> Dynamic bool altından seç
    public void SetFullscreen(bool isOn)
    {
        Screen.fullScreen = isOn; // Editörde etkisi yok, sadece build'de çalışır
        PlayerPrefs.SetInt(FullscreenKey, isOn ? 1 : 0);
    }

    // Oyun açılırken splashScreen tarafından çağrılır
    public static void ApplySavedSettings()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
    }
}