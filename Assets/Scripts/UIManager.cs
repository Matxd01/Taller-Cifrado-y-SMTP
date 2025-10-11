using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Refs – arrastra desde el Canvas")]
    public TMP_InputField MessageInput;   // "MessageInput"
    public TMP_InputField EmailInput;     // "EmailInput"
    public TMP_Text OutputText;           // "OutputText"
    public TMP_Text StatusText;           // "StatusText"
    public Button ConvertButton;          // "ConvertButton"
    public Button SendButton;             // "SendButton"

    [Header("Overlays")]
    public CanvasGroup Toast;             // "Toast"
    public TMP_Text ToastText;            // "ToastText"
    public CanvasGroup ConfirmDialog;     // "ConfirmDialog"
    public TMP_Text ConfirmText;          // "ConfirmText"
    public Button ConfirmYesButton;       // "ConfirmYesButton"
    public Button ConfirmNoButton;        // "ConfirmNoButton"

    [Header("Backends")]
    public Codificador codificador;       // GO "Codificador"
    public Emailsender emailer;           // GO "SMTP"

    private void Awake()
    {
        // Wire básico
        ConvertButton.onClick.AddListener(OnConvertClicked);
        SendButton.onClick.AddListener(OnSendClicked);

        // Oculta overlays al iniciar
        HideCanvasGroup(Toast, immediate:true);
        HideCanvasGroup(ConfirmDialog, immediate:true);
        Log("Listo.");
    }

    // ---- Convertir ----
    private void OnConvertClicked()
    {
        var plain = MessageInput.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(plain))
        {
            ToastShort("Escribe un mensaje para convertir.");
            return;
        }

        // --- MICRO-AJUSTE #1 requerido en Codificador (abajo):
        //     expón un método público que retorne el cifrado final.
        //     string ConvertPipeline(string raw)
        string final = codificador.ConvertPipeline(plain);      // <—

        OutputText.enableWordWrapping = false;
        OutputText.text = final;
        Log("Mensaje convertido.");
        ToastShort("Cifrado listo.");
    }

    // ---- Enviar ----
    private void OnSendClicked()
    {
        var to = EmailInput.text?.Trim() ?? "";
        if (!IsLikelyEmail(to))
        {
            ToastShort("Correo inválido.");
            return;
        }
        if (string.IsNullOrWhiteSpace(OutputText.text))
        {
            ToastShort("Primero convierte el mensaje.");
            return;
        }

        // Confirmación
        ConfirmText.text = $"Enviar mensaje cifrado a:\n<b>{to}</b>?";
        ShowCanvasGroup(ConfirmDialog);
        ConfirmYesButton.onClick.RemoveAllListeners();
        ConfirmNoButton.onClick.RemoveAllListeners();

        ConfirmYesButton.onClick.AddListener(() =>
        {
            HideCanvasGroup(ConfirmDialog);
            StartCoroutine(SendRoutine(to, "Taller: mensaje cifrado", OutputText.text));
        });
        ConfirmNoButton.onClick.AddListener(() => HideCanvasGroup(ConfirmDialog));
    }

    private IEnumerator SendRoutine(string to, string subject, string body)
    {
        SetInteractable(false);
        Log("Enviando…");

        // --- MICRO-AJUSTE #2 en Emailsender (abajo):
        //     agrega un método TrySend(to, subject, body, out error)
        bool ok;
        string err;
        ok = emailer.TrySend(to, subject, body, out err);       // <—

        yield return null; // cede un frame por si hay UI que refrescar

        if (ok)
        {
            Log("Correo enviado ✅");
            ToastShort("Correo enviado.");
        }
        else
        {
            Log("Error al enviar: " + err);
            ToastShort("Error al enviar. Revisa SMTP.");
        }

        SetInteractable(true);
    }

    // ---- Utils ----
    private void SetInteractable(bool value)
    {
        ConvertButton.interactable = value;
        SendButton.interactable = value;
    }

    private void Log(string msg)
    {
        if (StatusText != null) StatusText.text = msg;
        Debug.Log(msg);
    }

    private bool IsLikelyEmail(string s)
    {
        return !string.IsNullOrWhiteSpace(s) && s.Contains("@") && s.Contains(".");
    }

    private void ToastShort(string msg) => StartCoroutine(ToastCo(msg, 1.5f));

    private IEnumerator ToastCo(string msg, float seconds)
    {
        if (Toast == null || ToastText == null) yield break;
        ToastText.text = msg;
        ShowCanvasGroup(Toast);
        yield return new WaitForSeconds(seconds);
        HideCanvasGroup(Toast);
    }

    private void ShowCanvasGroup(CanvasGroup cg, bool immediate=false)
    {
        if (cg == null) return;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        if (immediate) cg.alpha = 1f;
        else StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, 0.15f));
    }

    private void HideCanvasGroup(CanvasGroup cg, bool immediate=false)
    {
        if (cg == null) return;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (immediate) cg.alpha = 0f;
        else StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, 0.15f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float a, float b, float t)
    {
        float el = 0f;
        while (el < t)
        {
            el += Time.deltaTime;
            cg.alpha = Mathf.Lerp(a, b, el / t);
            yield return null;
        }
        cg.alpha = b;
    }
}
