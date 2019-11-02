using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies
{
    public class Person : IEmbeddedDocument
    {
        public Person()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Name { get; set; }
        public int TmdbId { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
    }
}
