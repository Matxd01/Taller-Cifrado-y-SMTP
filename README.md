App creada en Unity que permite escribir un mensaje, cifrarlo pasando por 3 filtros y enviarlo por correo a través de SMTP y ver cómo se transforma el mensaje en pantalla.

¿Qué hace la app?

Simplemente escribes tu mensaje y la dirección de correo a la que quieres enviarlo.

La app aplica estos tres filtros:

NATO

Binario (7 bits)

Vista “griega”: 0→α, 1→μ, /→ω 

Finalmente, el texto cifrado se envía por SMTP.

La idea es : observar cómo el mismo mensaje pasa por diferentes capas osea (hashearlo lo vi en clase xd)  hasta convertirse en un mensaje cifrado

Ejemplo rápido

Entrada: PERRO
NATO: Papa Echo Romeo Romeo Oscar
Binario con /: 1010000/1000101/1010010/1010010/1001111
Vista A/M/W: MAMAAAAWMAAA... (A=0, M=1, W=/)

¿Cómo usar la app?

Abre la escena principal en Unity.

En el objeto Api/SMTP ingresa:

Base Url si usas un servidor .

fromEmail y password (Usar App Password en el Gmail con la verificacion en 2 pasos).

smtpHost, smtpPort, enableSsl .

En la interfaz:

MessageInput: tu mensaje.

EmailInput: dirección de destino.

Convertir: para ver el texto cifrado (A/M/W).

Enviar: confirma y manda el correo.

Aparece las notificaciones :(“Cifrado listo”, “Correo enviado”) .

Archivos clave

Codificador.cs → combina los tres filtros y te da el resultado final.

Emailsender.cs → se encarga del envío por SMTP (TrySend).

UIManager.cs → gestiona los botones, las notificaciones y la confirmación.

ServerData.cs, PlayerController.cs (si utilizas la parte de pruebas de posiciones).
