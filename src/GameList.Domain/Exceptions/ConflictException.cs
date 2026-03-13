namespace GameList.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando se intenta crear un recurso que ya existe (conflicto de unicidad).
/// </summary>
public sealed class ConflictException : DomainException
{
    /// <summary>
    /// Inicializa la excepción con el mensaje descriptivo del conflicto.
    /// </summary>
    /// <param name="message">Descripción del conflicto producido.</param>
    public ConflictException(string message) : base(message) { }
}
