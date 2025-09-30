using BuchVerwaltung.Data;
using BuchVerwaltung.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList; // Wichtig
using X.PagedList.Extensions;
using X.PagedList.Mvc.Core; // Wichtig

namespace BuchVerwaltung.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;

                var books = from b in _context.Books select b;

                if (!string.IsNullOrEmpty(searchString))
                {
                    books = books.Where(b =>
                        b.Title.Contains(searchString) ||
                        b.Author.Contains(searchString));
                }

                books = books.OrderBy(b => b.Title);

                int pageSize = 20;
                int pageNumber = page ?? 1;

                // Synchron paginieren (falls async nicht verfügbar)
                var bookList = await books.ToListAsync();
                var pagedBooks = bookList.ToPagedList(pageNumber, pageSize);

                // AJAX-Request erkennen
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_BooksTable", pagedBooks);
                }

                return View(pagedBooks);
            }
            catch (Exception ex)
            {
                // Loggen Sie den Fehler
                Console.WriteLine($"Fehler in Index Action: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Für AJAX-Requests einen Fehler zurückgeben
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return StatusCode(500, $"Server Error: {ex.Message}");
                }

                throw; // Für normale Requests den Fehler weiterwerfen
            }
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/DetailsPartial/5
        [HttpGet]
        public async Task<IActionResult> DetailsPartial(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return PartialView("_DetailsPartial", book);
        }

        // GET: Books/EditPartial/5
        [HttpGet]
        public async Task<IActionResult> EditPartial(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return Content("<div class='alert alert-danger'>Buch nicht gefunden.</div>");
            }
            return PartialView("_EditForm", book);
        }

        // GET: Books/DeleteConfirmed/5
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Books/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Book book)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex))
                    {
                        return StatusCode(400, new
                        {
                            success = false,
                            summary = "Ein Buch mit dieser ISBN existiert bereits.",
                            fields = new Dictionary<string, string>
                            {
                                { "Isbn", "Diese ISBN ist bereits vergeben." }
                            }
                        });
                    }
                    return StatusCode(500, new { success = false, summary = "Datenbankfehler beim Speichern." });
                }
            }

            var errors = ExtractValidationErrors();
            return StatusCode(400, new
            {
                success = false,
                summary = "Bitte korrigieren Sie die folgenden Fehler:",
                fields = errors
            });
        }

        // POST: Books/EditAjax
        [HttpPost]
        public async Task<IActionResult> EditAjax([FromBody] Book book)
        {
            if (book.Id == 0)
            {
                return StatusCode(400, new
                {
                    success = false,
                    summary = "Buch-ID fehlt.",
                    fields = new Dictionary<string, string> { { "Id", "Die Buch-ID ist ungültig." } }
                });
            }

            if (!BookExists(book.Id))
            {
                return StatusCode(404, new { success = false, summary = "Buch nicht gefunden." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateException ex)
                {
                    if (IsUniqueConstraintViolation(ex))
                    {
                        return StatusCode(400, new
                        {
                            success = false,
                            summary = "Ein Buch mit dieser ISBN existiert bereits.",
                            fields = new Dictionary<string, string>
                            {
                                { "Isbn", "Diese ISBN ist bereits vergeben." }
                            }
                        });
                    }
                    return StatusCode(500, new { success = false, summary = "Datenbankfehler beim Aktualisieren." });
                }
            }

            var errors = ExtractValidationErrors();
            return StatusCode(400, new
            {
                success = false,
                summary = "Bitte korrigieren Sie die folgenden Fehler:",
                fields = errors
            });
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var inner = ex.InnerException?.Message ?? "";
            return inner.Contains("UNIQUE") || inner.Contains("duplicate") || inner.Contains("verletzt Unique");
        }

        private Dictionary<string, string> ExtractValidationErrors()
        {
            var errors = new Dictionary<string, string>();
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    errors[key] = state.Errors.First().ErrorMessage!;
                }
            }
            return errors;
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}