using BateriaApp.BLL;
using BateriaApp.Domain;
using BateriaApp.Domain.Exceptions;

namespace BateriaApp.UI;

/// <summary>
/// Capa de presentación: único proyecto ejecutable de la solución.
/// Arma el servicio a través de la fábrica, registra los dos observadores concretos
/// (<see cref="SuscriptorVisual"/>, de esta capa, y <c>SuscriptorBitacora</c>, de BLL)
/// y brinda una demostración automática más un menú interactivo de prueba.
/// </summary>
internal static class Program
{
    /// <summary>Carpeta donde se guardan los archivos de bitácora.</summary>
    private const string CarpetaBitacoras = "Bitacoras";

    /// <summary>Días de bitácora que se conservan sin rotar.</summary>
    private const int DiasRetencionBitacoras = 30;

    /// <summary>Punto de entrada de la aplicación.</summary>
    /// <param name="args">
    /// Argumentos de línea de comandos. Con <c>--demo</c> sólo se ejecuta la demostración
    /// automática (sin menú), lo que permite correr el programa en forma no interactiva.
    /// </param>
    /// <returns>0 si la ejecución finalizó correctamente.</returns>
    private static int Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch (IOException)
        {
            // Algunas consolas no permiten cambiar la codificación; se continúa igual.
        }

        MostrarEncabezado();

        bool soloDemostracion = args.Any(argumento => argumento.Equals("--demo", StringComparison.OrdinalIgnoreCase));

        ServicioBateria servicio = FabricaBateria.Crear(
            cargaInicial: 50,
            conectado: false,
            carpetaBitacoras: CarpetaBitacoras,
            formato: FormatoBitacora.TextoPlano);

        var suscriptorVisual = new SuscriptorVisual();

        // El fallo de un observador se informa por pantalla, pero no interrumpe la
        // notificación de los demás suscriptores.
        servicio.ErrorNotificacion += (_, datos) =>
            Consola.Error($"El observador '{datos.Observador.Nombre}' falló: {datos.Excepcion.Message}");

        Consola.Info($"Carpeta de bitácoras : {Path.GetFullPath(servicio.CarpetaBitacoras)}");
        Consola.Info("Formato de bitácora  : texto plano (una línea por evento)");
        Consola.Info("Estado inicial       : 50 % de carga, desconectada");

        Consola.Accion("Suscribiendo observadores: SuscriptorVisual (UI) y SuscriptorBitacora (BLL)");
        servicio.Suscribir(suscriptorVisual);
        servicio.Suscribir(servicio.Bitacora);
        Consola.Exito($"Observadores suscriptos: {servicio.CantidadSuscriptores}");

        EjecutarDemostracion(servicio, suscriptorVisual);

        if (!soloDemostracion)
        {
            EjecutarMenu(servicio, suscriptorVisual);
        }

        MostrarResumen(servicio, suscriptorVisual);

