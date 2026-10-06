using System.Globalization;
using BateriaApp.DAL.Excepciones;
using BateriaApp.DAL.Formato;
using BateriaApp.Domain;

namespace BateriaApp.DAL;

/// <summary>
/// Implementación concreta del repositorio sobre archivos de texto.
/// <para>
/// <b>Rotación de archivos por fecha:</b> cada día se escribe en su propio archivo
/// (<c>bitacora_AAAA-MM-DD.txt</c>) dentro de la carpeta configurada. De esta manera la
/// rotación queda garantizada sin procesos externos ni temporizadores: al cambiar el día,
/// el evento se deriva automáticamente al archivo nuevo. El método <see cref="Rotar"/>
/// permite además apartar a la subcarpeta <c>historico</c> los archivos ya antiguos.
/// </para>
/// </summary>
public class RepositorioBitacoraArchivo : IRepositorioBitacora
{
    private const string PrefijoArchivo = "bitacora_";
    private const string SufijoArchivo = ".txt";
    private const string NombreSubcarpetaHistorico = "historico";

    private readonly IFormateadorRegistro _formateador;

    /// <summary>Objeto de sincronización: protege el acceso al archivo ante escrituras concurrentes.</summary>
    private readonly object _candado = new();

    /// <summary>Crea el repositorio.</summary>
    /// <param name="carpeta">Carpeta donde se guardarán las bitácoras. Por omisión, <c>Bitacoras</c>.</param>
    /// <param name="formateador">Estrategia de formato a utilizar. Por omisión, texto plano.</param>
    public RepositorioBitacoraArchivo(string? carpeta = null, IFormateadorRegistro? formateador = null)
    {
        Carpeta = string.IsNullOrWhiteSpace(carpeta) ? "Bitacoras" : carpeta.Trim();
        _formateador = formateador ?? new FormateadorTextoPlano();
    }

    /// <inheritdoc />
    public string Carpeta { get; }

    /// <inheritdoc />
    public DateTime? FechaUltimoRegistro { get; private set; }

    /// <inheritdoc />
    public int EventosRegistrados { get; private set; }

    /// <summary>Estrategia de formato actualmente en uso.</summary>
    public IFormateadorRegistro Formateador => _formateador;

    /// <inheritdoc />
    public string ObtenerRutaArchivo(DateTime fecha)
        => Path.Combine(Carpeta, $"{PrefijoArchivo}{fecha:yyyy-MM-dd}{SufijoArchivo}");

    /// <inheritdoc />
    public void Registrar(RegistroBitacora registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        string ruta = ObtenerRutaArchivo(registro.FechaHora);
        string texto = _formateador.Formatear(registro);

        try
        {
            lock (_candado)
            {
                // Creación de la carpeta si todavía no existe (manejo de archivos).
                Directory.CreateDirectory(Carpeta);

                // Aseguramos un único salto de línea final, sin importar el formateador elegido.
                string contenido = texto.EndsWith(Environment.NewLine, StringComparison.Ordinal)
                    ? texto
                    : texto + Environment.NewLine;

                // Apertura en modo "append": se crea el archivo si no existe y se
                // actualiza si ya existe, sin sobrescribir lo anterior.
                File.AppendAllText(ruta, contenido);
            }

            FechaUltimoRegistro = registro.FechaHora;
            EventosRegistrados++;
        }
        catch (Exception excepcion) when (excepcion is IOException
                                             or UnauthorizedAccessException
                                             or NotSupportedException
                                             or ArgumentException)
        {
            throw new BitacoraException(
                $"No se pudo escribir la bitácora en '{Path.GetFullPath(ruta)}': {excepcion.Message}",
                ruta,
                excepcion);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> LeerBitacora(DateTime fecha)
    {
        string ruta = ObtenerRutaArchivo(fecha);

        try
        {
            if (!File.Exists(ruta))
            {
                return Array.Empty<string>();
            }

            return File.ReadAllLines(ruta);
        }
        catch (Exception excepcion) when (excepcion is IOException
                                             or UnauthorizedAccessException
                                             or NotSupportedException
                                             or ArgumentException)
        {
            throw new BitacoraException(
                $"No se pudo leer la bitácora en '{Path.GetFullPath(ruta)}': {excepcion.Message}",
                ruta,
                excepcion);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Rotar(int diasARetener)
    {
        if (diasARetener < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(diasARetener), diasARetener, "La cantidad de días a retener no puede ser negativa.");
        }

        var movidos = new List<string>();

        try
        {
            lock (_candado)
            {
                if (!Directory.Exists(Carpeta))
                {
                    return movidos;
                }

                DateTime limite = DateTime.Today.AddDays(-diasARetener);

                foreach (string archivo in Directory.GetFiles(Carpeta, $"{PrefijoArchivo}*{SufijoArchivo}"))
                {
                    if (!TryObtenerFechaDelArchivo(archivo, out DateTime fechaArchivo) || fechaArchivo >= limite)
                    {
                        continue;
                    }

                    string carpetaHistorico = Path.Combine(Carpeta, NombreSubcarpetaHistorico);
                    Directory.CreateDirectory(carpetaHistorico);

                    string destino = Path.Combine(carpetaHistorico, Path.GetFileName(archivo));

                    if (File.Exists(destino))
                    {
                        File.Delete(destino);
                    }

                    File.Move(archivo, destino);
                    movidos.Add(destino);
                }
            }
        }
        catch (Exception excepcion) when (excepcion is IOException
                                             or UnauthorizedAccessException
                                             or NotSupportedException
                                             or ArgumentException)
        {
            throw new BitacoraException(
                $"No se pudo rotar la bitácora de la carpeta '{Carpeta}': {excepcion.Message}",
                Carpeta,
                excepcion);
        }

        return movidos;
    }

    /// <summary>
    /// Extrae la fecha del nombre de un archivo de bitácora (<c>bitacora_AAAA-MM-DD.txt</c>).
    /// </summary>
    /// <param name="archivo">Ruta del archivo a analizar.</param>
    /// <param name="fecha">Fecha obtenida del nombre.</param>
    /// <returns><c>true</c> si el nombre respeta el formato esperado.</returns>
    private static bool TryObtenerFechaDelArchivo(string archivo, out DateTime fecha)
    {
        fecha = default;

        string nombre = Path.GetFileNameWithoutExtension(archivo);

        if (!nombre.StartsWith(PrefijoArchivo, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string parteFecha = nombre[PrefijoArchivo.Length..];

        return DateTime.TryParseExact(
            parteFecha,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out fecha);
    }
}
