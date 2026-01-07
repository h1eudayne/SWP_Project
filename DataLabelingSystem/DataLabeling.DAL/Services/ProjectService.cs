using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataLabeling.Core.Entities;
using DataLabeling.Core.Interfaces;
using DataLabeling.Core.DTOs;
using DataLabeling.Core.Enums;

namespace DataLabeling.DAL.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectViewDto> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                Instruction = dto.Instruction,
                LabelConfig = dto.LabelConfig,
                ManagerId = dto.ManagerId,
                CreatedDate = DateTime.Now
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            await _context.Entry(project).Reference(p => p.Manager).LoadAsync();

            return new ProjectViewDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ManagerName = project.Manager?.FullName ?? "Unknown",
                TotalImages = 0
            };
        }

        public async Task<int> AddDataItemsAsync(AddDataItemDto dto)
        {
            var dataItems = dto.ImageUrls.Select(url => new DataItem
            {
                DataUrl = url,
                FileName = System.IO.Path.GetFileName(url), 
                ProjectId = dto.ProjectId
            }).ToList();

            _context.DataItems.AddRange(dataItems);
            await _context.SaveChangesAsync();

            return dataItems.Count;
        }

        public async Task<IEnumerable<ProjectViewDto>> GetProjectsByManagerAsync(int managerId)
        {
            return await _context.Projects
                .Where(p => p.ManagerId == managerId)
                .Select(p => new ProjectViewDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ManagerName = p.Manager.FullName,
                    TotalImages = p.DataItems.Count()
                })
                .ToListAsync();
        }


        public async Task<ProjectProgressDto> GetProjectProgressAsync(int projectId)
        {
            var dataItems = await _context.DataItems
                                          .Include(d => d.LabelTask)
                                          .Where(d => d.ProjectId == projectId)
                                          .ToListAsync();

            if (dataItems == null || !dataItems.Any())
            {
                return new ProjectProgressDto
                {
                    ProjectId = projectId,
                    TotalItems = 0,
                    PercentComplete = 0
                };
            }

            var stats = new ProjectProgressDto
            {
                ProjectId = projectId,
                TotalItems = dataItems.Count,
                NewItems = dataItems.Count(d => d.LabelTask == null || d.LabelTask.Status == ProjectTaskStatus.New),
                InProgressItems = dataItems.Count(d => d.LabelTask != null && d.LabelTask.Status == ProjectTaskStatus.InProgress),
                SubmittedItems = dataItems.Count(d => d.LabelTask != null && d.LabelTask.Status == ProjectTaskStatus.Submitted),
                RejectedItems = dataItems.Count(d => d.LabelTask != null && d.LabelTask.Status == ProjectTaskStatus.Rejected),
                ApprovedItems = dataItems.Count(d => d.LabelTask != null && d.LabelTask.Status == ProjectTaskStatus.Approved)
            };

            if (stats.TotalItems > 0)
            {
                stats.PercentComplete = Math.Round((double)stats.ApprovedItems / stats.TotalItems * 100, 2);
            }

            return stats;
        }

        public async Task<IEnumerable<ExportDataItemDto>> ExportApprovedDataAsync(int projectId)
        {
            var query = from d in _context.DataItems
                        join t in _context.LabelTasks on d.Id equals t.DataItemId
                        join u in _context.Users on t.AnnotatorId equals u.Id into users
                        from annotator in users.DefaultIfEmpty()
                        where d.ProjectId == projectId && t.Status == ProjectTaskStatus.Approved
                        select new ExportDataItemDto
                        {
                            Id = d.Id,
                            FileName = d.FileName,
                            Url = d.DataUrl,
                            LabelData = t.LabelData,
                            AnnotatorName = annotator != null ? annotator.FullName : "Unknown",
                            ReviewerComment = t.ReviewerComment
                        };

            return await query.ToListAsync();
        }
    }
}