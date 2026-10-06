using System.Globalization;
using BateriaApp.DAL.Excepciones;
using BateriaApp.DAL.Formato;
using BateriaApp.Domain;

namespace BateriaApp.DAL;

// Guarda la bitacora en un archivo de texto por dia: bitacora_2026-10-06.txt
// Como es un archivo por fecha, la rotacion sale sola cuando cambia el dia.
public class RepositorioBitacoraArchivo : IRepositorioBitacora
{
    private readonly IFormateadorRegistro _formateador;

    // Para que dos escrituras al mismo tiempo no rompan el archivo.
    private readonly object _candado = new object();

    public RepositorioBitacoraArchivo(string carpeta, IFormateadorRegistro formateador)
    {
        if (string.IsNullOrWhiteSpace(carpeta))
        {
            Carpeta = "Bitacoras";
        }
        else
        {
            Carpeta = carpeta;
        }

        if (formateador == null)
        {
            _formateador = new FormateadorTextoPlano();
        }
        else
        {
            _formateador = formateador;
        }
    }

    public string Carpeta { get; private set; }

    public int EventosRegistrados { get; private set; }

    // Armo el nombre del archivo con la fecha.
    public string ObtenerRutaArchivo(DateTime fecha)
    {
        return Path.Combine(Carpeta, "bitacora_" + fecha.ToString("yyyy-MM-dd") + ".txt");
    }

    public void Registrar(EstadoBateria estado)
    {
        string ruta = ObtenerRutaArchivo(estado.FechaHora);
        string linea = _formateador.Formatear(estado) + Environment.NewLine;

        try
        {
            lock (_candado)
            {
                Directory.CreateDirectory(Carpeta);  // la creo si no existe
                File.AppendAllText(ruta, linea);     // agrego al final, no piso lo anterior
            }

            EventosRegistrados++;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new BitacoraException("No pude escribir la bitacora en " + Path.GetFullPath(ruta), ruta, ex);
        }
    }

    public IReadOnlyList<string> LeerBitacora(DateTime fecha)
    {
        string ruta = ObtenerRutaArchivo(fecha);

        try
        {
            if (!File.Exists(ruta))
            {
                return new List<string>();
            }

            return File.ReadAllLines(ruta);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new BitacoraException("No pude leer la bitacora de " + Path.GetFullPath(ruta), ruta, ex);
        }
    }

    // Rotacion: paso a la carpeta "historico" los archivos mas viejos que N dias.
    // No borro nada, solo los muevo.
    public IReadOnlyList<string> Rotar(int diasARetener)
    {
        List<string> movidos = new List<string>();

        if (!Directory.Exists(Carpeta))
        {
            return movidos;
        }

        DateTime limite = DateTime.Today.AddDays(-diasARetener);

        try
        {
            lock (_candado)
            {
                string[] archivos = Directory.GetFiles(Carpeta, "bitacora_*.txt");

                foreach (string archivo in archivos)
                {
                    if (!EsMasViejoQue(archivo, limite)) continue;

                    string carpetaHistorico = Path.Combine(Carpeta, "historico");
                    Directory.CreateDirectory(carpetaHistorico);

                    string destino = Path.Combine(carpetaHistorico, Path.GetFileName(archivo));

                    if (File.Exists(destino)) File.Delete(destino);

                    File.Move(archivo, destino);
                    movidos.Add(destino);
                }
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new BitacoraException("No pude rotar los archivos de " + Carpeta, Carpeta, ex);
        }

        return movidos;
    }

    // Saco la fecha del nombre del archivo para saber si paso el limite.
    private static bool EsMasViejoQue(string archivo, DateTime limite)
    {
        string nombre = Path.GetFileNameWithoutExtension(archivo);
        string textoFecha = nombre.Replace("bitacora_", "");

        DateTime fecha;

        bool laPude = DateTime.TryParseExact(textoFecha, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);

        return laPude && fecha < limite;
    }
}
