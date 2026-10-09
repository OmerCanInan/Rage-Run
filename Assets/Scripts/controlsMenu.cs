using UnityEngine;

// Kontroller paneli. Şimdilik tek görevi "Varsayılana Dön" butonu.
public class controlsMenu : MonoBehaviour
{
    public void ResetToDefaults()
    {
        inputManager.ResetToDefaults();
    }
}
