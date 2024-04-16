using System.Linq.Expressions;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Validators;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Rules;

public abstract class ValidationBuilder<TFact>: AbstractRuleBuilder, IBuilder
{

    public virtual IValidator<TFact,TType> For<TType>(Expression<Func<TFact,TType>> extractor)
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";

        var rule = new ValidationRule<TFact>(fullSetName, "Placeholder");

        // Apply default salience
        rule.WithSalience(DefaultSalience);

        // Apply default inception and expiration
        rule.WithInception(DefaultInception);
        rule.WithExpiration(DefaultExpiration);

        Sinks.Add(t => t.Add(typeof(TFact), rule));

        var validator = rule.Assert(extractor);

        return validator;


    }

    protected ValidationRule<TFact> Add()
    {

        var nameSpace = GetType().Namespace;
        var fullSetName = $"{nameSpace}.{SetName}";

        var rule = new ValidationRule<TFact>(fullSetName, "Placeholder");

        return rule;

    }


}