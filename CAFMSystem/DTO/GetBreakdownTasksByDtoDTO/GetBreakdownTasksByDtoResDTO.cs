using CAFMSystem.DTO.DownstreamDTOs;
using System.Collections.Generic;
using System.Linq;

namespace CAFMSystem.DTO.GetBreakdownTasksByDtoDTO
{
    /// <summary>
    /// Response DTO for GetBreakdownTasksByDto API.
    /// Returns list of existing breakdown tasks (if any).
    /// </summary>
    public class GetBreakdownTasksByDtoResDTO
    {
        public List<BreakdownTaskInfo> Tasks { get; set; } = new List<BreakdownTaskInfo>();
        public bool TaskExists { get; set; }
        public string ExistingTaskId { get; set; } = string.Empty;

        public static GetBreakdownTasksByDtoResDTO Map(GetBreakdownTasksByDtoApiResDTO apiResponse)
        {
            GetBreakdownTasksByDtoResDTO responseDto = new GetBreakdownTasksByDtoResDTO();

            if (apiResponse?.Tasks != null && apiResponse.Tasks.Count > 0)
            {
                responseDto.Tasks = apiResponse.Tasks.Select(task => new BreakdownTaskInfo
                {
                    TaskId = task.TaskId ?? string.Empty,
                    CallId = task.CallId ?? string.Empty,
                    Status = task.Status ?? string.Empty
                }).ToList();

                responseDto.TaskExists = true;
                responseDto.ExistingTaskId = apiResponse.Tasks.FirstOrDefault()?.TaskId ?? string.Empty;
            }
            else
            {
                responseDto.TaskExists = false;
                responseDto.ExistingTaskId = string.Empty;
            }

            return responseDto;
        }
    }

    public class BreakdownTaskInfo
    {
        public string TaskId { get; set; } = string.Empty;
        public string CallId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
