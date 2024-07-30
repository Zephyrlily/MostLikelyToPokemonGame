using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PokemonGame.Classes
{
    public static class StaticHTTPClient
    {
        public static readonly HttpClient httpClient = new HttpClient();
    }
}
