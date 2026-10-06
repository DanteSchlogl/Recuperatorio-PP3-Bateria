namespace BateriaApp.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta asignar a la carga un valor fuera del rango válido
/// (0 a 100). Es una excepción de dominio: describe una regla de negocio violada,
/// no un error técnico.
/// </summary>
public class CargaFueraDeRangoException : Exception
{
    /// <summary>Valor mínimo admitido para la carga (0 %).</summary>
    public const int CargaMinima = 0;

    /// <summary>Valor máximo admitido para la carga (100 %).</summary>
    public const int CargaMaxima = 100;

    /// <summary>Crea la excepción informando el valor inválido recibido.</summary>
    /// <param name="valorInvalido">Valor de carga rechazado.</param>
    public CargaFueraDeRangoException(int valorInvalido)
        : base($"La carga debe estar entre {CargaMinima} y {CargaMaxima} %. Valor recibido: {valorInvalido} %.")
    {
        ValorInvalido = valorInvalido;
    }

    /// <summary>Valor de carga que provocó el rechazo.</summary>
    public int ValorInvalido { get; }
}
