using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

/// <summary>
/// Formato por omisión: un renglón por evento, con fecha y hora, carga y estado.
/// Es el más adecuado para una bitácora porque permite leerla y compararla de un vistazo.
/// </summary>
public sealed class FormateadorTextoPlano : IFormateadorRegistro
{
    /// <inheritdoc />
    public string Nombre => "Texto plano (una línea por evento)";

    /// <inheritdoc />
    public string Formatear(RegistroBitacora registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        string estado = registro.Conectado
            ? registro.Carga >= 100
                ? "CARGADA COMPLETA"
                : $"CARGANDO (faltan {registro.TiempoCarga} min)"
            : $"EN USO (quedan {registro.TiempoUso} min)";

        return $"[{registro.FechaHora:yyyy-MM-dd HH:mm:ss}] {registro.Carga,3}% | {estado,-26} | origen: {registro.Origen}";
    }
}
