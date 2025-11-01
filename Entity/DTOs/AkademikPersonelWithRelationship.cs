using Core.Entities;

namespace Entity.DTOs
{
    /// <summary>
    /// AkademikPersonel with BolumAkademikPersoneller relationship ID
    /// Used for displaying personel with ability to remove the relationship
    /// </summary>
    public class AkademikPersonelWithRelationship : IDto
    {
        public int Id { get; set; }
        public string Ad { get; set; }
        public string Unvan { get; set; }
        public int RelationshipId { get; set; } // BolumAkademikPersoneller.Id
    }
}
