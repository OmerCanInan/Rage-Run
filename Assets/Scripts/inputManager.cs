using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Oyunun tüm tuş okumaları buradan geçer. Sahneye eklenmez, her yerden çağrılır.
// Tuşlar Assets/Resources/gameControls.inputactions dosyasında tanımlı.
// Oyuncunun değiştirdiği tuşlar PlayerPrefs'e ("Keybinds") JSON olarak kaydedilir.
//
// Karakter kodunda kullanım:
//   float yon = inputManager.MoveDirection;   // -1 sol, 0 dur, 1 sağ
//   if (inputManager.JumpPressed) { ... }     // zıplama tuşuna bu karede basıldı
//   if (inputManager.JumpHeld) { ... }        // zıplama tuşu basılı tutuluyor
public static class inputManager
{
    private const string AssetName = "gameControls";
    private const string MapName = "Gameplay";
    private const string SaveKey = "Keybinds";

    // Tuşlar değişince (atama, iptal, varsayılana dönüş) tetiklenir; ayarlardaki butonlar dinler
    public static event Action OnBindingsChanged;

    public static bool IsRebinding { get; private set; }

    private static InputActionAsset asset;
    private static InputActionMap map;
    private static InputActionRebindingExtensions.RebindingOperation operation;

    // Editörde her Play'e basışta sıfırdan başlasın
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        operation?.Dispose();
        operation = null;
        asset = null;
        map = null;
        IsRebinding = false;
        OnBindingsChanged = null;
    }

    // --- Oyun içinde kullanılanlar ---

    public static float MoveDirection
    {
        get
        {
            if (!EnsureLoaded()) return 0f;
            float direction = 0f;
            if (map["MoveRight"].IsPressed()) direction += 1f;
            if (map["MoveLeft"].IsPressed()) direction -= 1f;
            return direction; // İkisine birden basılırsa 0 = durur
        }
    }

    public static bool JumpPressed    => WasPressed("Jump");
    public static bool JumpHeld       => IsHeld("Jump");
    public static bool RestartPressed => WasPressed("Restart");
    public static bool PausePressed   => WasPressed("Pause");

    private static bool WasPressed(string action) => EnsureLoaded() && map[action].WasPressedThisFrame();
    private static bool IsHeld(string action)     => EnsureLoaded() && map[action].IsPressed();

    // --- Ayarlar ekranının kullandıkları ---

    // Bir tuşun ekranda görünecek adı (örn. "Space", "A"). Atanmamışsa "—"
    public static string GetKeyName(string actionName, int bindingIndex)
    {
        if (!EnsureLoaded()) return "—";
        InputAction action = map.FindAction(actionName);
        if (action == null || bindingIndex >= action.bindings.Count) return "—";

        string name = action.GetBindingDisplayString(bindingIndex);
        return string.IsNullOrEmpty(name) ? "—" : name;
    }

    // "Bir tuşa bas..." modunu başlatır. Esc ile iptal edilir.
    public static void StartRebind(string actionName, int bindingIndex)
    {
        if (!EnsureLoaded() || IsRebinding) return;

        InputAction action = map.FindAction(actionName);
        if (action == null || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError($"[Tuşlar] '{actionName}' eylemi veya {bindingIndex}. bağlantısı bulunamadı.");
            return;
        }

        string oldPath = action.bindings[bindingIndex].effectivePath;

        IsRebinding = true;
        map.Disable(); // Atama sırasında karakter tuşlara tepki vermesin (Unity bunu şart koşuyor)

        operation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsHavingToMatchPath("<Keyboard>")      // Sadece klavye tuşları
            .WithControlsExcluding("<Keyboard>/anyKey")       // "Herhangi bir tuş" atanamasın
            .WithCancelingThrough("<Keyboard>/escape")        // Esc = vazgeç
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                ResolveConflicts(action, bindingIndex, oldPath);
                FinishRebind();
            })
            .OnCancel(op => FinishRebind())
            .Start();
    }

    // Ayarlar paneli kapanırken yarım kalan atamayı iptal etmek için
    public static void CancelRebind()
    {
        operation?.Cancel();
    }

    public static void ResetToDefaults()
    {
        if (!EnsureLoaded() || IsRebinding) return;
        asset.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(SaveKey);
        OnBindingsChanged?.Invoke();
    }

    // --- İç işler ---

    private static void FinishRebind()
    {
        operation?.Dispose();
        operation = null;
        map.Enable();
        IsRebinding = false;
        PlayerPrefs.SetString(SaveKey, asset.SaveBindingOverridesAsJson());
        OnBindingsChanged?.Invoke();
    }

    // Yeni tuş başka bir eylemde kullanılıyorsa, o eyleme bu bağlantının eski tuşunu ver (yer değiştirme).
    // Örn: Zıpla = Space, Sola = A. Sola'ya Space atanırsa, Zıpla otomatik A olur.
    private static void ResolveConflicts(InputAction changedAction, int changedIndex, string oldPath)
    {
        string newPath = changedAction.bindings[changedIndex].effectivePath;
        if (newPath == oldPath) return;

        foreach (InputAction action in map.actions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action == changedAction && i == changedIndex) continue;
                if (action.bindings[i].effectivePath == newPath)
                    action.ApplyBindingOverride(i, oldPath);
            }
        }
    }

    // İlk kullanımda dosyayı yükler, kayıtlı tuşları uygular ve tuş okumayı açar
    private static bool EnsureLoaded()
    {
        if (asset != null) return true;

        InputActionAsset original = Resources.Load<InputActionAsset>(AssetName);
        if (original == null)
        {
            Debug.LogError("[Tuşlar] Assets/Resources/gameControls.inputactions bulunamadı.");
            return false;
        }

        // Kopya üzerinde çalış: oyuncunun değişiklikleri proje dosyasına asla yazılmasın
        asset = UnityEngine.Object.Instantiate(original);
        map = asset.FindActionMap(MapName, true);

        string saved = PlayerPrefs.GetString(SaveKey, "");
        if (saved.Length > 0) asset.LoadBindingOverridesFromJson(saved);

        asset.Enable();
        return true;
    }
}
