namespace GameList.Domain.ValueObjects;

/// <summary>
/// Value Object que representa un rango de fechas con inicio y fin inclusivos.
/// </summary>
public sealed class DateRangeValue : IEquatable<DateRangeValue>
{
    /// <summary>Fecha de inicio del rango (inclusiva).</summary>
    public DateOnly Start { get; }

    /// <summary>Fecha de fin del rango (inclusiva).</summary>
    public DateOnly End { get; }

    private DateRangeValue(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Crea un rango de fechas validando que el fin no sea anterior al inicio.
    /// </summary>
    /// <param name="start">Fecha de inicio.</param>
    /// <param name="end">Fecha de fin.</param>
    /// <returns>Nueva instancia de <see cref="DateRangeValue"/>.</returns>
    public static DateRangeValue Create(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException("La fecha de fin debe ser mayor o igual a la fecha de inicio.");

        return new DateRangeValue(start, end);
    }

    /// <summary>
    /// Crea un rango de fechas que abarca el mes completo indicado.
    /// </summary>
    /// <param name="year">Año del mes.</param>
    /// <param name="month">Mes (1-12).</param>
    /// <returns>Rango desde el primer al último día del mes.</returns>
    public static DateRangeValue ForMonth(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRangeValue(start, end);
    }

    /// <summary>
    /// Indica si la fecha proporcionada está dentro del rango (inclusivo).
    /// </summary>
    /// <param name="date">Fecha a comprobar.</param>
    /// <returns><c>true</c> si la fecha está dentro del rango; <c>false</c> en caso contrario.</returns>
    public bool Contains(DateOnly date) => date >= Start && date <= End;

    /// <summary>
    /// Comprueba la igualdad por valor con otro <see cref="DateRangeValue"/>.
    /// </summary>
    /// <param name="other">Instancia a comparar.</param>
    /// <returns><c>true</c> si ambos rangos tienen el mismo inicio y fin.</returns>
    public bool Equals(DateRangeValue? other)
    {
        if (other is null) return false;
        return Start == other.Start && End == other.End;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DateRangeValue);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <inheritdoc/>
    public override string ToString() => $"{Start:yyyy-MM-dd} — {End:yyyy-MM-dd}";
}
