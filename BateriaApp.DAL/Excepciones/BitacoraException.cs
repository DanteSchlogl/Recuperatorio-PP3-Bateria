namespace BateriaApp.DAL.Excepciones;

/// <summary>
/// Envuelve cualquier problema de entrada/salida ocurrido al operar sobre la bitácora,
/// para que las capas superiores no tengan que conocer <c>IOException</c>,
/// <c>UnauthorizedAccessException</c>, etc. Conserva la excepción original como
/// excepción interna para no perder información de diagnóstico.
/// </summary>
public class BitacoraException : Exception
{
    /// <summary>Crea la excepción indicando el mensaje, la ruta afectada y el error original.</summary>
    /// <param name="mensaje">Descripción del problema.</param>
    /// <param name="ruta">Ruta del archivo o carpeta involucrada.</param>
    /// <param name="excepcionInterna">Excepción original de entrada/salida.</param>
    public BitacoraException(string mensaje, string ruta, Exception? excepcionInterna = null)
        : base(mensaje, excepcionInterna)
    {
        Ruta = ruta;
    }

    /// <summary>Ruta del archivo o carpeta donde se produjo el problema.</summary>
    public string Ruta { get; }
}
