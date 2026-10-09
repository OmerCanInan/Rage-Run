using UnityEngine;
using UnityEngine.SceneManagement;

// Ana menüdeki panelleri yönetir: Ana Menü, Bölüm Seçimi, Ayarlar, Kontroller.
// Boş bir "menuManager" objesine ekle, panelleri Inspector'dan bağla.
public class mainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Sahneler")]
    [SerializeField] private string firstLevelScene = "level01";

    private void Start()
    {
        ShowMain(); // Sahne açılınca sadece ana panel görünsün
    }

    // --- Panel geçişleri (butonlara bağlanır) ---
    public void ShowMain() => ShowOnly(mainPanel);
    public void ShowLevelSelect() => ShowOnly(levelSelectPanel);
    public void ShowSettings() => ShowOnly(settingsPanel);
    public void ShowControls() => ShowOnly(controlsPanel);

    // Verilen paneli açar, diğerlerini kapatır
    private void ShowOnly(GameObject panel)
    {
        SetPanel(mainPanel, panel);
        SetPanel(levelSelectPanel, panel);
        SetPanel(settingsPanel, panel);
        SetPanel(controlsPanel, panel);
    }

    // Inspector'da boş bırakılan panel hata vermesin
    private static void SetPanel(GameObject target, GameObject active)
    {
        if (target != null) target.SetActive(target == active);
    }

    // --- Butonlar ---
    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelScene);
    }

    public void QuitGame()
    {
        Application.Quit(); // Gerçek oyunda kapatır
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Editörde Play modunu durdurur
#endif
    }
}
