using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabrica.Persistence.Ef.Contexts
{


    public interface IModeler<TEntity> where TEntity : class, IEntity
    {

        void Configure( EntityTypeBuilder<TEntity> builder );

    }


}
