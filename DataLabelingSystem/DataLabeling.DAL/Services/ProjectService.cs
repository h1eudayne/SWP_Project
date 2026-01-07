using DataLabeling.Core.DTOs;
using DataLabeling.Core.Entities;
using DataLabeling.Core.Enums;
using DataLabeling.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataLabeling.DAL.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.Repository<Project>().AddAsync(project);
            await _unitOfWork.CompleteAsync();

            return new ProjectViewDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                ManagerName = "Manager" 
            };
        }

        public async Task<int> AddDataItemsAsync(AddDataItemDto dto)
        {
            var project = await _unitOfWork.Repository<Project>().GetByIdAsync(dto.ProjectId);
            if (project == null) throw new Exception("Dự án không tồn tại");

            int count = 0;
            foreach (var url in dto.ImageUrls)
            {
                var item = new DataItem
                {
                    ProjectId = dto.ProjectId,
                    DataUrl = url,
                    FileName = System.IO.Path.GetFileName(url)
                };
                await _unitOfWork.Repository<DataItem>().AddAsync(item);

                var task = new LabelTask
                {
                    DataItem = item, 
                    Status = ProjectTaskStatus.New,
                    LastUpdated = DateTime.Now
                };
                await _unitOfWork.Repository<LabelTask>().AddAsync(task);
                count++;
            }

            await _unitOfWork.CompleteAsync();
            return count; 
        }

        public async Task<IEnumerable<ProjectViewDto>> GetProjectsByManagerAsync(int managerId)
        {
            var projects = await _unitOfWork.Repository<Project>()
                .FindAsync(p => p.ManagerId == managerId);

            return projects.Select(p => new ProjectViewDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ManagerName = p.ManagerId.ToString()
            });
        }
    }
}