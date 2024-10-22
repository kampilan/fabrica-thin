namespace Fabrica.Utilities.Types;

public class DateTimeRangeModel: IDateTimeRange
{

    private readonly DateTime _origin = new (1970,1,1,0,0,0,0, DateTimeKind.Utc);

    public int Id { get; set; }

    public string Label { get; set; } = "Today";

    public DateTimeRange RangeKind { get; set; } = DateTimeRange.Today;

    public DateTime Begin => DateTimeHelpers.CalculateRange(RangeKind).begin;
    public DateTime End => DateTimeHelpers.CalculateRange(RangeKind).end;


    int IDateTimeRange.Id => Id;
    string IDateTimeRange.Label => Label;
    DateTime IDateTimeRange.Begin => Begin;
    long IDateTimeRange.BeginTimestamp => (long)(Begin.ToUniversalTime() - _origin).TotalSeconds;

    DateTime IDateTimeRange.End => End;
    long IDateTimeRange.EndTimestamp => (long)(End.ToUniversalTime() - _origin).TotalSeconds;

}