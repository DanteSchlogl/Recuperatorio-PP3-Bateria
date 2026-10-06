using BateriaApp.Domain;

namespace BateriaApp.DAL;

// Lo que necesita la capa de negocio para guardar la bitacora.
// No dice como ni donde se guarda, eso lo decide la clase que lo implementa.
public interface IRepositorioBitacora
{
    string Carpeta { get; }

    int EventosRegistrados { get; }

    // Me dice la ruta del archivo que le toca a esa fecha.
    string ObtenerRutaArchivo(DateTime fecha);

    // Guarda un evento.
    void Registrar(EstadoBateria estado);

    // Devuelve las lineas de la bitacora de una fecha.
    IReadOnlyList<string> LeerBitacora(DateTime fecha);

    // Mueve a la carpeta "historico" los archivos mas viejos que los dias indicados.
    IReadOnlyList<string> Rotar(int diasARetener);
}
