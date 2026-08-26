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
}
