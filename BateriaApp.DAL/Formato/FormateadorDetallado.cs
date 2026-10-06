using System.Text;
using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

// Otro formato posible: cada evento ocupa varios renglones con todo el detalle.
public class FormateadorDetallado : IFormateadorRegistro
{
    public string Nombre
    {
        get { return "Detallado"; }
    }

    public string Formatear(EstadoBateria estado)
    {
        StringBuilder texto = new StringBuilder();

        texto.AppendLine("--- evento ---");
        texto.AppendLine("Fecha: " + estado.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"));
        texto.AppendLine("Conectada: " + (estado.Conectado ? "SI" : "NO"));
        texto.AppendLine("Carga: " + estado.Carga + " %");
        texto.AppendLine("Tiempo de carga: " + estado.TiempoCarga + " min");
        texto.AppendLine("Tiempo de uso: " + estado.TiempoUso + " min");

        return texto.ToString();
    }
}
