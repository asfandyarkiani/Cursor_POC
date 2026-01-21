
namespace Core.SystemLayer.DTOs
{
    public interface IDownStreamRequestDTO
    {
        //Throws Request Validation Failure exception
        void ValidateDownStreamRequestParameters();
    }
}
