using TMPro;
using UnityEngine;

// Bir TextMeshPro yazısını seçili dile göre otomatik doldurur.
// Kullanım: yazı objesine ekle, Inspector'daki "Key" alanına CSV'deki anahtarı yaz (örn. menu_play).
[RequireComponent(typeof(TMP_Text))]
public class localizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text label;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        localizationManager.OnLanguageChanged += Refresh; // Dil değişince haber al
        Refresh();
    }

    private void OnDisable()
    {
        localizationManager.OnLanguageChanged -= Refresh; // Obje kapanınca dinlemeyi bırak
    }

    // Koddan anahtar değiştirmek gerekirse (örn. bölüm adları)
    public void SetKey(string newKey)
    {
        key = newKey;
        Refresh();
    }

    private void Refresh()
    {
        if (label == null) label = GetComponent<TMP_Text>();
        label.text = localizationManager.Get(key);
    }
}
