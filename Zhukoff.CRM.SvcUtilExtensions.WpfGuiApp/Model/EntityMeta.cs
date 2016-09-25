using System;

namespace Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp.Model
{
    public class EntityMeta
    {
        public bool IsSelected { get; set; }
        public Guid MetaId { get; set; }
        public string SchemaName { get; set; }
        public string LogicalName { get; set; }
        public int ObjectTypeCode { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsCustomEntity { get; set; }
        public bool IsManaged { get; set; }
        public bool IsMappable { get; set; }
        public bool IsValidForAdvancedFind { get; set; }
        public bool IsValidForQueue { get; set; }
        public bool IsVisibleInMobile { get; set; }
    }
}
