using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Açılışta ekip logosunu yavaşça gösterir, sonra ana menüye geçer.
// Herhangi bir tuşa / tıka / gamepad A tuşuna basılırsa atlanır.
public class splashScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup logoGroup;      // kempleLogo üzerindeki Canvas Group
    [SerializeField] private float fadeInTime = 3f;    // Belirme süresi
    [SerializeField] private float holdTime = 1.5f;      // Bekleme süresi
    [SerializeField] private float fadeOutTime = 0.3f;   // Kaybolma süresi
    [SerializeField] private string nextScene = "mainMenu";         

    private bool skipRequested;

    private IEnumerator Start()
    {
        
        settingsMenu.ApplySavedSettings();

        logoGroup.alpha = 0f;

        // Belirme
        yield return Fade(0f, 1f, fadeInTime);

        // Bekleme (atlanabilir)
        float t = 0f;
        while (t < holdTime && !skipRequested)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Kaybolma (atlandıysa daha hızlı)
        yield return Fade(logoGroup.alpha, 0f, skipRequested ? 0.2f : fadeOutTime);

        SceneManager.LoadScene(nextScene);
    }

    private void Update()
    {
        if (AnyPressed()) skipRequested = true;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            if (skipRequested && to > from) yield break; // Belirirken atlanırsa beklemeden geç
            t += Time.unscaledDeltaTime;
            logoGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        logoGroup.alpha = to;
    }

    private bool AnyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
#else
        return Input.anyKeyDown;
#endif
    }
}