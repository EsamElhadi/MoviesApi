using System.ComponentModel.DataAnnotations;

namespace MoviesApi.Dtos
{
    public class CreateGenresDto
    {
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
