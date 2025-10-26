using System;

namespace Entity.DTOs
{
    /// <summary>
    /// YasirSharp AI - Sayfa rehberi DTO
    /// </summary>
    public class PageGuideDto
    {
        public string PageName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Features { get; set; }
        public string[] Tips { get; set; }
    }
}
