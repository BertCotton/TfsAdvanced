namespace TfsAdvanced.Models
{
    public class Response<T>
    {
        public int count { get; set; }
        public T value { get; set; }
    }
}