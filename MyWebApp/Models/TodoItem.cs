using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Todo title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? Deadline { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public bool IsOverdue => Deadline.HasValue && 
                                Deadline.Value < DateTime.Now && 
                                !IsCompleted;
        
        public string GetPriorityClass()
        {
            if (IsCompleted) return "text-muted";
            if (IsOverdue) return "text-danger";
            if (Deadline.HasValue && Deadline.Value <= DateTime.Now.AddDays(1)) 
                return "text-warning";
            return "text-dark";
        }
    }
}