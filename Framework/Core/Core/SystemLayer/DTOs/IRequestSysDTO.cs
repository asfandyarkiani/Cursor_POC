
namespace Core.SystemLayer.DTOs
{
    public interface IRequestSysDTO
    {
        //Throws Request Validation Failure exception
        void ValidateAPIRequestParameters();
    }
}
