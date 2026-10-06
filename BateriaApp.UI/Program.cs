using BateriaApp.BLL;
using BateriaApp.Domain.Exceptions;

namespace BateriaApp.UI;

// Programa principal. Primero corre una demostracion sola y despues
// muestra un menu para probar la bateria a mano.
internal static class Program
{
    private const string CarpetaBitacoras = "Bitacoras";

    private static int Main(string[] args)
    {
        Console.WriteLine("INSTITUTO UNIVERSITARIO LEONARDO DA VINCI");
        Console.WriteLine("Practicas Profesionalizantes III - Recuperatorio");
        Console.WriteLine("Patron Observer aplicado a la clase Bateria");
        Console.WriteLine("Alumno: Dante Schlogl");
        Console.WriteLine();

        // Con --demo corre solo la demostracion y sale, sin menu.
        bool soloDemo = false;

        foreach (string arg in args)
        {
            if (arg.ToLower() == "--demo") soloDemo = true;
        }

        // Armo el servicio y los dos suscriptores.
        ServicioBateria servicio = FabricaBateria.Crear(50, false, CarpetaBitacoras, FormatoBitacora.TextoPlano);
        SuscriptorVisual visual = new SuscriptorVisual();

        servicio.ErrorNotificacion += mensaje => Console.WriteLine("Ojo: " + mensaje);

        Console.WriteLine("Carpeta de bitacoras: " + Path.GetFullPath(servicio.CarpetaBitacoras));
        Console.WriteLine("Arranco con 50 % de carga y desconectada.");

        servicio.Suscribir(visual);
        servicio.Suscribir(servicio.Bitacora);
        Console.WriteLine("Suscriptos: " + servicio.CantidadSuscriptores + " (visual y bitacora)");

        Demostracion(servicio, visual);

        if (!soloDemo)
        {
            Menu(servicio, visual);
        }

        Resumen(servicio, visual);

        return 0;
    }

    // Recorro los casos mas importantes del ejercicio uno por uno.
    private static void Demostracion(ServicioBateria servicio, SuscriptorVisual visual)
    {
        Console.WriteLine();
        Console.WriteLine("=== Demostracion ===");

        Console.WriteLine();
        Console.WriteLine("Paso 1: conecto el cargador.");
        servicio.Conectar();

        Console.WriteLine();
        Console.WriteLine("Paso 2: subo la carga al 80 %.");
        servicio.CambiarCarga(80);

        Console.WriteLine();
        Console.WriteLine("Paso 3: la lleno al 100 %.");
        servicio.CambiarCarga(100);

        Console.WriteLine();
        Console.WriteLine("Paso 4: desconecto el cargador.");
        servicio.Desconectar();

        Console.WriteLine();
        Console.WriteLine("Paso 5: bajo la carga al 25 %.");
        servicio.CambiarCarga(25);

        Console.WriteLine();
        Console.WriteLine("Paso 6: desuscribo el visual, queda solo la bitacora.");
        servicio.Desuscribir(visual);
        Console.WriteLine("Suscriptos ahora: " + servicio.CantidadSuscriptores);

        Console.WriteLine();
        Console.WriteLine("Paso 7: cambio el estado sin el visual.");
        servicio.Conectar();
        servicio.CambiarCarga(60);

        Console.WriteLine();
        Console.WriteLine("Paso 8: vuelvo a suscribir el visual.");
        servicio.Suscribir(visual);

        Console.WriteLine();
        Console.WriteLine("Paso 9: pruebo una carga invalida (150).");
        try
        {
            servicio.CambiarCarga(150);
        }
        catch (CargaFueraDeRangoException ex)
        {
            Console.WriteLine("Se controlo el error: " + ex.Message);
        }
    }

    // Menu simple para probar a mano.
    private static void Menu(ServicioBateria servicio, SuscriptorVisual visual)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Menu ===");
            Console.WriteLine("1) Conectar el cargador");
            Console.WriteLine("2) Desconectar el cargador");
            Console.WriteLine("3) Cambiar la carga");
            Console.WriteLine("4) Suscribir el visual");
            Console.WriteLine("5) Desuscribir el visual");
            Console.WriteLine("6) Forzar el aviso a todos");
            Console.WriteLine("7) Ver la bitacora de hoy");
            Console.WriteLine("8) Rotar bitacoras viejas");
            Console.WriteLine("0) Salir");
            Console.Write("Opcion: ");

