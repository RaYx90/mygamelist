namespace GameList.Domain.Exceptions;

/// <summary>
/// Excepción base para errores de dominio. Todas las excepciones de negocio deben heredar de esta clase.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Inicializa la excepción con el mensaje de error proporcionado.
    /// </summary>
    /// <param name="message">Mensaje descriptivo del error de dominio.</param>
    protected DomainException(string message) : base(message) { }
}
