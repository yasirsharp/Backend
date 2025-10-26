namespace Entity.DTOs
{
    /// <summary>
    /// YasirSharp AI - Soru sorma DTO
    /// </summary>
    public class AskQuestionDto
    {
        public int UserId { get; set; }
        public string Question { get; set; }
        public string PageContext { get; set; }
        public string UserRole { get; set; }
        public string Language { get; set; }
    }
}
