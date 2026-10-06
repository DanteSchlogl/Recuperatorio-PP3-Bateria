namespace BateriaApp.Domain;

// Lo tiene que implementar cualquier clase que quiera enterarse de los cambios de la bateria.
public interface IObservadorBateria
{
    // Un nombre para reconocerlo en los mensajes y en la bitacora.
    string Nombre { get; }

    // La bateria llama a este metodo cada vez que cambia el estado.
    void Actualizar(EstadoBateria estado);
}
