namespace Crypton.Domain.Common.Abstractions;

public interface IAuditableEntity
{
    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }
}

public abstract class AuditableEntityBase
{
    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? Deleted { get; set; }

    public string? DeletedBy { get; set; }
}