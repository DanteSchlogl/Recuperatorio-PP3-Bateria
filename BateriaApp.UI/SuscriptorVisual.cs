using BateriaApp.Domain;

namespace BateriaApp.UI;

/// <summary>
/// Observador concreto (patrón Observer) que muestra por consola el estado de la batería.
/// Vive en la capa de presentación porque su única responsabilidad es visual: no calcula
/// nada, sólo da formato a la información que el sujeto le entrega.
/// </summary>
public class SuscriptorVisual : IObservadorBateria
{
    private const int AnchoBarra = 24;

    /// <summary>Crea el suscriptor visual.</summary>
    /// <param name="nombre">Nombre simbólico del observador.</param>
    public SuscriptorVisual(string nombre = "SuscriptorVisual") => Nombre = nombre;

    /// <inheritdoc />
    public string Nombre { get; }

    /// <summary>Cantidad de actualizaciones (notificaciones) recibidas.</summary>
    public int Actualizaciones { get; private set; }

    /// <inheritdoc />
    /// <remarks>
    /// La salida es diferenciada: verde y con tiempo restante de carga cuando la batería
    /// está cargando; amarillo y con tiempo estimado de uso cuando trabaja sin tensión.
    /// </remarks>
    public void Actualizar(EstadoBateria estado)
    {
        ArgumentNullException.ThrowIfNull(estado);

        Actualizaciones++;

        ConsoleColor color = estado.Conectado ? ConsoleColor.Green : ConsoleColor.Yellow;

        Consola.ConColor(color, () =>
        {
            Console.WriteLine();
            EscribirBorde();
            EscribirFila("SUSCRIPTOR VISUAL - CAMBIO DE ESTADO");
            EscribirBorde();
            EscribirFila("Hora", estado.FechaHora.ToString("dd/MM/yyyy HH:mm:ss"));
            EscribirFila("Estado", estado.DescripcionEstado);
            EscribirFila("Porcentaje de carga", estado.Carga + " %");
            EscribirBarra(estado.Carga);

            // Visualización diferenciada carga vs. uso (criterio de evaluación).
            if (estado.EstaCargando)
            {
                EscribirFila("Tiempo para el 100 %", estado.TiempoCarga + " min");
            }
            else if (estado.EstaCompleta && estado.Conectado)
            {
                EscribirFila("Tiempo para el 100 %", "carga completa (0 min)");
            }

            if (estado.EstaEnUso)
            {
                EscribirFila("Tiempo estimado de uso", estado.TiempoUso + " min");
            }

            EscribirBorde();
        });
    }

    /// <summary>Dibuja una fila del recuadro con una etiqueta y su valor.</summary>
    /// <param name="etiqueta">Etiqueta de la izquierda.</param>
    /// <param name="valor">Valor de la derecha (opcional).</param>
    private static void EscribirFila(string etiqueta, string? valor = null)
    {
        string contenido = valor is null
            ? etiqueta
            : (etiqueta + ": ").PadRight(24) + valor;

        Console.WriteLine("  | " + contenido.PadRight(Consola.AnchoInterno) + " |");
    }

    /// <summary>Dibuja una barra de progreso proporcional a la carga.</summary>
    /// <param name="carga">Carga en tanto por ciento.</param>
    private static void EscribirBarra(int carga)
    {
        int llenos = (int)Math.Round(carga / 100.0 * AnchoBarra);
        llenos = Math.Clamp(llenos, 0, AnchoBarra);

        string barra = new string('#', llenos) + new string('.', AnchoBarra - llenos);
        string contenido = "[".PadRight(24) + barra + " ]";

        Console.WriteLine("  | " + contenido.PadRight(Consola.AnchoInterno) + " |");
    }

    /// <summary>Dibuja una línea horizontal del recuadro.</summary>
    private static void EscribirBorde()
        => Console.WriteLine("  +" + new string('-', Consola.AnchoInterno + 2) + "+");
}
