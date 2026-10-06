namespace BateriaApp.Domain;

/// <summary>
/// Contrato del patrón Observer. Todo objeto interesado en enterarse de los cambios
/// de estado de la <see cref="Bateria"/> debe implementar esta interfaz y registrarse
/// mediante <see cref="Bateria.Suscribir"/>.
/// Gracias a esta abstracción la batería ignora por completo quién la observa y
/// cuántos observadores hay: se cumple la relación 1 a muchos que pide el enunciado.
/// </summary>
public interface IObservadorBateria
{
    /// <summary>Nombre simbólico del observador; se usa en los mensajes y en la bitácora.</summary>
    string Nombre { get; }

    /// <summary>
    /// Invocado automáticamente por la batería cada vez que se produce un cambio de
    /// estado. El observador sólo debe leer la información recibida; no debe modificar
    /// al sujeto, para no disparar notificaciones recursivas.
    /// </summary>
    /// <param name="estado">Estado de la batería en el instante del cambio.</param>
    void Actualizar(EstadoBateria estado);
}
