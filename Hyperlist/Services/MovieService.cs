using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hyperlist.Models;
using Newtonsoft.Json;

namespace Hyperlist.Services
{
    public class MovieService
    {
        private readonly HttpClient _client;

        public MovieService()
        {
            _client = new HttpClient();
        }

        public async Task<List<ShowDto>> LoadPage(int pageNr)
        {
            var result = await _client.GetStringAsync($"https://api.tvmaze.com/shows?page={pageNr}").ConfigureAwait(false);
            var items = JsonConvert.DeserializeObject<List<ShowDto>>(result);
            return items;
        }
    }
}
