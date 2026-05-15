namespace Logistics.Services.Ordering.Api.Repositories
{
    /// <summary>
    /// 仓储层分页查询结果。
    /// </summary>
    /// <typeparam name="T">当前页中的领域对象类型。</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 当前页查出的领域对象集合。
        /// </summary>
        public IReadOnlyCollection<T> Items { get; init; } = [];

        /// <summary>
        /// 符合查询条件的总记录数。
        /// </summary>
        public int TotalCount { get; init; }
    }
}
