using BateriaApp.Domain;

namespace BateriaApp.DAL;

/// <summary>
/// Contrato de persistencia de la bitácora de la batería.
/// La capa de negocio depende de esta abstracción y no del archivo concreto: se puede
/// cambiar el almacenamiento (archivo de texto, base de datos, servicio en la nube) sin
/// modificar BLL ni UI.
/// </summary>
public interface IRepositorioBitacora
{
    /// <summary>Carpeta donde se guardan los archivos de bitácora.</summary>
    string Carpeta { get; }

    /// <summary>Marca de tiempo del último evento registrado, o <c>null</c> si todavía no hay ninguno.</summary>
    DateTime? FechaUltimoRegistro { get; }

    /// <summary>Cantidad total de eventos registrados en esta ejecución.</summary>
    int EventosRegistrados { get; }

    /// <summary>Devuelve la ruta del archivo de bitácora que corresponde a una fecha.</summary>
    /// <param name="fecha">Fecha cuya ruta se desea obtener.</param>
    /// <returns>Ruta del archivo diario.</returns>
    string ObtenerRutaArchivo(DateTime fecha);

    /// <summary>Persiste un registro en el archivo del día que corresponda a su marca de tiempo.</summary>
    /// <param name="registro">Registro a guardar.</param>
    void Registrar(RegistroBitacora registro);

    /// <summary>Lee todos los renglones de la bitácora de una fecha.</summary>
    /// <param name="fecha">Fecha de la bitácora a leer.</param>
    /// <returns>Renglones leídos; colección vacía si el archivo no existe.</returns>
    IReadOnlyList<string> LeerBitacora(DateTime fecha);

    /// <summary>
    /// Aplica la rotación por fecha: mueve a la subcarpeta <c>historico</c> los archivos
    /// diarios más antiguos que la retención indicada, de modo que la carpeta principal
    /// sólo conserve los días recientes.
    /// </summary>
    /// <param name="diasARetener">Cantidad de días que se conservan sin rotar.</param>
    /// <returns>Rutas de los archivos que se movieron.</returns>
    IReadOnlyList<string> Rotar(int diasARetener);
}
