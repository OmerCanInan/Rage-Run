using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Oyunun dil sistemi. Metinleri Assets/Resources/localization.csv dosyasından okur.
// Sahneye eklenmez; her yerden localizationManager.Get("anahtar") şeklinde çağrılır.
//
// CSV düzeni:  key,tr,en      <- ilk satır: "key" + dil kodları (yeni dil = yeni sütun)
//              menu_play,Oyna,Play
// "#" ile başlayan satırlar açıklamadır, okunmaz.
public static class localizationManager
{
    private const string CsvName = "localization";   // Resources klasöründeki dosya adı (uzantısız)
    private const string LanguageKey = "Language";   // PlayerPrefs'te seçili dilin anahtarı
    private const string FallbackLanguage = "en";    // Bir çeviri eksikse bu dil gösterilir

    // Dil değiştiğinde tetiklenir; ekrandaki tüm yazılar bunu dinleyip kendini günceller
    public static event Action OnLanguageChanged;

    private static Dictionary<string, Dictionary<string, string>> table; // dil -> (anahtar -> metin)
    private static List<string> languages;                               // CSV'deki sırayla dil kodları
    private static string currentLanguage;

    public static string CurrentLanguage { get { EnsureLoaded(); return currentLanguage; } }
    public static IReadOnlyList<string> Languages { get { EnsureLoaded(); return languages; } }

    // Editörde her Play'e basışta sıfırdan başlasın (eski veriler kalmasın)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        table = null;
        languages = null;
        currentLanguage = null;
        OnLanguageChanged = null;
    }

    // --- Dışarıdan kullanılanlar ---

    // Seçili dildeki metni verir
    public static string Get(string key)
    {
        EnsureLoaded();
        return GetForLanguage(currentLanguage, key);
    }

    // Belirli bir dildeki metni verir (dil menüsü her dilin kendi adını göstermek için kullanır)
    public static string GetForLanguage(string language, string key)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(key)) return "";

        if (table.TryGetValue(language, out var texts) && texts.TryGetValue(key, out var text) && text.Length > 0)
            return text;

        // Çeviri yoksa İngilizcesini göster
        if (table.TryGetValue(FallbackLanguage, out var fallback) && fallback.TryGetValue(key, out text) && text.Length > 0)
            return text;

        Debug.LogWarning($"[Dil] '{key}' anahtarı localization.csv içinde bulunamadı.");
        return "#" + key; // Ekranda göze batsın, eksik olduğu hemen fark edilsin
    }

    // Dili değiştirir, kaydeder ve tüm yazılara haber verir
    public static void SetLanguage(string language)
    {
        EnsureLoaded();
        if (!table.ContainsKey(language))
        {
            Debug.LogWarning($"[Dil] '{language}' dili CSV'de yok.");
            return;
        }
        if (language == currentLanguage) return;

        currentLanguage = language;
        PlayerPrefs.SetString(LanguageKey, language);
        OnLanguageChanged?.Invoke();
    }

    // --- Yükleme ---

    private static void EnsureLoaded()
    {
        if (table == null) Load();
    }

    private static void Load()
    {
        table = new Dictionary<string, Dictionary<string, string>>();
        languages = new List<string>();

        TextAsset csv = Resources.Load<TextAsset>(CsvName);
        List<List<string>> rows = csv != null ? ParseCsv(csv.text) : new List<List<string>>();

        if (rows.Count > 0)
        {
            // İlk satır: key, tr, en, ...
            List<string> header = rows[0];
            var columnLanguage = new Dictionary<int, string>();
            for (int c = 1; c < header.Count; c++)
            {
                string lang = header[c].Trim();
                if (lang.Length == 0 || table.ContainsKey(lang)) continue;
                columnLanguage[c] = lang;
                languages.Add(lang);
                table[lang] = new Dictionary<string, string>();
            }

            // Diğer satırlar: anahtar + her dildeki metin
            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                string key = row[0].Trim();
                if (key.Length == 0 || key.StartsWith("#")) continue; // boş satır veya açıklama

                foreach (var pair in columnLanguage)
                {
                    string value = pair.Key < row.Count ? row[pair.Key].Trim() : "";
                    table[pair.Value][key] = value;
                }
            }
        }
        else
        {
            Debug.LogError("[Dil] Assets/Resources/localization.csv bulunamadı veya boş.");
        }

        if (languages.Count == 0)
        {
            languages.Add(FallbackLanguage);
            table[FallbackLanguage] = new Dictionary<string, string>();
        }

        // Kayıtlı dil > bilgisayarın dili > İngilizce > tablodaki ilk dil
        currentLanguage = PlayerPrefs.GetString(LanguageKey, DetectSystemLanguage());
        if (!table.ContainsKey(currentLanguage))
            currentLanguage = table.ContainsKey(FallbackLanguage) ? FallbackLanguage : languages[0];
    }

    // Oyun ilk kez açıldığında bilgisayarın diline göre başlangıç dili
    private static string DetectSystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Turkish:    return "tr";
            case SystemLanguage.Spanish:    return "es";
            case SystemLanguage.German:     return "de";
            case SystemLanguage.French:     return "fr";
            case SystemLanguage.Russian:    return "ru";
            case SystemLanguage.Portuguese: return "pt";
            default:                        return "en";
        }
    }

    // Basit CSV okuyucu. Tırnak içindeki virgülleri ve satır atlamalarını destekler.
    // Ayırıcıyı kendisi bulur: Türkçe Excel ";" ile kaydeder, Google Sheets "," ile.
    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        if (string.IsNullOrEmpty(text)) return rows;

        text = text.TrimStart('﻿'); // Excel'in eklediği görünmez BOM karakteri

        int firstLineEnd = text.IndexOf('\n');
        string firstLine = firstLineEnd >= 0 ? text.Substring(0, firstLineEnd) : text;
        char separator = Count(firstLine, ';') > Count(firstLine, ',') ? ';' : ',';

        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; } // "" = tırnak işareti
                    else inQuotes = false;
                }
                else field.Append(ch);
            }
            else if (ch == '"') inQuotes = true;
            else if (ch == separator) { row.Add(field.ToString()); field.Clear(); }
            else if (ch == '\n' || ch == '\r')
            {
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++; // Windows satır sonu
                row.Add(field.ToString()); field.Clear();
                rows.Add(row); row = new List<string>();
            }
            else field.Append(ch);
        }

        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
        return rows;
    }

    private static int Count(string s, char c)
    {
        int n = 0;
        foreach (char x in s) if (x == c) n++;
        return n;
    }
}
