using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Forms;

namespace Crypton.WebUIOld.Shared;

public sealed class BootstrapFormValidationClassProvider : FieldCssClassProvider
{
    public bool ValidationRequested { get; set; }

    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        if (!editContext.IsModified(fieldIdentifier) && !ValidationRequested)
            return string.Empty;

        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
        if (editContext.IsModified(fieldIdentifier))
            return isValid ? "is-valid" : "is-invalid";

        return string.Empty;
    }

    public static EditContext CreateEditContextFor<T>([NotNull] T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var provider = new BootstrapFormValidationClassProvider();
        var editContext = new EditContext(entity);

        editContext.SetFieldCssClassProvider(provider);
        editContext.OnValidationRequested += (_, _) => provider.ValidationRequested = true;

        return editContext;
    }
}