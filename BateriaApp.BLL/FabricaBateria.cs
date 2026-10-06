using BateriaApp.DAL;
using BateriaApp.DAL.Formato;

namespace BateriaApp.BLL;

// Los formatos de bitacora que puede elegir la pantalla.
public enum FormatoBitacora
{
    TextoPlano,
    Detallado
}

// Fabrica que arma todo junto: el repositorio, el formato y el servicio.
// Asi la pantalla pide "un servicio" y no necesita conocer la capa de datos.
public static class FabricaBateria
{
    public static ServicioBateria Crear(int cargaInicial, bool conectado, string carpetaBitacoras, FormatoBitacora formato)
    {
        IFormateadorRegistro formateador;

        if (formato == FormatoBitacora.Detallado)
        {
            formateador = new FormateadorDetallado();
        }
        else
        {
            formateador = new FormateadorTextoPlano();
        }

        RepositorioBitacoraArchivo repositorio = new RepositorioBitacoraArchivo(carpetaBitacoras, formateador);

        return new ServicioBateria(repositorio, cargaInicial, conectado);
    }
}
