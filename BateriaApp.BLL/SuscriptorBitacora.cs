using BateriaApp.DAL;
using BateriaApp.Domain;

namespace BateriaApp.BLL;

// Suscriptor que guarda cada cambio en la bitacora.
// Usa el repositorio, no abre archivos por su cuenta.
public class SuscriptorBitacora : IObservadorBateria
{
    private readonly IRepositorioBitacora _repositorio;

    public SuscriptorBitacora(IRepositorioBitacora repositorio)
    {
        if (repositorio == null) throw new ArgumentNullException(nameof(repositorio));

        _repositorio = repositorio;
    }

    public string Nombre
    {
        get { return "Bitacora"; }
    }

    public int EventosRegistrados
    {
        get { return _repositorio.EventosRegistrados; }
    }

    // La bateria me llama a este metodo cada vez que cambia algo.
    public void Actualizar(EstadoBateria estado)
    {
        _repositorio.Registrar(estado);
    }
}
