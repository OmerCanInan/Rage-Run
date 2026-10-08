using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Ayarlardaki dil seçim menüsü. TMP Dropdown objesine eklemen yeterli;
// seçenekleri CSV'deki dillerden kendisi oluşturur, Inspector'da bağlantı gerekmez.
[RequireComponent(typeof(TMP_Dropdown))]
public class languageDropdown : MonoBehaviour
{
    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(OnLanguageSelected);
    }

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnLanguageSelected);
    }

    // Seçenekleri doldur: her dil kendi adıyla görünür (Türkçe, English...)
    private void Rebuild()
    {
        IReadOnlyList<string> languages = localizationManager.Languages;

        var options = new List<string>();
        foreach (string lang in languages)
            options.Add(localizationManager.GetForLanguage(lang, "language_name"));

        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        int current = 0;
        for (int i = 0; i < languages.Count; i++)
            if (languages[i] == localizationManager.CurrentLanguage) current = i;

        dropdown.SetValueWithoutNotify(current);
        dropdown.RefreshShownValue();
    }

    private void OnLanguageSelected(int index)
    {
        localizationManager.SetLanguage(localizationManager.Languages[index]);
    }
}
