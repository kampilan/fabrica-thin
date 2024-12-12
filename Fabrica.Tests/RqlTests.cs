using Fabrica.Rql.Parser;

namespace Fabrica.Tests;

public class RqlTests
{

    public void Test0400_0100_Should_Parse()
    {

        var tree = RqlLanguageParser.ToCriteria("(sw(Name,'Fabrica.Watch.Development'))");


    }
    
}