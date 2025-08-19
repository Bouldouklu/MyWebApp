using MyWebApp.Models;

namespace MyWebApp.Services
{
    public class TodoService
    {
        private readonly List<TodoItem> _todos = new List<TodoItem>();
        private int _nextId = 1;

        public event Action? OnTodosChanged;

        public TodoService()
        {
            // Add some sample data for demo purposes
            AddTodo(new TodoItem 
            { 
                Title = "Welcome to your Todo List!", 
                Description = "This is a sample todo item. You can check it off, edit it, or delete it.",
                CreatedAt = DateTime.Now.AddDays(-1)
            });
            
            AddTodo(new TodoItem 
            { 
                Title = "Try adding a deadline", 
                Description = "Todos with deadlines will show priority colors",
                Deadline = DateTime.Now.AddDays(2),
                CreatedAt = DateTime.Now.AddHours(-2)
            });
        }

        public List<TodoItem> GetAllTodos()
        {
            return _todos.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public List<TodoItem> GetActiveTodos()
        {
            return _todos.Where(t => !t.IsCompleted)
                        .OrderBy(t => t.Deadline ?? DateTime.MaxValue)
                        .ThenByDescending(t => t.CreatedAt)
                        .ToList();
        }

        public List<TodoItem> GetCompletedTodos()
        {
            return _todos.Where(t => t.IsCompleted)
                        .OrderByDescending(t => t.CompletedAt)
                        .ToList();
        }

        public List<TodoItem> GetOverdueTodos()
        {
            return _todos.Where(t => t.IsOverdue).ToList();
        }

        public TodoItem? GetTodoById(int id)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }

        public void AddTodo(TodoItem todo)
        {
            todo.Id = _nextId++;
            todo.CreatedAt = DateTime.Now;
            _todos.Add(todo);
            OnTodosChanged?.Invoke();
        }

        public void UpdateTodo(TodoItem todo)
        {
            var existing = GetTodoById(todo.Id);
            if (existing != null)
            {
                existing.Title = todo.Title;
                existing.Description = todo.Description;
                existing.Deadline = todo.Deadline;
                OnTodosChanged?.Invoke();
            }
        }

        public void ToggleTodoComplete(int id)
        {
            var todo = GetTodoById(id);
            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
                todo.CompletedAt = todo.IsCompleted ? DateTime.Now : null;
                OnTodosChanged?.Invoke();
            }
        }

        public void DeleteTodo(int id)
        {
            var todo = GetTodoById(id);
            if (todo != null)
            {
                _todos.Remove(todo);
                OnTodosChanged?.Invoke();
            }
        }

        public TodoStats GetStats()
        {
            return new TodoStats
            {
                Total = _todos.Count,
                Active = _todos.Count(t => !t.IsCompleted),
                Completed = _todos.Count(t => t.IsCompleted),
                Overdue = _todos.Count(t => t.IsOverdue)
            };
        }
    }

    public class TodoStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
    }
}