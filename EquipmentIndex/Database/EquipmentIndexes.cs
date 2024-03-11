using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentIndex.Database
{
    public class EquipmentIndexes
    {
                [Key]
                public int? EquipmentId { get; set; }
                public int? ParentId { get; set; }
                public string? Name { get; set; }
                public string? PersianName { get; set; }
                public string? UMDNS { get; set; }
                public string? DomesticStatus { get; set; }
                public DateTime? CreationDate { get; set; }
                public bool? Deleted { get; set; }
                public DateTime? LastUpdated { get; set; }
                public string? RequireBatchRelease { get; set; }
                public string? RiskClass { get; set; }
                public string? Description { get; set; }
                public string? PersianDescription { get; set; }
                public int? EquipmentType { get; set; }
                public string? ProductionSupport { get; set; }
                public string? Rating { get; set; }
                public string? Strategic { get; set; }
                public string? Pricing { get; set; }
        }
}
