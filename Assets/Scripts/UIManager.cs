using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Refs – drag from Canvas")]
    public TMP_InputField MessageInput;
    public TMP_InputField EmailInput;
    public TMP_Text OutputText;
    public TMP_Text StatusText;
    public Button ConvertButton;
    public Button SendButton;

    [Header("Overlays")]
    public CanvasGroup Toast;
    public TMP_Text ToastText;
    public CanvasGroup ConfirmDialog;
    public TMP_Text ConfirmText;
    public Button ConfirmYesButton;
    public Button ConfirmNoButton;

    [Header("Backends")]
    public Codificador codificador;
    public Emailsender emailer;

    private void Awake()
    {
        ConvertButton.onClick.AddListener(OnConvertClicked);
        SendButton.onClick.AddListener(OnSendClicked);

        HideCanvasGroup(Toast, immediate: true);
        HideCanvasGroup(ConfirmDialog, immediate: true);
        Log("Listo.");
    }

    private void OnConvertClicked()
    {
        var plain = MessageInput.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(plain))
        {
            ToastShort("Escribe un mensaje para convertir.");
            return;
        }

        string final = codificador.ConvertPipeline(plain);

        OutputText.enableWordWrapping = false;
        OutputText.text = final;
        Log("Mensaje convertido.");
        ToastShort("Cifrado listo.");
    }

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

        bool ok = emailer.TrySend(to, subject, body, out string err);

        yield return null;

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

    private void ShowCanvasGroup(CanvasGroup cg, bool immediate = false)
    {
        if (cg == null) return;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        if (immediate) cg.alpha = 1f;
        else StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, 0.15f));
    }

    private void HideCanvasGroup(CanvasGroup cg, bool immediate = false)
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
