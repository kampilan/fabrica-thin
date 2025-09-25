// ReSharper disable UnusedMember.Global

using System.Linq.Expressions;
using Fabrica.Persistence;
using Fabrica.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabrica.App.Persistence.Contexts;

public static class EntityTypeBuilderExtensions
{

    public static EntityTypeBuilder<TEntity> HasRequiredReference<TEntity, TReference>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TReference?>> navigation) where TEntity : class, IEntity where TReference : class, IReferenceEntity
    {
        builder.HasOne(navigation).WithMany().OnDelete(DeleteBehavior.Restrict).IsRequired();
        return builder;
    }

    public static EntityTypeBuilder<TEntity> HasOptionalReference<TEntity, TReference>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TReference?>> navigation) where TEntity : class, IEntity where TReference : class, IReferenceEntity
    {
        builder.HasOne(navigation).WithMany().OnDelete(DeleteBehavior.ClientSetNull).IsRequired(false);
        return builder;
    }

    public static EntityTypeBuilder<TEntity> HasDependents<TEntity, TDependent>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, IEnumerable<TDependent>?>>? navigation, Expression<Func<TDependent, TEntity?>>? parent) where TEntity : class, IEntity where TDependent : class, IDependentEntity
    {
        builder.HasMany(navigation).WithOne(parent).OnDelete(DeleteBehavior.ClientCascade);
        return builder;
    }

    public static EntityTypeBuilder<TDependent> HasParent<TDependent, TEntity>(this EntityTypeBuilder<TDependent> builder, Expression<Func<TDependent, TEntity?>>? parent) where TEntity : class, IEntity where TDependent : class, IDependentEntity
    {
        builder.HasOne(parent);
        return builder;
    }

}

