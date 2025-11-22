namespace TodoBackend.Domain.SpecificationConfig;

public static class SpecificationExtensions
{
    public static IQueryable<TEntity> Specify<TEntity>(this IQueryable<TEntity> query, Specification<TEntity> specification)
    {
        var queryable = query;

        // Apply criteria (WHERE clause)
        if (specification.Criteria is not null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        // Apply OrderBy Ascending with optional ThenOrderBy
        if (specification.OrderByAscendingExpression is not null)
        {
            // specification.OrderByAscendingExpression -> artan sekilde siralanacak ifade. Orn: x => x.Name
            // OrderBy() fonksiyonu ascending siralama yapar 
            var orderedQuery = queryable.OrderBy(specification.OrderByAscendingExpression);
            
            if (specification.ThenOrderByAscendingExpression is not null)
            {
                queryable = orderedQuery.ThenBy(specification.ThenOrderByAscendingExpression);
            }
            else if (specification.ThenOrderByDescendingExpression is not null)
            {
                queryable = orderedQuery.ThenByDescending(specification.ThenOrderByDescendingExpression);
            }
            else
            {
                queryable = orderedQuery;
            }
        }
        // Apply OrderBy Descending with optional ThenOrderBy
        else if (specification.OrderByDescendingExpression is not null)
        {
            // specification.OrderByDescendingExpression -> azalan sekilde siralanacak ifade. Orn: x => x.Name
            // OrderByDescending() fonksiyonu descending siralama yapar
            var orderedQuery = queryable.OrderByDescending(specification.OrderByDescendingExpression);
            
            if (specification.ThenOrderByAscendingExpression is not null)
            {
                queryable = orderedQuery.ThenBy(specification.ThenOrderByAscendingExpression);
            }
            else if (specification.ThenOrderByDescendingExpression is not null)
            {
                queryable = orderedQuery.ThenByDescending(specification.ThenOrderByDescendingExpression);
            }
            else
            {
                queryable = orderedQuery;
            }
        }

        return queryable;
    }
}
