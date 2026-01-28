using Core.Exceptions;
using Core.SystemLayer.DTOs;
using System.Collections.Generic;

namespace CAFMSystem.DTO.CreateBreakdownTaskDTO
{
    /// <summary>
    /// Request DTO for CreateBreakdownTask API.
    /// Creates a new breakdown task/work order in CAFM system.
    /// </summary>
    public class CreateBreakdownTaskReqDTO : IRequestSysDTO
    {
        // Reporter Information
        public string ReporterName { get; set; } = string.Empty;
        public string ReporterEmail { get; set; } = string.Empty;
        public string ReporterPhoneNumber { get; set; } = string.Empty;

        // Work Order Details
        public string Description { get; set; } = string.Empty;
        public string ServiceRequestNumber { get; set; } = string.Empty;

        // Location Information
        public string PropertyName { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;

        // Category Information
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;

        // Assignment
        public string Technician { get; set; } = string.Empty;

        // Source Information
        public string SourceOrgId { get; set; } = string.Empty;

        // Ticket Details
        public string Status { get; set; } = string.Empty;
        public string SubStatus { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ScheduledDate { get; set; } = string.Empty;
        public string ScheduledTimeStart { get; set; } = string.Empty;
        public string ScheduledTimeEnd { get; set; } = string.Empty;
        public string Recurrence { get; set; } = string.Empty;
        public string OldCAFMSRnumber { get; set; } = string.Empty;
        public string RaisedDateUtc { get; set; } = string.Empty;

        public void ValidateAPIRequestParameters()
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ServiceRequestNumber))
                errors.Add("ServiceRequestNumber is required.");

            if (string.IsNullOrWhiteSpace(Description))
                errors.Add("Description is required.");

            if (string.IsNullOrWhiteSpace(RaisedDateUtc))
                errors.Add("RaisedDateUtc is required.");

            if (string.IsNullOrWhiteSpace(SourceOrgId))
                errors.Add("SourceOrgId is required.");

            if (errors.Count > 0)
                throw new RequestValidationFailureException(
                    errorDetails: errors,
                    stepName: "CreateBreakdownTaskReqDTO.cs / Executing ValidateAPIRequestParameters"
                );
        }
    }
}
