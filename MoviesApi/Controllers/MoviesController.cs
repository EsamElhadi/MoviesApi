using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Infrastructure;
using MoviesApi.Dtos;
using MoviesApi.Models;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private new List<string> _allowedExtenstion = new List<string> {".jpg", ".png"};
        private long _maxAllowedPsterSize = 1048576;
        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                .OrderByDescending(m => m.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    Poster = m.Poster,
                    Storeline = m.Storeline,
                    Rate = m.Rate,
                    Year = m.Year,
                })
                .ToListAsync();

            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Genre)
                .SingleOrDefaultAsync(m => m.Id == id);
            if(movie == null)
                return NotFound();

                var dto = new MovieDetailsDto
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    GenreId = movie.GenreId,
                    GenreName = movie.Genre.Name,
                    Poster = movie.Poster,
                    Storeline = movie.Storeline,
                    Rate = movie.Rate,
                    Year = movie.Year,
                };

            return Ok(dto);
        }

        [HttpGet("{GetByGenreId}")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _context.Movies
                .Where(m => m.GenreId == genreId)
                .OrderByDescending(m => m.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    GenreId = m.GenreId,
                    GenreName = m.Genre.Name,
                    Poster = m.Poster,
                    Storeline = m.Storeline,
                    Rate = m.Rate,
                    Year = m.Year,
                })
                .ToListAsync();

            if(genreId == null)
                return NotFound();

            return Ok(movies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MoviesDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is Required");

            if (!_allowedExtenstion.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are Allowed");

            if (dto.Poster.Length > _maxAllowedPsterSize)
                return BadRequest("Max allowed size for Poster is 1MB");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre ID!");

            using var dataStream = new MemoryStream();

            await dto.Poster.CopyToAsync(dataStream);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Poster = dataStream.ToArray(),
                Rate = dto.Rate,
                Storeline = dto.Storeline,
                Year = dto.Year,
            };

            await _context.AddAsync(movie);

            _context.SaveChanges();

            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MoviesDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound($"No genre was found with ID: {id}");
            }

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == dto.GenreId);

            if (!isValidGenre)
                return BadRequest("Invalid Genre ID!");

            if (dto.Poster != null)
            {
                if (!_allowedExtenstion.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are Allowed");

                if (dto.Poster.Length > _maxAllowedPsterSize)
                    return BadRequest("Max allowed size for Poster is 1MB");

                var dataStream = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.Storeline = dto.Storeline;
            movie.Rate = dto.Rate;

            _context.SaveChanges();

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound($"No genre was found with ID: {id}");
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }
    }
}
