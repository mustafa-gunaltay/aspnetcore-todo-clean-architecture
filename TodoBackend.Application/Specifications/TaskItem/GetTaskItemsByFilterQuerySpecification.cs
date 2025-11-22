using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByFilter;
using TodoBackend.Domain.SpecificationConfig;

namespace TodoBackend.Application.Specifications.TaskItem;

public class GetTaskItemsByFilterQuerySpecification : Specification<Domain.Models.TaskItem>
{
    public GetTaskItemsByFilterQuerySpecification(
        GetTaskItemsByFilterQuery query,
        ILogger<GetTaskItemsByFilterQuerySpecification> logger)
    {
        var filterDetails = new List<string>();

        // Zorunlu filtre: UserId
        AddCriteria(t => t.UserId == query.UserId);
        logger.LogDebug("Applied mandatory filter: UserId={UserId}", query.UserId);

        // Opsiyonel filtre: IsCompleted
        if (query.IsCompleted.HasValue)
        {
            AddCriteria(t => t.IsCompleted == query.IsCompleted.Value);
            filterDetails.Add($"IsCompleted={query.IsCompleted.Value}");
            logger.LogDebug("Applied optional filter: IsCompleted={IsCompleted}", query.IsCompleted.Value);
        }

        // Opsiyonel filtre: Priority
        if (query.Priority.HasValue)
        {
            AddCriteria(t => t.Priority == query.Priority.Value);
            filterDetails.Add($"Priority={query.Priority.Value}");
            logger.LogDebug("Applied optional filter: Priority={Priority}", query.Priority.Value);
        }

        // Opsiyonel filtre: StartDate (DueDate >= StartDate)
        if (query.StartDueDate.HasValue)
        {
            AddCriteria(t => t.DueDate >= query.StartDueDate.Value);
            filterDetails.Add($"StartDueDate={query.StartDueDate.Value:yyyy-MM-dd}");
            logger.LogDebug("Applied optional filter: StartDueDate={StartDueDate:yyyy-MM-dd}", query.StartDueDate.Value);
        }

        // Opsiyonel filtre: EndDate (DueDate <= EndDate)
        if (query.EndDueDate.HasValue)
        {
            AddCriteria(t => t.DueDate <= query.EndDueDate.Value);
            filterDetails.Add($"EndDueDate={query.EndDueDate.Value:yyyy-MM-dd}");
            logger.LogDebug("Applied optional filter: EndDueDate={EndDueDate:yyyy-MM-dd}", query.EndDueDate.Value);
        }

        // Sıralama (OrderBy)
        if (query.OrderBy.HasValue)
        {
            switch (query.OrderBy.Value)
            {
                case OrderTaskItemByFilter.Title:
                    AddOrderBy(t => t.Title, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=Title, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.Description:
                    AddOrderBy(t => t.Description ?? string.Empty, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=Description, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.Priority:
                    AddOrderBy(t => t.Priority, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=Priority, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.DueDate:
                    AddOrderBy(t => t.DueDate ?? DateTime.MaxValue, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=DueDate, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.CompletedAt:
                    AddOrderBy(t => t.CompletedAt ?? DateTime.MaxValue, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=CompletedAt, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.CreatedAt:
                    AddOrderBy(t => t.CreatedAt, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=CreatedAt, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.UpdatedAt:
                    AddOrderBy(t => t.UpdatedAt ?? DateTime.MaxValue, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=UpdatedAt, OrderType={OrderType}", query.OrderType);
                    break;

                case OrderTaskItemByFilter.DeletedAt:
                    AddOrderBy(t => t.DeletedAt ?? DateTime.MaxValue, query.OrderType);
                    logger.LogDebug("Applied ordering: OrderBy=DeletedAt, OrderType={OrderType}", query.OrderType);
                    break;

                default:
                    // Default sıralama: CreatedAt Descending
                    AddOrderBy(t => t.CreatedAt, Domain.Enums.BuildingBlocks.OrderType.Descending);
                    logger.LogDebug("Applied default ordering: OrderBy=CreatedAt, OrderType=Descending");
                    break;
            }
        }
        else
        {
            // OrderBy belirtilmemişse default sıralama: CreatedAt Descending
            AddOrderBy(t => t.CreatedAt, Domain.Enums.BuildingBlocks.OrderType.Descending);
            logger.LogDebug("Applied default ordering: OrderBy=CreatedAt, OrderType=Descending");
        }

        // Pagination
        AddPaging(query.PageSize, query.PageNumber);
        logger.LogDebug("Applied pagination: PageSize={PageSize}, PageNumber={PageNumber}, Skip={Skip}, Take={Take}",
            query.PageSize, query.PageNumber, Skip, Take);

        var filterDescription = filterDetails.Count > 0 ? string.Join(", ", filterDetails) : "No additional filters";
        logger.LogInformation("Specification created for UserId={UserId} with filters: [{FilterDescription}], OrderBy={OrderBy}, OrderType={OrderType}",
            query.UserId, filterDescription, query.OrderBy?.ToString() ?? "CreatedAt (default)", query.OrderBy.HasValue ? query.OrderType.ToString() : "Descending (default)");
    }
}
