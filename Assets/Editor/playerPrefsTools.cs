using UnityEditor;
using UnityEngine;

// Sadece editörde çalışan yardımcı araç. Oyunun içine dahil edilmez.
// Bu dosya mutlaka "Assets/Editor/" klasöründe durmalı.
// Kullanım: Unity üst menüsü -> Tools -> PlayerPrefs
public static class playerPrefsTools
{
    [MenuItem("Tools/PlayerPrefs/Kayıtlı Ayarları Göster")]
    private static void ShowPrefs()
    {
        // HasKey: bu anahtarla bir değer kaydedilmiş mi?
        string volume = PlayerPrefs.HasKey("MasterVolume")
            ? PlayerPrefs.GetFloat("MasterVolume").ToString("0.00")
            : "kayıt yok (varsayılan 0.80 kullanılır)";

        string fullscreen = PlayerPrefs.HasKey("Fullscreen")
            ? (PlayerPrefs.GetInt("Fullscreen") == 1 ? "Açık" : "Kapalı")
            : "kayıt yok (varsayılan Açık kullanılır)";

        Debug.Log($"[PlayerPrefs] Ses: {volume}  |  Tam ekran: {fullscreen}");
    }

    [MenuItem("Tools/PlayerPrefs/Tüm Kayıtları Sil")]
    private static void DeleteAllPrefs()
    {
        if (EditorUtility.DisplayDialog("PlayerPrefs",
                "Tüm kayıtlı ayarlar silinecek. Emin misin?", "Sil", "Vazgeç"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[PlayerPrefs] Tüm kayıtlar silindi.");
        }
    }
}