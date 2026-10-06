using BateriaApp.Domain;

namespace BateriaApp.UI;

// Suscriptor que muestra el estado de la bateria en la consola.
// Solo muestra: no calcula nada, todo se lo da la bateria.
public class SuscriptorVisual : IObservadorBateria
{
    public string Nombre
    {
        get { return "Visual"; }
    }

    // Cuento cuantas veces me avisaron, para el resumen final.
    public int Actualizaciones { get; private set; }

    public void Actualizar(EstadoBateria estado)
    {
        Actualizaciones++;

        Console.WriteLine();
        Console.WriteLine("--- cambio de estado ---");
        Console.WriteLine("Hora: " + estado.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"));
        Console.WriteLine("Estado: " + estado.DescripcionEstado);
        Console.WriteLine("Carga: " + estado.Carga + " %");

        // Si esta enchufada muestro lo que falta para cargar, si no lo que queda de uso.
        if (estado.Conectado)
        {
            Console.WriteLine("Tiempo de carga: " + estado.TiempoCarga + " min");
        }
        else
        {
            Console.WriteLine("Tiempo de uso: " + estado.TiempoUso + " min");
        }
    }
}
