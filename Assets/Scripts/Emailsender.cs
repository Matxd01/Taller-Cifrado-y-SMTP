using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Envío SMTP sencillo para Unity.
/// Adjunta este script al GameObject "SMTP".
/// Recomendación (Gmail):
///  - Activa 2FA y usa un App Password (16 chars) en 'password'.
///  - Host: smtp.gmail.com, Port: 587, EnableSsl: true
/// </summary>
public class Emailsender : MonoBehaviour
{
    [Header("Credenciales del remitente")]
    [Tooltip("Correo emisor. Debe coincidir con el usuario SMTP.")]
    public string fromEmail = "tucorreo@gmail.com";

    [Tooltip("App Password (NUNCA subas esto al repo).")]
    public string password = "app-password-16chars";

    [Header("SMTP Server")]
    public string smtpHost = "smtp.gmail.com";
    public int smtpPort = 587;
    public bool enableSsl = true;
    [Tooltip("Tiempo máximo de espera en milisegundos.")]
    public int timeoutMs = 15000;

    [Header("Pruebas rápidas desde Inspector")]
    [Tooltip("Si lo dejas vacío, TrySend usa el 'to' que le pases por parámetro.")]
    public string receiverEmail = "destinatario@ejemplo.com";

    // --- UTILIDAD: creador de cliente SMTP configurado ---
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

    /// <summary>
    /// Método sincrónico ligero para el coordinador (UIManager).
    /// </summary>
    /// <param name="to">Destino</param>
    /// <param name="subject">Asunto</param>
    /// <param name="body">Cuerpo del mensaje</param>
    /// <param name="error">Sale con el mensaje de error si falla</param>
    /// <returns>true si se envió; false en error</returns>
    public bool TrySend(string to, string subject, string body, out string error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(password))
        {
            error = "Config SMTP incompleta: fromEmail/password.";
            Debug.LogError(error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            error = "Correo destino vacío.";
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
                msg.Subject = subject ?? "(sin asunto)";
                msg.Body = body ?? "";

                smtp.Send(msg);
            }

            Debug.Log("Correo enviado exitosamente.");
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            Debug.LogError("Error al enviar el correo: " + e);
            return false;
        }
    }

    /// <summary>
    /// Variante async/Task para quien prefiera no bloquear el hilo principal.
    /// </summary>
    public async Task<(bool ok, string error)> SendAsync(string to, string subject, string body)
    {
        string err = null;
        bool ok = await Task.Run(() => TrySend(to, subject, body, out err));
        return (ok, err);
    }

    // --------- Herramientas de prueba desde el Inspector ---------

    [ContextMenu("Send Email (usar receiverEmail como destino)")]
    private void SendEmail_Context()
    {
        if (string.IsNullOrWhiteSpace(receiverEmail))
        {
            Debug.LogWarning("receiverEmail está vacío. Configúralo o usa TrySend(to,...).");
            return;
        }

        string subject = "Prueba SMTP desde Unity";
        string body = "Este correo es una prueba enviada desde un proyecto Unity.";

        if (TrySend(receiverEmail, subject, body, out string err))
            Debug.Log("Correo enviado (ContextMenu).");
        else
            Debug.LogError("Fallo en ContextMenu: " + err);
    }
}
