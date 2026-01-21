namespace Core.ProcessLayer.Domains
{
    public interface IDomain<T>
    {
        public T Id { get; set; }
        public bool IsNew()
        {
            if (typeof(T) == typeof(string))
            {
                string value = Id as string;
                return string.IsNullOrEmpty(value);
            }
            else
            {
                return EqualityComparer<T>.Default.Equals(Id, default) ? true : false;
            }
        }
    }
}
