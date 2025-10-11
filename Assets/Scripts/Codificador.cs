using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Codificador : MonoBehaviour
{
    [Header("UI (arrastra desde el Canvas)")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public Button convertirBtn;

    // ===== AÉREO (NATO) =====
    private static readonly Dictionary<char, string> NATO = new Dictionary<char, string>()
    {
        ['A']="Alpha",['B']="Bravo",['C']="Charlie",['D']="Delta",['E']="Echo",
        ['F']="Foxtrot",['G']="Golf",['H']="Hotel",['I']="India",['J']="Juliet",
        ['K']="Kilo",['L']="Lima",['M']="Mike",['N']="November",['O']="Oscar",
        ['P']="Papa",['Q']="Quebec",['R']="Romeo",['S']="Sierra",['T']="Tango",
        ['U']="Uniform",['V']="Victor",['W']="Whiskey",['X']="X-RAY",
        ['Y']="Yankee",['Z']="Zulu",
        ['0']="Zero",['1']="One",['2']="Two",['3']="Three",['4']="Four",
        ['5']="Five",['6']="Six",['7']="Seven",['8']="Eight",['9']="Nine",
        [' ']=" "
    };

    // ===== BINARIO (7 bits como en la tabla) =====
    private static readonly Dictionary<char, string> LATIN_TO_BIN7 = new Dictionary<char, string>()
    {
        // A=65 (1000001) ... Z=90 (1011010) en 7 bits
        ['A']="1000001", ['B']="1000010", ['C']="1000011", ['D']="1000100",
        ['E']="1000101", ['F']="1000110", ['G']="1000111", ['H']="1001000",
        ['I']="1001001", ['J']="1001010", ['K']="1001011", ['L']="1001100",
        ['M']="1001101", ['N']="1001110", ['O']="1001111", ['P']="1010000",
        ['Q']="1010001", ['R']="1010010", ['S']="1010011", ['T']="1010100",
        ['U']="1010101", ['V']="1010110", ['W']="1010111", ['X']="1011000",
        ['Y']="1011001", ['Z']="1011010",
        // Dígitos tras NATO (Zero, One...) no se usan aquí: los convierte a palabras el paso AÉREO.
    };

    // ===== BINARIO → “GRIEGO” =====
    private static readonly Dictionary<char, string> BIN_TO_GRE = new Dictionary<char, string>()
    {
        ['0']="α", ['1']="μ", ['/']="ω"
    };

    // ===== “GRIEGO” → LATÍN (para mostrar) =====
    private static readonly Dictionary<char, char> GRE_TO_LAT = new Dictionary<char, char>()
    {
        ['α']='A', ['μ']='M', ['ω']='W'
    };

    private void Awake()
    {
        if (convertirBtn != null)
            convertirBtn.onClick.AddListener(OnConvertir);
    }

    /// <summary>
    /// Pipeline público para reutilizar desde otros scripts (p. ej. UIManager).
    /// </summary>
    public string ConvertPipeline(string raw)
    {
        string limpio = Normalizar(raw);                     // MAYÚSCULAS + sin tildes
        string sNato = MapPorCaracter(limpio, NATO, " ");    // 1) AERONÁUTICO (NATO)
        string sBin  = NatoToBinary7Compact(sNato);          // 2) NATO → BINARIO (7 bits)
        string sGre  = MapPorCaracter(sBin, BIN_TO_GRE, ""); // 3) BINARIO → “GRIEGO”
        string final = GriegoALatinFinal(sGre)               // 4) “GRIEGO” → LATÍN
                        .ToUpperInvariant();
        return final;
    }

    private void OnConvertir()
    {
        string raw = (inputField != null) ? inputField.text : string.Empty;
        string final = ConvertPipeline(raw);

        if (outputText != null)
        {
            outputText.enableWordWrapping = false;
            outputText.text = final; // solo la codificación final
        }
    }

    // ===== Utilidades =====

    private static string Normalizar(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        // Quita tildes/diacríticos y pasa a MAYÚSCULAS
        string formD = s.ToUpperInvariant().Normalize(NormalizationForm.FormD);
        return Regex.Replace(formD, @"\p{Mn}+", "");
    }

    private static string MapPorCaracter(string texto, Dictionary<char, string> regla, string separador)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        var sb = new StringBuilder(texto.Length * 2);

        for (int i = 0; i < texto.Length; i++)
        {
            char c = texto[i];
            if (regla.TryGetValue(c, out string val)) sb.Append(val);
            else sb.Append(c);

            if (i < texto.Length - 1 && !string.IsNullOrEmpty(separador))
                sb.Append(separador);
        }
        return sb.ToString();
    }

    // Convierte cadena NATO (palabras con espacios, guiones, etc.) a binario 7 bits:
    // - Solo letras A–Z se convierten usando la tabla (sin separadores entre bytes).
    // - Espacio entre palabras en NATO -> '/' (separador de palabra).
    // - Caracteres no A–Z (como '-') se ignoran.
    private static string NatoToBinary7Compact(string nato)
    {
        if (string.IsNullOrEmpty(nato)) return "";
        var sb = new StringBuilder(nato.Length * 4);

        foreach (char ch in nato)
        {
            if (ch == ' ')
            {
                sb.Append('/'); // separador entre palabras
                continue;
            }
            if (ch >= 'A' && ch <= 'Z')
            {
                if (LATIN_TO_BIN7.TryGetValue(ch, out string bits))
                    sb.Append(bits);
            }
            // ignora otros caracteres (p. ej., '-')
        }
        return sb.ToString();
    }

    private static string GriegoALatinFinal(string gre)
    {
        if (string.IsNullOrEmpty(gre)) return "";
        var sb = new StringBuilder(gre.Length);

        foreach (char g in gre)
        {
            if (GRE_TO_LAT.TryGetValue(g, out char lat))
                sb.Append(lat);
            // Si aparece otro símbolo, se ignora para mantener la cadena limpia
        }
        return sb.ToString();
    }
}
