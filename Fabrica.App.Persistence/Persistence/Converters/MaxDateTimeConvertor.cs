
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ClassNeverInstantiated.Global

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fabrica.App.Persistence.Converters;

public class MaxDateTimeConvertor() : ValueConverter<DateTime, DateTime>( time => time != DateTime.MaxValue ? time : MaxDateTime, time => time != MaxDateTime ? time : DateTime.MaxValue )
{

    public static readonly DateTime MaxDateTime = new (2200, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

}