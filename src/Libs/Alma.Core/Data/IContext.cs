using Alma.Core.Entities;

namespace Alma.Core.Data
{
    public interface IContext
    {
        #region Repositories

        IRepository<T> Repository<T>()
            where T : Entity;

        #endregion

        #region Transactions

        ValueTask BeginTransactionAsync();

        ValueTask CommitAsync();

        ValueTask RollbackAsync();

        #endregion
    }
}