using DataLabeling.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLabeling.Core.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectViewDto> CreateProjectAsync(CreateProjectDto dto);
        Task<int> AddDataItemsAsync(AddDataItemDto dto);
        Task<IEnumerable<ProjectViewDto>> GetProjectsByManagerAsync(int managerId);
    }
}