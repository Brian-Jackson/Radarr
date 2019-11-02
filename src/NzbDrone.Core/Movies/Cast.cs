namespace NzbDrone.Core.Movies
{
    public class Cast : Person
    {
        public string Character { get; set; }
        public int Order { get; set; }
    }
}
