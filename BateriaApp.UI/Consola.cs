namespace BateriaApp.UI;

/// <summary>
/// Utilidades de presentación por consola: títulos, colores y separadores.
/// Se centralizan acá para que la salida del programa sea uniforme y fácil de leer.
/// </summary>
internal static class Consola
{
    /// <summary>Ancho interior de los recuadros que dibuja la aplicación.</summary>
    public const int AnchoInterno = 54;

    /// <summary>
    /// Indica si la salida está conectada a una consola real. Cuando la salida se
    /// redirige a un archivo o a un pipe no se aplican colores, para no ensuciar el texto.
    /// </summary>
    private static bool _admiteColor = !Console.IsOutputRedirected;

    /// <summary>Ejecuta una acción aplicando un color, restaurando el color anterior al finalizar.</summary>
    /// <param name="color">Color a aplicar.</param>
    /// <param name="accion">Acción a ejecutar.</param>
    public static void ConColor(ConsoleColor color, Action accion)
    {
        if (!_admiteColor)
        {
            accion();
            return;
        }

        ConsoleColor anterior = Console.ForegroundColor;

        try
        {
            Console.ForegroundColor = color;
        }
        catch (IOException)
        {
            // Algunas consolas no admiten cambios de color: se sigue sin color.
            _admiteColor = false;
            accion();
            return;
        }

        try
        {
            accion();
        }
        finally
        {
            try
            {
                Console.ForegroundColor = anterior;
            }
            catch (IOException)
            {
                _admiteColor = false;
            }
        }
    }

    /// <summary>Escribe una línea con color.</summary>
    /// <param name="texto">Texto a mostrar.</param>
    /// <param name="color">Color con el que se muestra.</param>
    public static void Linea(string texto = "", ConsoleColor color = ConsoleColor.Gray)
        => ConColor(color, () => Console.WriteLine(texto));

    /// <summary>Muestra un título principal enmarcado.</summary>
    /// <param name="texto">Título a mostrar.</param>
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Linea("  +" + new string('=', AnchoInterno + 2) + "+", ConsoleColor.Cyan);
        Linea("  | " + texto.PadRight(AnchoInterno) + " |", ConsoleColor.Cyan);
        Linea("  +" + new string('=', AnchoInterno + 2) + "+", ConsoleColor.Cyan);
    }

    /// <summary>Muestra un separador horizontal simple.</summary>
    public static void Separador()
        => Linea("  " + new string('-', AnchoInterno), ConsoleColor.DarkGray);

    /// <summary>Muestra un texto informativo.</summary>
    /// <param name="texto">Texto a mostrar.</param>
    public static void Info(string texto)
        => Linea("  " + texto, ConsoleColor.Gray);

    /// <summary>Muestra el detalle de una acción que se está por ejecutar.</summary>
    /// <param name="texto">Descripción de la acción.</param>
    public static void Accion(string texto)
    {
        Console.WriteLine();
        Linea("  >> " + texto, ConsoleColor.White);
    }

    /// <summary>Muestra un mensaje de advertencia.</summary>
    /// <param name="texto">Texto a mostrar.</param>
    public static void Advertencia(string texto)
        => Linea("  [!] " + texto, ConsoleColor.Yellow);

    /// <summary>Muestra un mensaje de error o de excepción controlada.</summary>
    /// <param name="texto">Texto a mostrar.</param>
    public static void Error(string texto)
        => Linea("  [X] " + texto, ConsoleColor.Red);

    /// <summary>Muestra un mensaje de éxito.</summary>
    /// <param name="texto">Texto a mostrar.</param>
    public static void Exito(string texto)
        => Linea("  [OK] " + texto, ConsoleColor.Green);
}
