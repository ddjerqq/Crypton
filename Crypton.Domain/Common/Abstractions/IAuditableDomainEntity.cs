namespace Crypton.Domain.Common.Abstractions;

public interface IAuditableDomainEntity : IDomainEntity
{
    public DateTime Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}