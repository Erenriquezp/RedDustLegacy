using UnityEngine;

/// <summary>
/// Datos de un objeto escaneable (S05 T1, GDD §11.2).
/// Guía completa: Docs/Architecture/ScanSystem.md.
/// </summary>
[CreateAssetMenu(fileName = "ScanData", menuName = "Opportunity/Scan Data")]
public class ScanDataSO : ScriptableObject
{
    public enum Encabezado { ANALISIS, REGISTRO_AMBIENTAL, SENAL_IDENTIFICADA, ARCHIVO_HISTORICO }
    public enum Severidad { Nominal, Advertencia, Anomalia }

    [Header("Identidad (GDD §11.2)")]
    public string id = "SC-00";
    public Encabezado encabezado = Encabezado.ANALISIS;
    [Tooltip("Color del texto: Nominal = blanco, Advertencia = ambar, Anomalia = rojo (GDD §11.1).")]
    public Severidad severidad = Severidad.Nominal;

    [Header("Contenido (sin tildes ni enie — estetica de terminal)")]
    [TextArea(3, 8)] public string texto;

    [Header("Efectos")]
    [Tooltip("FB-01…06 o vacio. El SI minimo lo valida el CinematicManager (GDD §11.3).")]
    public string flashbackId = "";
    [Tooltip("true = SC de historia: el gating/flashback solo dispara la primera vez.")]
    public bool unaVez = true;

    public string EncabezadoTexto => encabezado switch
    {
        Encabezado.REGISTRO_AMBIENTAL => "REGISTRO AMBIENTAL",
        Encabezado.SENAL_IDENTIFICADA => "SENAL IDENTIFICADA",
        Encabezado.ARCHIVO_HISTORICO  => "ARCHIVO HISTORICO",
        _                             => "ANALISIS",
    };
}
