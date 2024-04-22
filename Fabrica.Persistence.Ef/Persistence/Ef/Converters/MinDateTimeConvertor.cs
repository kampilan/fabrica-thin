using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fabrica.Persistence.Ef.Converters;

public class MinDateTimeConvertor() : ValueConverter<DateTime, DateTime>(time => time != DateTime.MinValue ? time : MinDateTime, time => time != MinDateTime ? DateTime.SpecifyKind(time,DateTimeKind.Utc) : DateTime.SpecifyKind(DateTime.MinValue,DateTimeKind.Utc)  )
{

    public static readonly DateTime MinDateTime = new ( 1883, 11, 19, 0, 0, 0, 0, DateTimeKind.Utc );



}