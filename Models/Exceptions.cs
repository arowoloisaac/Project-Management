namespace Task_Management_System.Models
{
    public class Exceptions
    {
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
