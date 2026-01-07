using DataLabeling.Core.Enums;
using System.Collections.Generic;

namespace DataLabeling.Core.DTOs
{
    public class AssignTaskDto
    {
        public int AnnotatorId { get; set; }
        public List<int> TaskIds { get; set; }
    }
    public class SubmitLabelDto
    {
        public int TaskId { get; set; }
        public string LabelData { get; set; } = string.Empty;
    }
    public class TaskViewDto
    {
        public int Id { get; set; }
        public string DataUrl { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? LabelData { get; set; }
        public string? ReviewerComment { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string LabelConfig { get; set; } = string.Empty;
    }

    public class ReviewTaskDto
    {
        public int TaskId { get; set; }
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
        public ErrorType? ErrorType { get; set; }

    }
}