using System.Text;
using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

/// <summary>
/// Formato alternativo: cada evento se escribe como un bloque de varios renglones con
/// todos los datos de la batería detallados. Demuestra que el formato es intercambiable
/// sin modificar ni una línea del repositorio.
/// </summary>
public sealed class FormateadorDetallado : IFormateadorRegistro
{
    /// <inheritdoc />
    public string Nombre => "Detallado (bloque multilínea)";

    /// <inheritdoc />
    public string Formatear(RegistroBitacora registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        var constructor = new StringBuilder();

        constructor.AppendLine("=== EVENTO DE BATERIA ===");
        constructor.AppendLine($"  Fecha y hora    : {registro.FechaHora:dd/MM/yyyy HH:mm:ss}");
        constructor.AppendLine($"  Origen          : {registro.Origen}");
        constructor.AppendLine($"  Conectada       : {(registro.Conectado ? "SI" : "NO")}");
        constructor.AppendLine($"  Carga           : {registro.Carga} %");
        constructor.AppendLine($"  Tiempo de carga : {registro.TiempoCarga} min");
        constructor.AppendLine($"  Tiempo de uso   : {registro.TiempoUso} min");

        return constructor.ToString();
    }
}
