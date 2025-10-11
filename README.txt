Taller: Cifrado y SMTP (Unity)

1) ¿Qué hace la aplicación?

La app permite escribir un mensaje y un correo de destino, aplica una cadena de tres filtros de cifrado al mensaje, y luego envía el texto cifrado por SMTP.
Incluye validaciones básicas , confirmación antes de enviar, y mensajes emergentes para feedback rápido al usuario.

Flujo general:

Entrada del usuario (texto) 
   → Filtro 1: Aéreo (NATO)
   → Filtro 2: Binario 7 bits
   → Filtro 3: Mapeo simbólico (representación “griega” y visual final)
   → Envío por SMTP (cuerpo del correo = texto cifrado)

2) Filtros de cifrado (creados por mí)

Filtro 1 — Aéreo (NATO)

Tipo: Sustitución por palabras clave aeronáuticas.

Descripción: Cada carácter alfabético del mensaje en mayúsculas se convierte a su palabra del alfabeto NATO:

A → Alpha, B → Bravo, C → Charlie, …, Z → Zulu.

Los dígitos 0–9 se transforman en sus palabras (Zero, One, … Nine).

Los espacios se conservan 

Objetivo: Aumentar longitud y estructura del texto , generando una expansión semántica previa al paso binario.

Ejemplo: Entrada: PERRO

Salida: Papa Echo Romeo Romeo Oscar

Filtro 2 — Binario 7 bits (ASCII básico)

Tipo: Sustitución carácter→bits.

Descripción: Se recorre la salida del Filtro 1 y se toman solo letras A–Z de esas palabras. Cada letra se mapea a su código ASCII de 7 bits (65–90):

A → 1000001, B → 1000010, …, Z → 1011010

Separación de palabras: donde el texto NATO tenía espacios, aquí se coloca un separador /.

Se ignoran signos como guiones u otros caracteres no alfabéticos en las palabras NATO.

Objetivo: Pasar a una representación binaria compacta y homogénea, manteniendo separación entre palabras.

Ejemplo: Entrada: Papa Echo

Letras consideradas: P a p a E c h o → PAPA y ECHO

Salida (ilustrativa): 1010000 1000001 1010000 1000001 / 1000101 1000011 1001000 1001111

Filtro 3 — Mapeo simbólico (representación “griega” y visual final)

Tipo: Sustitución de símbolos y forma de visualización.

Descripción (dos capas):

Binario → “Griego”:

0 → α, 1 → μ, / → ω
Esto produce un texto compuesto por símbolos α μ ω, difícil de leer a simple vista, útil como capa simbólica.

Visualización final (para mostrar en UI):

Para depurar/leer más compacto en la app, mapeo solo de visualización:
α → A, μ → M, ω → W, y se muestra en mayúsculas.

Equivalencias mentales: A ≡ 0, M ≡ 1, W ≡ /.

Este paso no altera el cifrado conceptual; es una representación para la interfaz, que además facilita pruebas rápidas sin perder la estructura.

Objetivo: Producir una salida no obvia y fácilmente verificable (gracias a la versión visual A/M/W), manteniendo la delimitación de palabras (W).

Ejemplo final :

Binario: 1010000/1000101

“Griego”: μ α μ α α α α ω μ α α α α α 

Visual final: MAMAAAAWMAAAA…

Decodificación (sólo para pruebas): A=0, M=1, W=/

3) Interfaz de Usuario (UI) y experiencia

Inputs: MessageInput (texto), EmailInput (destino).

Acciones:

Convertir: aplica la cadena de filtros y muestra el resultado en OutputText.

Enviar: abre un ConfirmDialog. Al aceptar, envía por SMTP el mensaje cifrado.

Feedback:

StatusText para estado (listo, cifrando, enviando…).

Toast para avisos breves (ej. “Cifrado listo”).

Confirmación antes de enviar (evita envíos accidentales).

4) Envío por SMTP

Script Emailsender configurable por Inspector:

fromEmail, password (recomendado App Password si usas Gmail),

smtpHost, smtpPort, enableSsl.

Método principal utilizado por la app:
TrySend(to, subject, body, out error)
Retorna true/false y llena error en caso de fallo (credenciales, firewall, etc.).

5) Pruebas y validaciones

Validaciones previas: mensaje no vacío, email con formato probable.

Confirmación: diálogo antes de transmitir.

Pruebas unitarias sugeridas (no obligatorias):

Entradas con tildes/ñ (se normalizan a mayúsculas sin diacríticos).

Cadena vacía y de 1 carácter.

Cadenas mixtas con números y espacios.

Verificar separador de palabras (W) en salida visual.

6) Limitaciones y notas

No es cifrado fuerte (no usa claves/semillas). Es didáctico para cumplir el taller (diseñar filtros, encadenarlos y transmitir via SMTP).

La salida visual A/M/W es un alias de 0/1/ (no parte adicional del cifrado), pensada para inspección rápida y ejercicios de decodificación.

No se suben credenciales al repositorio.

7) Cómo correr

Abrir escena principal con el Canvas y los objetos:

MessageInput, EmailInput, ConvertButton, SendButton,
OutputText, StatusText, Toast(CanvasGroup+ToastText),
ConfirmDialog(CanvasGroup+ConfirmText+ConfirmYesButton+ConfirmNoButton).

GameObjects: Codificador, SMTP, UIManager (con referencias asignadas).

Configurar SMTP en SMTP (Emailsender).

Play → Escribir mensaje y email → Convertir → Enviar.

Apéndice: ejemplo de ida y vuelta (conceptual)

Entrada: PERRO

Filtro 1 (NATO): Papa Echo Romeo Romeo Oscar

Filtro 2 (binario 7 bits, con ‘/’ entre palabras):
1010000 1000101 1010010 1010010 1001111 → 1010000/1000101/1010010/1010010/1001111

Filtro 3 (“griego”): μ α μ α α α α ω μ α α α α …

Visual final (UI): MAMAAAAWMAA…

Decodificación de prueba (visual→binario→ASCII): A=0, M=1, W=/ → PERRO.