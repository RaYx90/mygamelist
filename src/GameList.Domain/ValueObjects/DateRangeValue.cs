namespace GameList.Domain.ValueObjects;

public sealed class DateRangeValue : IEquatable<DateRangeValue>
{
    public DateOnly Start { get; }
    public DateOnly End { get; }

    private DateRangeValue(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public static DateRangeValue Create(DateOnly start, DateOnly end)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date.");

        return new DateRangeValue(start, end);
    }

    public static DateRangeValue ForMonth(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRangeValue(start, end);
    }

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool Equals(DateRangeValue? other)
    {
        if (other is null) return false;
        return Start == other.Start && End == other.End;
    }

    public override bool Equals(object? obj) => Equals(obj as DateRangeValue);

    public override int GetHashCode() => HashCode.Combine(Start, End);

    public override string ToString() => $"{Start:yyyy-MM-dd} — {End:yyyy-MM-dd}";
}
