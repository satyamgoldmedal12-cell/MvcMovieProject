
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

public class MovieController : Controller
{
    private readonly MvcMovieContext _context;

    public MovieController(MvcMovieContext context)
    {
        _context = context;
    }

    // GET: MOVIES
    //public async Task<IActionResult> Index()
    //{
    //    return View(await _context.MovieGenreViewModel.ToListAsync());
    //}

    // GET: MOVIES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // GET: MOVIES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MOVIES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Stored Procedure के माध्यम से insert करना
                int insertedId = await _context.InsertMovieSPAsync(
                    movie.Title,
                    movie.ReleaseDate,
                    movie.Genre,
                    movie.Price
                );

                if (insertedId > 0)
                {
                    // Success - TempData में message set करना
                    TempData["SuccessMessage"] = $"Movie '{movie.Title}' inserted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Error inserting movie via stored procedure.";
                    return View(movie);
                }
            }
            catch (Exception ex)
            {
                // अगर SP fail हो तो regular insert करना (fallback)
                Console.WriteLine($"SP Insert failed: {ex.Message}. Using regular insert.");
                try
                {
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Movie '{movie.Title}' inserted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception fallbackEx)
                {
                    TempData["ErrorMessage"] = $"Error inserting movie: {fallbackEx.Message}";
                    return View(movie);
                }
            }
        }
        return View(movie);
    }

    // GET: MOVIES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }
        return View(movie);
    }

    // POST: MOVIES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    // POST: Movies/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
    {
        if (id != movie.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(movie);
    }

    // GET: MOVIES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _context.Movie
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }

    // POST: MOVIES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var movie = await _context.Movie.FindAsync(id);
        if (movie != null)
        {
            _context.Movie.Remove(movie);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MovieExists(int? id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }

    //GET: Movies
    public async Task<IActionResult> Index(string movieGenre, string searchString)
    {
        if (_context.Movie == null)
        {
            return Problem("Entity set 'MvcMovieContext.Movie'  is null.");
        }
        try
        {
            // Stored Procedure से data fetch करना
            var movies = await _context.GetAllMoviesFromSPAsync(searchString, movieGenre);

            // Genres list बनाना (dropdown के लिए)
            IQueryable<string> genreQuery = from m in _context.Movie orderby m.Genre select m.Genre;

            IQueryable<string> TitleQuery = from m in _context.Movie orderby m.Title select m.Title;

            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = movies,
                //SearchString = searchString,

                Titles = new SelectList(await TitleQuery.Distinct().ToListAsync())
            };

            return View(movieGenreVM);
        }

        catch (Exception ex)
        {
            // अगर SP fail हो तो regular LINQ से data fetch करना
            Console.WriteLine($"SP में error: {ex.Message}. Regular LINQ से data fetch हो रहा है।");

            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            IQueryable<string> TitleQuery = from m in _context.Movie orderby m.Title select m.Title;

            var movies = from m in _context.Movie
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title!.ToUpper().Contains(searchString.ToUpper()));
            }

            if (!string.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var fallbackMovies = await movies.ToListAsync();
            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = fallbackMovies,
                Titles = new SelectList(await TitleQuery.Distinct().ToListAsync())
            };

            return View(movieGenreVM);
        }
    }

    //[HttpPost]
    //public string Index(string searchString, bool notUsed)
    //{
    //    return "From [HttpPost]Index: filter on " + searchString;
    //}
}
