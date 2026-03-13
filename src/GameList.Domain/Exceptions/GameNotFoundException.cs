namespace GameList.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando no se encuentra un juego con el identificador indicado.
/// </summary>
public sealed class GameNotFoundException : DomainException
{
    /// <summary>
    /// Inicializa la excepción con el identificador del juego no encontrado.
    /// </summary>
    /// <param name="id">Identificador del juego que no existe.</param>
    public GameNotFoundException(int id) : base($"Game with id '{id}' was not found.") { }
}
