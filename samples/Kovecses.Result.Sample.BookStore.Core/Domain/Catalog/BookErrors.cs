namespace Kovecses.Result.Sample.BookStore.Core.Domain.Catalog;

public static class BookErrors
{
    public static class Codes
    {
        public const string NotFound = "Catalog.BookNotFound";
        public const string DuplicateIsbn = "Catalog.DuplicateIsbn";
        public const string Retired = "Catalog.BookRetired";
        public const string InsufficientStock = "Catalog.InsufficientStock";
        public const string HasOpenOrders = "Catalog.BookHasOpenOrders";
        public const string InvalidPrice = "Catalog.InvalidPrice";
        public const string InvalidStock = "Catalog.InvalidStock";
    }

    public static Error NotFound(string isbn)
        => Error.NotFound(message: $"Book '{isbn}' was not found.", code: Codes.NotFound);

    public static Error DuplicateIsbn(string isbn)
        => Error.Conflict(message: $"A book with ISBN '{isbn}' already exists.", code: Codes.DuplicateIsbn);

    public static Error Retired(string isbn)
        => Error.Conflict(message: $"Book '{isbn}' has been retired.", code: Codes.Retired);

    public static Error InsufficientStock(string isbn, int requested, int available)
        => Error.Conflict(message: $"Only {available} copies of '{isbn}' are in stock, {requested} requested.", code: Codes.InsufficientStock)
            with
            {
                Metadata = new Dictionary<string, object>
                {
                    ["isbn"] = isbn,
                    ["requested"] = requested,
                    ["available"] = available
                }
            };

    public static Error HasOpenOrders(string isbn)
        => Error.Failure(message: $"Book '{isbn}' cannot be retired while it has open orders.", code: Codes.HasOpenOrders);

    public static Error InvalidPrice()
        => Error.Failure(message: "The price must be greater than zero.", code: Codes.InvalidPrice);

    public static Error InvalidStock()
        => Error.Failure(message: "The stock cannot be negative.", code: Codes.InvalidStock);
}
