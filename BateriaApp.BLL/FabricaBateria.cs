using BateriaApp.DAL;
using BateriaApp.DAL.Formato;

namespace BateriaApp.BLL;

/// <summary>
/// Formatos de bitácora disponibles para la capa de presentación.
/// Existe para que la UI pueda elegir el formato sin conocer las clases concretas de DAL:
/// es el punto donde se resuelve la estrategia (patrón Strategy) elegida.
/// </summary>
public enum FormatoBitacora
{
    /// <summary>Un renglón por evento (formato por omisión).</summary>
    TextoPlano,

    /// <summary>Un bloque multilínea por evento, con todos los datos detallados.</summary>
    Detallado
}

/// <summary>
/// Fábrica (patrón Factory Method) que construye un <see cref="ServicioBateria"/> ya
/// cableado: repositorio concreto + estrategia de formato + servicio de negocio.
/// La UI pide "un servicio" y lo recibe listo para usar, sin acoplarse a las capas internas.
/// </summary>
public static class FabricaBateria
{
    /// <summary>Crea el servicio completo de la batería.</summary>
    /// <param name="cargaInicial">Carga inicial en tanto por ciento (0 a 100).</param>
    /// <param name="conectado">Indica si la batería arranca conectada a la tensión.</param>
    /// <param name="carpetaBitacoras">Carpeta de bitácoras; por omisión, <c>Bitacoras</c>.</param>
    /// <param name="formato">Formato con el que se escribirá la bitácora.</param>
    /// <returns>El servicio listo para operar.</returns>
    public static ServicioBateria Crear(
        int cargaInicial = 50,
        bool conectado = false,
        string? carpetaBitacoras = null,
        FormatoBitacora formato = FormatoBitacora.TextoPlano)
    {
        IFormateadorRegistro formateador = formato switch
        {
            FormatoBitacora.Detallado => new FormateadorDetallado(),
            _ => new FormateadorTextoPlano(),
        };

        var repositorio = new RepositorioBitacoraArchivo(carpetaBitacoras, formateador);

        return new ServicioBateria(repositorio, cargaInicial, conectado);
    }
}
