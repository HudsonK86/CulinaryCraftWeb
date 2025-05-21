using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CulinaryCraftWeb.Models
{
    public class PostRecipeViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int Cuisine_Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Url(ErrorMessage = "Please enter a valid YouTube URL.")]
        public string Youtube_Link { get; set; }

        [Required(ErrorMessage = "Please upload an image.")]
        public IFormFile ImageFile { get; set; }
    }
}