            string? opcion = Console.ReadLine();

            // Si ya no hay entrada (por ejemplo al correrlo por un pipe) salgo sin error.
            if (opcion == null)
            {
                Console.WriteLine();
                Console.WriteLine("No hay mas entrada, salgo.");
                return;
            }

            opcion = opcion.Trim();

            if (opcion == "0") return;

            Console.WriteLine();

            try
            {
                Opcion(opcion, servicio, visual);
            }
            catch (CargaFueraDeRangoException ex)
            {
                Console.WriteLine("Carga invalida: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    // Ejecuto la opcion que eligio el usuario.
    private static void Opcion(string opcion, ServicioBateria servicio, SuscriptorVisual visual)
    {
        if (opcion == "1")
        {
            servicio.Conectar();
        }
        else if (opcion == "2")
        {
            servicio.Desconectar();
        }
        else if (opcion == "3")
        {
            Console.Write("Nueva carga (0 a 100): ");
            string? texto = Console.ReadLine();

            if (texto == null) return;

            int carga;

            if (!int.TryParse(texto.Trim(), out carga))
            {
                Console.WriteLine("Eso no es un numero.");
                return;
            }

            servicio.CambiarCarga(carga);
        }
        else if (opcion == "4")
        {
            servicio.Suscribir(visual);
            Console.WriteLine("Suscriptos: " + servicio.CantidadSuscriptores);
        }
        else if (opcion == "5")
        {
            bool estaba = servicio.Desuscribir(visual);

            if (estaba)
            {
                Console.WriteLine("Visual desuscripto.");
            }
            else
            {
                Console.WriteLine("El visual no estaba suscripto.");
            }

            Console.WriteLine("Suscriptos: " + servicio.CantidadSuscriptores);
        }
        else if (opcion == "6")
        {
            servicio.Notificar();
        }
        else if (opcion == "7")
        {
            MostrarBitacora(servicio);
        }
        else if (opcion == "8")
        {
            IReadOnlyList<string> movidos = servicio.RotarBitacoras(30);
            Console.WriteLine("Archivos rotados: " + movidos.Count);
        }
        else
        {
            Console.WriteLine("Opcion invalida.");
        }
    }

    // Muestro la bitacora del dia de hoy.
    private static void MostrarBitacora(ServicioBateria servicio)
    {
        DateTime hoy = DateTime.Today;

        Console.WriteLine("Archivo: " + Path.GetFullPath(servicio.ObtenerRutaBitacora(hoy)));

        IReadOnlyList<string> lineas = servicio.LeerBitacora(hoy);

        if (lineas.Count == 0)
        {
            Console.WriteLine("Todavia no hay eventos.");
            return;
        }

        foreach (string linea in lineas)
        {
            Console.WriteLine(linea);
        }
    }

    // Muestro la bitacora y dos contadores para comprobar que el patron funciona.
    private static void Resumen(ServicioBateria servicio, SuscriptorVisual visual)
    {
        Console.WriteLine();
        Console.WriteLine("=== Bitacora de hoy ===");
        MostrarBitacora(servicio);

        Console.WriteLine();
        Console.WriteLine("=== Resumen ===");
        Console.WriteLine("Avisos que recibio el visual: " + visual.Actualizaciones);
        Console.WriteLine("Eventos guardados en la bitacora: " + servicio.Bitacora.EventosRegistrados);
        Console.WriteLine("Suscriptos al final: " + servicio.CantidadSuscriptores);

        IReadOnlyList<string> movidos = servicio.RotarBitacoras(30);
        Console.WriteLine("Bitacoras rotadas (mas de 30 dias): " + movidos.Count);

        Console.WriteLine();
        Console.WriteLine("Fin del programa.");
    }
}
