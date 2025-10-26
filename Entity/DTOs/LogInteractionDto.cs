namespace Entity.DTOs
{
    /// <summary>
    /// YasirSharp AI - Etkileşim kaydetme DTO
    /// </summary>
    public class LogInteractionDto
    {
        public int UserId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string PageContext { get; set; }
        public string FeatureUsed { get; set; }
        public string Language { get; set; }
    }
}
