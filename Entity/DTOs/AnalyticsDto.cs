namespace Entity.DTOs
{
    /// <summary>
    /// YasirSharp AI - Analytics DTO
    /// </summary>
    public class AnalyticsDto
    {
        public int TotalInteractions { get; set; }
        public int TotalUsers { get; set; }
        public PopularQuestionDto[] PopularQuestions { get; set; }
        public PageUsageDto[] PopularPages { get; set; }
        public FeatureUsageDto[] PopularFeatures { get; set; }
    }

    public class PopularQuestionDto
    {
        public string Question { get; set; }
        public int Count { get; set; }
    }

    public class PageUsageDto
    {
        public string PageName { get; set; }
        public int Count { get; set; }
    }

    public class FeatureUsageDto
    {
        public string FeatureName { get; set; }
        public int Count { get; set; }
    }
}
