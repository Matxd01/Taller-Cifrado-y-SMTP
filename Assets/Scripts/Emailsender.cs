using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

public class Emailsender : MonoBehaviour
{
    [Header("Sender credentials")]
    public string fromEmail = "tucorreo@gmail.com";
    public string password = "app-password-16chars";

    [Header("SMTP server")]
    public string smtpHost = "smtp.gmail.com";
    public int smtpPort = 587;
    public bool enableSsl = true;
    public int timeoutMs = 15000;

    [Header("Quick test from Inspector")]
    public string receiverEmail = "destinatario@ejemplo.com";

    private SmtpClient CreateClient()
    {
        var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, password),
            Timeout = Mathf.Max(1000, timeoutMs)
        };
        return client;
    }

    public bool TrySend(string to, string subject, string body, out string error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(password))
        {
            error = "Missing SMTP config: fromEmail/password.";
            Debug.LogError(error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            error = "Empty recipient.";
            Debug.LogError(error);
            return false;
        }

        try
        {
            using (var msg = new MailMessage())
            using (var smtp = CreateClient())
            {
                msg.From = new MailAddress(fromEmail, "Taller Cifrado");
                msg.To.Add(new MailAddress(to));
                msg.Subject = subject ?? "(no subject)";
                msg.Body = body ?? "";

                smtp.Send(msg);
            }

            Debug.Log("Email sent.");
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            Debug.LogError("SMTP error: " + e);
            return false;
        }
    }

    public async Task<(bool ok, string error)> SendAsync(string to, string subject, string body)
    {
        string err = null;
        bool ok = await Task.Run(() => TrySend(to, subject, body, out err));
        return (ok, err);
    }

    [ContextMenu("Send Email (uses receiverEmail)")]
    private void SendEmail_Context()
    {
        if (string.IsNullOrWhiteSpace(receiverEmail))
        {
            Debug.LogWarning("receiverEmail is empty.");
            return;
        }

        string subject = "Prueba SMTP desde Unity";
        string body = "Este correo es una prueba enviada desde un proyecto Unity.";

        if (TrySend(receiverEmail, subject, body, out string err))
            Debug.Log("Email sent (ContextMenu).");
        else
            Debug.LogError("ContextMenu failed: " + err);
    }
}
