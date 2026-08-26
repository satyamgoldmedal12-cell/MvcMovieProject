using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

public class MvcMovieContext : DbContext
{
    public MvcMovieContext(DbContextOptions<MvcMovieContext> options) : base(options)
    {

    }

    public DbSet<MvcMovie.Models.Movie> Movie { get; set; } = default!;

    /// <summary>
    /// Stored Procedure से सभी Movies fetch करता है parameters के साथ
    /// </summary>
    public async Task<List<MvcMovie.Models.Movie>> GetAllMoviesFromSPAsync(string? searchString = null, string? movieGenre = null)
    {
        try
        {
            // Parameters बनाना
            var searchParam = new SqlParameter("@SearchString", (object?)searchString ?? DBNull.Value);
            var genreParam = new SqlParameter("@MovieGenre", (object?)movieGenre ?? DBNull.Value);

            // Stored Procedure को parameters के साथ execute करना
            var movies = await Movie.FromSqlInterpolated(
                $"EXECUTE sp_GetAllMovies @SearchString={searchParam}, @MovieGenre={genreParam}"
            ).ToListAsync();

            return movies;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SP Execution Error: {ex.Message}");
            return new List<MvcMovie.Models.Movie>();
        }
    }

    /// <summary>
    /// Stored Procedure के माध्यम से नई Movie को database में insert करता है
    /// </summary>
    public async Task<int> InsertMovieSPAsync(string? title, DateTime releaseDate, string? genre, decimal price)
    {
        try
        {
            // Parameters बनाना
            var titleParam = new SqlParameter("@Title", (object?)title ?? DBNull.Value);
            var releaseDateParam = new SqlParameter("@ReleaseDate", releaseDate);
            var genreParam = new SqlParameter("@Genre", (object?)genre ?? DBNull.Value);
            var priceParam = new SqlParameter("@Price", price);

            // Return parameter बनाना (Id के लिए)
            var returnParam = new SqlParameter
            {
                ParameterName = "@InsertedId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            // Stored Procedure को execute करना
            await Database.ExecuteSqlInterpolatedAsync(
                $"EXECUTE sp_InsertMovie @Title={titleParam}, @ReleaseDate={releaseDateParam}, @Genre={genreParam}, @Price={priceParam}, @InsertedId={returnParam} OUTPUT"
            );

            // Inserted Id को return करना
            int insertedId = (int?)returnParam.Value ?? 0;
            return insertedId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Insert SP Execution Error: {ex.Message}");
            return 0;
        }
    }
}
