using System.ComponentModel.DataAnnotations;

namespace BuchVerwaltung.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Der Titel muss zwischen 2 und 100 Zeichen lang sein.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Der Autor ist erforderlich.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Der Autorname muss zwischen 2 und 80 Zeichen lang sein.")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "Die ISBN ist erforderlich.")]
        [RegularExpression(@"^[\d\-xX]{10,17}$", ErrorMessage = "Ungültige ISBN (10–13 Ziffern, ggf. mit Bindestrichen).")]
        public string Isbn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Das Jahr ist erforderlich.")]
        [Range(1000, 2025, ErrorMessage = "Das Erscheinungsjahr muss zwischen 1000 und 2025 liegen.")]
        public int Year { get; set; }
    }
}