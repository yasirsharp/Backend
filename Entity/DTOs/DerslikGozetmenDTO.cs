using Core.Entities;

namespace Entity.DTOs
{
    public class DerslikGozetmenDTO:IDto
    {
        public int DerslikId { get; set; }
        public int? GozetmenId { get; set; }
    }
}
