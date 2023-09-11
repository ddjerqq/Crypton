using ErrorOr;

namespace Crypton.Domain.Common.Errors;

public static partial class Errors
{
    public static class Inventory
    {
        public static Error InvalidItem = Error.Failure("economy.invalid_item", "the item is invalid");
        public static Error InvalidItemType = Error.Failure("economy.invalid_item_type", "the item type is invalid");
    }
}