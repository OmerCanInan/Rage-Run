using UnityEngine;
using UnityEngine.SceneManagement;

// Ana menüdeki panelleri yönetir: Ana Menü, Bölüm Seçimi, Ayarlar.
// Boş bir "menuManager" objesine ekle, panelleri Inspector'dan bağla.
public class mainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

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

    // Verilen paneli açar, diğer ikisini kapatır
    private void ShowOnly(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        levelSelectPanel.SetActive(panel == levelSelectPanel);
        settingsPanel.SetActive(panel == settingsPanel);
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
