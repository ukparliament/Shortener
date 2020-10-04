// MIT License
//
// Copyright (c) 2019 UK Parliament
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

namespace Shortener
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;

    [Route("s/stats")]
    public class StatsController : Controller
    {
        private readonly IAppInsightDataService dataService;

        public StatsController(IAppInsightDataService dataService)
        {
            this.dataService = dataService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string query = "requests | where url contains \"/s/\"  | where resultCode == 302 | summarize totalcount = sum(itemCount) by url | order by totalcount";
            var json = this.dataService.Query(query);
            var root = JObject.Parse(json);
            IEnumerable<UrlRequestCounts> results = root["tables"][0]["rows"].ToObject<IEnumerable<string[]>>()
                .Select(row => new UrlRequestCounts() { Url = row[0], Counts = int.Parse(row[1]) });
            return View(results);
        }
    }
}
