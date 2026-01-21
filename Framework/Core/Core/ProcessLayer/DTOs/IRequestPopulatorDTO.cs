
namespace Core.ProcessLayer.DTOs
{
    public interface IRequestPopulatorDTO<D> where D : class
    {
        void Populate(D domain);
    }
}
