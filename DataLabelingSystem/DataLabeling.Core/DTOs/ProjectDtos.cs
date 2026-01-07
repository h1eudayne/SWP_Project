using System.Collections.Generic;

namespace DataLabeling.Core.DTOs
{

    public class CreateProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public string LabelConfig { get; set; } = string.Empty;
        public int ManagerId { get; set; } 
    }

    public class AddDataItemDto
    {
        public int ProjectId { get; set; }
        public List<string> ImageUrls { get; set; } 
    }

    public class ProjectViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalImages { get; set; }
        public string ManagerName { get; set; } = string.Empty;
    }
}