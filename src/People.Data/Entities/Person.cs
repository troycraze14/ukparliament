using System.ComponentModel.DataAnnotations;

namespace People.Data.Entities
{
    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
    }
}
