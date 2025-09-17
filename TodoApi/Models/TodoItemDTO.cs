using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models
{
    public class TodoItemDTO
    {
        //  [Range(1, long.MaxValue, ErrorMessage = "Id must be a positive number")]
        public long Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "IsCompleted must be true or false")]
        public bool? IsCompleted { get; set; }
    }
}