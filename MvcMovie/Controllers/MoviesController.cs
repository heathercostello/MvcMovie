using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;
        private readonly IHostingEnvironment hostingEnvironment;

        public MoviesController(MvcMovieContext context,
                                IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.hostingEnvironment = hostingEnvironment;
        }

        public string ReleaseDateSort { get; set; }

            public async Task<IActionResult> Index(string sortOrder, 
            string currentGenreFilter, 
            string currentTitleFilter, string searchTitle, int? page)
        {
            sortOrder = String.IsNullOrEmpty(sortOrder) ? "id_desc" : sortOrder;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortParm"] = sortOrder == "Title" ? "title_desc" : "Title";
            ViewData["ReleaseDateSortParm"] = sortOrder == "ReleaseDate" ? "releasedate_desc" : "ReleaseDate";

            if (searchTitle != null)
            {
                page = 1;
            }
            else
            {
                searchTitle = currentTitleFilter;
            }

            ViewData["CurrentTitleFilter"] = searchTitle;

            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movie
                         select m;

            if (!string.IsNullOrEmpty(searchTitle))
            {
                movies = movies.Where(s => s.Title.Contains(searchTitle));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    movies = movies.OrderByDescending(s => s.Title);
                    break;
                case "Title":
                    movies = movies.OrderBy(s => s.Title);
                    break;
                case "releasedate_desc":
                    movies = movies.OrderByDescending(s => s.ReleaseDate);
                    break;
                case "ReleaseDate":
                    movies = movies.OrderBy(s => s.ReleaseDate);
                    break;
                case "price_desc":
                    movies = movies.OrderByDescending(s => s.Price);
                    break;
                case "Price":
                    movies = movies.OrderBy(s => s.Price);
                    break;
            }

           
            var movieGenreVM = new MovieGenreViewModel
            {
                Genres = new SelectList(await genreQuery.Distinct().ToListAsync()),
                Movies = await movies.ToListAsync()
            };

            return View(movieGenreVM);
        }


        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MovieCreateViewModel model,
            [Bind("ID,Title,ReleaseDate,Genre,Price, Rating")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Photo != null)
                {
                    string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                Movie newMovie = new Movie
                {
                    Title = model.Title,
                    ReleaseDate = model.ReleaseDate,
                    Price = model.Price,
                    Genre = model.Genre
                };
        
                _context.Add(newMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
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

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ReleaseDate,Genre,Price")] Movie movie)
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

        // GET: Movies/Delete/5
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

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
