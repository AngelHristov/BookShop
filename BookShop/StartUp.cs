namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;

    using System;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {

            using (var db = new BookShopContext())
            {
                Console.WriteLine(GetMostRecentBooks(db));
            }
        }

        // 1.	Age Restriction
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);

            var result = context.Books
                .Where(a => a.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(x => x)
                .ToList();

            return string.Join(Environment.NewLine, result);
        }

        // 2.	Golden Books
        public static string GetGoldenBooks(BookShopContext context)
        {
            var editionType = Enum.Parse<EditionType>("Gold", true);

            var result = context.Books
                .Where(e => e.EditionType == editionType && e.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, result);
        }

        // 3.	Books by Price
        public static string GetBooksByPrice(BookShopContext context)
        {
            var result = context.Books
                .Where(b => b.Price > 40)
                .OrderByDescending(b => b.Price)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .ToList();

            return string.Join(Environment.NewLine, result.Select(x => $"{x.Title} - ${x.Price:F2}"));
        }

        //4.	Not Released In
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {

            var result = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                 .ToList();

            return string.Join(Environment.NewLine, result);
        }

        //5.	Book Titles by Category
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var result = context.Books
                .Where(bc => bc.BookCategories.Any(c => categories.Contains(c.Category.Name.ToLower())))
                .Select(b => b.Title)
                .OrderBy(x => x)
                .ToList();

            return string.Join(Environment.NewLine, result);
        }

        //6.	Released Before Date
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime dateTime = DateTime.ParseExact(date, "dd-MM-yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);

            var books = context.Books
                .Where(b => b.ReleaseDate < dateTime)
                .OrderByDescending(b => b.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.Price,
                    b.EditionType
                })
                .ToList();

            return string.Join(Environment.NewLine, books.Select(x => $"{x.Title} - {x.EditionType} - ${x.Price:F2}"));
        }

        //7.	Author Search
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName
                })
                .OrderBy(a => a.FullName)
                .ToList();

            return string.Join(Environment.NewLine, authors.Select(x => x.FullName));
        }

        //8.	Book Search
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(b => b.Title.IndexOf(input, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(b => b.Title)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        //9.	Book Search by Author
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(b => b.Author.LastName.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    AuthorName = b.Author.FirstName + " " + b.Author.LastName
                })
                .ToList();

            return string.Join(Environment.NewLine, books.Select(x => $"{x.Title} ({x.AuthorName})"));
        }

        //10.	Count Books
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            return context.Books.Where(b => b.Title.Length > lengthCheck).Count();
        }

        //11.	Total Book Copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(a => new
                {
                    AuthorName = a.FirstName + " " + a.LastName,
                    Count = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(b => b.Count)
                .ToList();

            return string.Join(Environment.NewLine, authors.Select(x => $"{x.AuthorName} - {x.Count}"));
        }

        //12.	Profit by Category
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categories = context.Categories
                .Select(c => new
                {
                    c.Name,
                    Money = c.CategoryBooks.Sum(i => i.Book.Price * i.Book.Copies)
                })
                .OrderByDescending(b => b.Money)
                .ThenBy(c => c.Name)
                .ToList();

            return string.Join(Environment.NewLine, categories.Select(x => $"{x.Name} ${x.Money:F2}"));
        }

        //13.	Most Recent Books
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var result = context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    CategoryName = c.Name,
                    BooksNames = c.CategoryBooks
                    .Select(b => new
                    {
                        b.Book.Title,
                        b.Book.ReleaseDate,
                        b.Book.ReleaseDate.Value.Year
                    })
                    .OrderByDescending(b => b.ReleaseDate)
                    .Take(3)
                    .ToList()
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var category in result)
            {
                sb.AppendLine($"--{category.CategoryName}");

                foreach (var book in category.BooksNames)
                {
                    sb.AppendLine($"{book.Title} ({book.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //14.	Increase Prices
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books.Where(b => b.ReleaseDate.Value.Year < 2010);

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        //15.	Remove Books
        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books.Where(b => b.Copies < 4200);
            int count = books.Count();

            foreach (var book in books)
            {
                context.Books.Remove(book);
            }

            context.SaveChanges();

            return count;
        }
    }
}
