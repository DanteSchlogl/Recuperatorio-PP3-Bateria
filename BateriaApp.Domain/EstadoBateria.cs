namespace BateriaApp.Domain;

// Es una foto del estado de la bateria en un momento dado.
// Se la paso a los observadores para que solo lean y no puedan modificar nada.
public class EstadoBateria
{
    public bool Conectado { get; set; }
    public int Carga { get; set; }
    public int TiempoCarga { get; set; }
    public int TiempoUso { get; set; }
    public DateTime FechaHora { get; set; }

    // Texto corto del estado, listo para mostrar en pantalla o guardar en la bitacora.
    public string DescripcionEstado
    {
        get
        {
            if (!Conectado) return "DESCONECTADA - EN USO";
            if (Carga >= 100) return "CONECTADA - CARGA COMPLETA";

            return "CONECTADA - CARGANDO";
        }
    }
}
