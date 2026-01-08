using DataLabeling.Core.DTOs;
using DataLabeling.Core.Entities;
using DataLabeling.Core.Enums;
using DataLabeling.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLabeling.BLL.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AssignTasksAsync(AssignTaskDto dto)
        {
            var annotator = await _unitOfWork.Repository<User>().GetByIdAsync(dto.AnnotatorId);
            if (annotator == null) throw new Exception("Annotator không tồn tại");

            foreach (var taskId in dto.TaskIds)
            {
                var task = await _unitOfWork.Repository<LabelTask>().GetByIdAsync(taskId);
                if (task != null)
                {
                    task.AnnotatorId = dto.AnnotatorId;
                    task.Status = ProjectTaskStatus.New;
                    task.LabelData = null;      
                    task.ReviewerComment = null; 

                    _unitOfWork.Repository<LabelTask>().Update(task);
                }
            }
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<TaskViewDto>> GetTasksByAnnotatorAsync(int annotatorId)
        {
            var tasks = await _unitOfWork.Repository<LabelTask>()
                .FindAsync(t => t.AnnotatorId == annotatorId);

            var result = new List<TaskViewDto>();

            foreach (var t in tasks)
            {
                var dataItem = await _unitOfWork.Repository<DataItem>().GetByIdAsync(t.DataItemId);
                if (dataItem != null)
                {
                    var project = await _unitOfWork.Repository<Project>().GetByIdAsync(dataItem.ProjectId);

                    result.Add(new TaskViewDto
                    {
                        Id = t.Id,
                        DataUrl = dataItem.DataUrl,
                        Status = t.Status.ToString(),
                        LabelData = t.LabelData,
                        ProjectName = project?.Name ?? $"Project {dataItem.ProjectId}",
                        Instruction = project?.Instruction ?? "Không có hướng dẫn",
                        LabelConfig = project?.LabelConfig ?? "",
                        ReviewerComment = t.ReviewerComment,
                        ErrorType = t.ErrorType.HasValue ? t.ErrorType.Value.ToString() : ""
                    });
                }
            }
            return result;
        }
        public async Task SubmitLabelAsync(SubmitLabelDto dto)
        {
            var task = await _unitOfWork.Repository<LabelTask>().GetByIdAsync(dto.TaskId);
            if (task == null) throw new Exception("Nhiệm vụ không tồn tại");

            task.LabelData = dto.LabelData;
            task.Status = ProjectTaskStatus.Submitted;
            task.LastUpdated = DateTime.Now;
            _unitOfWork.Repository<LabelTask>().Update(task);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<TaskViewDto>> GetSubmittedTasksAsync()
        {
            var tasks = await _unitOfWork.Repository<LabelTask>()
                .FindAsync(t => t.Status == ProjectTaskStatus.Submitted);

            var result = new List<TaskViewDto>();
            foreach (var t in tasks)
            {
                var dataItem = await _unitOfWork.Repository<DataItem>().GetByIdAsync(t.DataItemId);

                result.Add(new TaskViewDto
                {
                    Id = t.Id,
                    DataUrl = dataItem?.DataUrl ?? "N/A",
                    ProjectName = "Project " + dataItem?.ProjectId,
                    Status = t.Status.ToString(),
                    LabelData = t.LabelData 
                });
            }
            return result;
        }

        public async Task ReviewTaskAsync(ReviewTaskDto dto)
        {
            var task = await _unitOfWork.Repository<LabelTask>().GetByIdAsync(dto.TaskId);
            if (task == null) throw new Exception("Task không tồn tại");

            if (dto.IsApproved)
            {
                task.Status = ProjectTaskStatus.Approved;
                task.ReviewerComment = "Đã duyệt";
                task.ErrorType = ErrorType.None;
            }
            else
            {
                task.Status = ProjectTaskStatus.Rejected;
                task.ReviewerComment = dto.Comment;
                task.ErrorType = dto.ErrorType ?? ErrorType.Other;
            }

            task.LastUpdated = DateTime.Now;
            _unitOfWork.Repository<LabelTask>().Update(task);
            await _unitOfWork.CompleteAsync();
        }
    }
}