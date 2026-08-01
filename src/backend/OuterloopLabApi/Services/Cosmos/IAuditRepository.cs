using OuterloopLabApi.Models;

namespace OuterloopLabApi.Services.Cosmos;

public interface IAuditRepository
{
    Task CreateAsync(AuditRecord record, CancellationToken cancellationToken);
    Task<AuditRecord?> GetByAuditIdAsync(string auditId, CancellationToken cancellationToken);
}
