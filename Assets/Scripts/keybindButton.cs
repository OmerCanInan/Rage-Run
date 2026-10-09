using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Ayarlardaki tek bir tuş butonu. Tıklanınca "Bir tuşa bas..." yazar, basılan tuşu atar.
// Butona ekle, Inspector'da eylem adını ve hangi tuş olduğunu (0 = 1. tuş, 1 = 2. tuş) seç.
[RequireComponent(typeof(Button))]
public class keybindButton : MonoBehaviour
{
    // Eylem adları gameControls dosyasındakiyle birebir aynı olmalı
    private enum GameAction { MoveLeft, MoveRight, Jump, Restart }

    [SerializeField] private GameAction action = GameAction.Jump;
    [Tooltip("0 = 1. tuş, 1 = 2. tuş")]
    [SerializeField, Range(0, 1)] private int bindingIndex = 0;
    [SerializeField] private TMP_Text keyLabel; // Boş bırakılırsa butonun içindeki yazıyı kullanır

    private Button button;
    private bool waitingForKey;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (keyLabel == null) keyLabel = GetComponentInChildren<TMP_Text>();
        button.onClick.AddListener(OnClicked); // Inspector'da On Click bağlamana gerek yok
    }

    private void OnEnable()
    {
        inputManager.OnBindingsChanged += Refresh;
        localizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        inputManager.OnBindingsChanged -= Refresh;
        localizationManager.OnLanguageChanged -= Refresh;
        if (waitingForKey) inputManager.CancelRebind(); // Panel kapandıysa yarım atamayı iptal et
        waitingForKey = false;
    }

    private void OnClicked()
    {
        if (inputManager.IsRebinding) return; // Başka bir buton zaten tuş bekliyor

        // Butonu seçili bırakma; yoksa Enter gibi tuşlar butona tekrar tıklamış sayılır
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        waitingForKey = true;
        inputManager.StartRebind(action.ToString(), bindingIndex);
        Refresh(); // Atama başladıysa "Bir tuşa bas..." yazar, başlamadıysa tuş adına döner
    }

    private void Refresh()
    {
        if (waitingForKey && !inputManager.IsRebinding) waitingForKey = false; // Atama bitti

        keyLabel.text = waitingForKey
            ? localizationManager.Get("controls_press_key")
            : inputManager.GetKeyName(action.ToString(), bindingIndex);
    }
}