        return 0;
    }

    /// <summary>Muestra el encabezado institucional de la aplicación.</summary>
    private static void MostrarEncabezado()
    {
        Consola.Titulo("INSTITUTO UNIVERSITARIO LEONARDO DA VINCI");
        Consola.Info("Carrera     : Analista en Sistemas Informáticos");
        Consola.Info("Asignatura  : Prácticas Profesionalizantes III");
        Consola.Info("Instancia   : Examen Recuperatorio");
        Consola.Info("Ejercicio   : Patrón Observer aplicado a la clase Bateria");
        Consola.Info("Alumno      : Dante Schlögl");
        Consola.Info("Arquitectura: UI - BLL - DAL - Domain (4 capas)");
    }

    /// <summary>
    /// Recorre automáticamente los escenarios más importantes: suscripción, notificación
    /// al cambiar el estado de conexión, cálculo de tiempos, desuscripción, re-suscripción
    /// y validación de rangos.
    /// </summary>
    /// <param name="servicio">Servicio de la batería.</param>
    /// <param name="suscriptorVisual">Observador visual, para poder suscribirlo y desuscribirlo.</param>
    private static void EjecutarDemostracion(ServicioBateria servicio, SuscriptorVisual suscriptorVisual)
    {
        Consola.Titulo("PARTE 1 - DEMOSTRACION AUTOMATICA");

        Consola.Accion("Paso 1: se conecta el cargador (Conectado = true) -> notifica a los dos observadores");
        servicio.Conectar();

        Consola.Accion("Paso 2: la carga sube al 80 % -> se recalcula el tiempo de carga");
        servicio.CambiarCarga(80);

        Consola.Accion("Paso 3: la batería llega al 100 % -> el tiempo de carga debe ser 0");
        servicio.CambiarCarga(100);

        Consola.Accion("Paso 4: se desconecta el cargador (Conectado = false) -> ahora se informa el tiempo de uso");
        servicio.Desconectar();

        Consola.Accion("Paso 5: la carga baja al 25 % -> se recalcula el tiempo estimado de uso");
        servicio.CambiarCarga(25);

        Consola.Accion("Paso 6: se desuscribe el SuscriptorVisual (la bitácora sigue activa)");
        bool seDioDeBaja = servicio.Desuscribir(suscriptorVisual);
        Consola.Exito($"Desuscripción {(seDioDeBaja ? "correcta" : "fallida")}. Observadores suscriptos: {servicio.CantidadSuscriptores}");

        Consola.Accion("Paso 7: cambia el estado SIN el observador visual -> sólo lo registra la bitácora");
        servicio.Conectar();
        servicio.CambiarCarga(60);

        Consola.Accion("Paso 8: se vuelve a suscribir el SuscriptorVisual -> recibe el estado actual de inmediato");
        servicio.Suscribir(suscriptorVisual);

        Consola.Accion("Paso 9: se intenta asignar una carga inválida (150 %) para mostrar la validación");
        try
        {
            servicio.CambiarCarga(150);
            Consola.Advertencia("No se lanzó la excepción esperada.");
        }
        catch (CargaFueraDeRangoException excepcion)
        {
            Consola.Error("Excepción controlada: " + excepcion.Message);
        }

        Consola.Accion("Paso 10: valor inválido negativo (-10 %)");
        try
        {
            servicio.CambiarCarga(-10);
            Consola.Advertencia("No se lanzó la excepción esperada.");
        }
        catch (CargaFueraDeRangoException excepcion)
        {
            Consola.Error("Excepción controlada: " + excepcion.Message);
        }

        Consola.Exito($"Estado final de la demostración: {servicio.EstadoActual.Carga} % - {servicio.EstadoActual.DescripcionEstado}");
    }

    /// <summary>Muestra el menú interactivo y atiende la opción elegida hasta que se sale.</summary>
    /// <param name="servicio">Servicio de la batería.</param>
    /// <param name="suscriptorVisual">Observador visual, para poder suscribirlo y desuscribirlo.</param>
    private static void EjecutarMenu(ServicioBateria servicio, SuscriptorVisual suscriptorVisual)
    {
        while (true)
        {
            Consola.Titulo("PARTE 2 - MENU INTERACTIVO");
            Consola.Info("1) Conectar el cargador");
            Consola.Info("2) Desconectar el cargador");
            Consola.Info("3) Establecer el porcentaje de carga");
            Consola.Info("4) Suscribir el SuscriptorVisual");
            Consola.Info("5) Desuscribir el SuscriptorVisual");
            Consola.Info("6) Forzar una notificación (Notificar)");
            Consola.Info("7) Leer la bitácora del día");
            Consola.Info("8) Rotar las bitácoras anteriores");
            Consola.Info("0) Salir");
            Console.WriteLine();
            Console.Write("  Opción: ");

            string? opcion = Console.ReadLine();

            if (opcion is null)
            {
                Console.WriteLine();
                Consola.Info("Entrada finalizada (fin de la entrada estándar). Se cierra el menú.");
                return;
            }

            Console.WriteLine();

            if (opcion.Trim() == "0")
            {
                return;
            }

            try
            {
                EjecutarOpcionDelMenu(opcion.Trim(), servicio, suscriptorVisual);
            }
            catch (CargaFueraDeRangoException excepcion)
            {
                Consola.Error("Carga inválida: " + excepcion.Message);
            }
            catch (Exception excepcion)
            {
                Consola.Error("Error inesperado: " + excepcion.Message);
            }
        }
    }

    /// <summary>Ejecuta la opción elegida en el menú.</summary>
    /// <param name="opcion">Texto ingresado por el usuario.</param>
    /// <param name="servicio">Servicio de la batería.</param>
    /// <param name="suscriptorVisual">Observador visual.</param>
    private static void EjecutarOpcionDelMenu(string opcion, ServicioBateria servicio, SuscriptorVisual suscriptorVisual)
    {
        switch (opcion)
        {
            case "1":
                servicio.Conectar();
                break;

            case "2":
                servicio.Desconectar();
                break;

            case "3":
                Console.Write("  Indique la nueva carga (0 a 100): ");
                string? ingresado = Console.ReadLine();

                if (ingresado is null)
                {
                    Consola.Info("Entrada finalizada.");
                    return;
                }

                if (!int.TryParse(ingresado.Trim(), out int carga))
                {
                    Consola.Error($"'{ingresado.Trim()}' no es un número entero válido.");
                    return;
                }

                servicio.CambiarCarga(carga);
                break;

            case "4":
                servicio.Suscribir(suscriptorVisual);
                Consola.Exito($"SuscriptorVisual suscripto. Observadores: {servicio.CantidadSuscriptores}");
                break;

            case "5":
                bool dadoDeBaja = servicio.Desuscribir(suscriptorVisual);
                Consola.Info($"SuscriptorVisual {(dadoDeBaja ? "desuscripto" : "no estaba suscripto")}. Observadores: {servicio.CantidadSuscriptores}");
                break;

            case "6":
                servicio.Notificar();
                Consola.Exito("Notificación forzada a todos los suscriptores.");
                break;

            case "7":
                MostrarBitacoraDelDia(servicio);
                break;

            case "8":
                IReadOnlyList<string> movidos = servicio.RotarBitacoras(DiasRetencionBitacoras);
                Consola.Exito(movidos.Count == 0
                    ? $"No había bitácoras con más de {DiasRetencionBitacoras} días para rotar."
                    : $"Se rotaron {movidos.Count} archivo(s) a la subcarpeta 'historico'.");
                break;

            default:
                Consola.Advertencia($"La opción '{opcion}' no es válida.");
                break;
        }
    }

    /// <summary>Muestra por pantalla el contenido de la bitácora del día.</summary>
    /// <param name="servicio">Servicio de la batería.</param>
    private static void MostrarBitacoraDelDia(ServicioBateria servicio)
    {
        DateTime hoy = DateTime.Today;
        IReadOnlyList<string> renglones = servicio.LeerBitacora(hoy);

        Consola.Info("Archivo: " + Path.GetFullPath(servicio.ObtenerRutaBitacora(hoy)));

        if (renglones.Count == 0)
        {
            Consola.Info("Todavía no hay registros para la fecha de hoy.");
            return;
        }

        Consola.Separador();

        foreach (string renglon in renglones)
        {
            Console.WriteLine("  " + renglon);
        }

        Consola.Separador();
    }

    /// <summary>Muestra el resumen final: contenido de la bitácora, contadores y rotación.</summary>
    /// <param name="servicio">Servicio de la batería.</param>
    /// <param name="suscriptorVisual">Observador visual, para informar cuántas notificaciones recibió.</param>
    private static void MostrarResumen(ServicioBateria servicio, SuscriptorVisual suscriptorVisual)
    {
        Consola.Titulo("BITACORA GENERADA EN ESTA EJECUCION");
        MostrarBitacoraDelDia(servicio);

        Consola.Titulo("RESUMEN");
        Consola.Info($"Actualizaciones recibidas por el SuscriptorVisual : {suscriptorVisual.Actualizaciones}");
        Consola.Info($"Eventos registrados por el SuscriptorBitacora      : {servicio.Bitacora.EventosRegistrados}");
        Consola.Info($"Observadores suscriptos al finalizar                : {servicio.CantidadSuscriptores}");

        IReadOnlyList<string> movidos = servicio.RotarBitacoras(DiasRetencionBitacoras);
        Consola.Info($"Rotación de bitácoras (retención {DiasRetencionBitacoras} días)     : {movidos.Count} archivo(s) movido(s)");
        Consola.Info("Fin del programa.");
    }
}
