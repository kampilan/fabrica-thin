using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fabrica.Persistence.Ef.Converters;

public class MaxDateTimeConvertor() : ValueConverter<DateTime, DateTime>( time => time != DateTime.MaxValue ? time : MaxDateTime, time => time != MaxDateTime ? time : DateTime.MaxValue )
{

    public static readonly DateTime MaxDateTime = new(2200, 1, 1, 0, 0, 0, 0);

}