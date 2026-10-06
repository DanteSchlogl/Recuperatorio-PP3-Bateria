using BateriaApp.Domain;

namespace BateriaApp.DAL.Formato;

// Formato simple: un renglon por evento.
public class FormateadorTextoPlano : IFormateadorRegistro
{
    public string Nombre
    {
        get { return "Texto plano"; }
    }

    public string Formatear(EstadoBateria estado)
    {
        string situacion;

        if (!estado.Conectado)
        {
            situacion = "EN USO (quedan " + estado.TiempoUso + " min)";
        }
        else if (estado.Carga >= 100)
        {
            situacion = "CARGADA COMPLETA";
        }
        else
        {
            situacion = "CARGANDO (faltan " + estado.TiempoCarga + " min)";
        }

        return "[" + estado.FechaHora.ToString("dd/MM/yyyy HH:mm:ss") + "] " + estado.Carga + "% - " + situacion;
    }
}
