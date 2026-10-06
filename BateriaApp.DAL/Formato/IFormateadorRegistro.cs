using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

/// <summary>
/// Estrategia de formato (patrón Strategy) que convierte un registro de bitácora en el
/// texto que finalmente se escribe en el archivo.
/// Gracias a esta abstracción el repositorio sabe <b>dónde</b> guardar pero no
/// <b>con qué formato</b>: es exactamente el "formato personalizado" que pide el
/// enunciado, y permite agregar nuevos formatos sin tocar el repositorio.
/// </summary>
public interface IFormateadorRegistro
{
    /// <summary>Nombre del formato, útil para mostrarlo al usuario.</summary>
    string Nombre { get; }

    /// <summary>Convierte el registro en el texto a persistir.</summary>
    /// <param name="registro">Registro a formatear.</param>
    /// <returns>Línea (o bloque) de texto lista para escribirse en la bitácora.</returns>
    string Formatear(RegistroBitacora registro);
}
