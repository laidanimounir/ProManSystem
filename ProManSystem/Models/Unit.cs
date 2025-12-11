namespace ProManSystem.Models
{
    public class Unit
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;   
        public bool EstPredefined { get; set; } = false;  
    }
}
