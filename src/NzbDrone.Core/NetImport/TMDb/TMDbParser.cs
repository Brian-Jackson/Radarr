using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
using NzbDrone.Core.NetImport.Exceptions;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbParser : IParseNetImportResponse
    {
        private readonly ISearchForNewMovie _skyhookProxy;
        private NetImportResponse _importResponse;

        public TMDbParser(ISearchForNewMovie skyhookProxy)
        {
            _skyhookProxy = skyhookProxy;
        }

        public virtual IList<Movies.Movie> ParseResponse(NetImportResponse importResponse)
        {
            _importResponse = importResponse;

            var movies = new List<Movies.Movie>();

            if (!PreProcess(_importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<MovieSearchRoot>(_importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.results.SelectList(_skyhookProxy.MapMovie);
        }

        protected virtual bool PreProcess(NetImportResponse listResponse)
        {
            if (listResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(listResponse,
                    "TMDb List API call resulted in an unexpected StatusCode [{0}]",
                    listResponse.HttpResponse.StatusCode);
            }

            if (listResponse.HttpResponse.Headers.ContentType != null &&
                listResponse.HttpResponse.Headers.ContentType.Contains("text/json") &&
                listResponse.HttpRequest.Headers.Accept != null &&
                !listResponse.HttpRequest.Headers.Accept.Contains("text/json"))
            {
                throw new NetImportException(listResponse,
                    "TMDb list responded with html content. Site is likely blocked or unavailable.");
            }

            return true;
        }
    }
}
