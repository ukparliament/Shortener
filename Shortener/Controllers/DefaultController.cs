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
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("s")]
    public class DefaultController : Controller
    {
        private readonly IStorageService storage;

        public DefaultController(IStorageService storage)
        {
            this.storage = storage;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View();
        }

        [HttpGet("{id}", Name = nameof(GetById))]
        public async Task<IActionResult> GetById(string id)
        {
            var url = await storage.GetValue(id);

            if (url is null)
            {
                return NotFound();
            }

            return Redirect(url);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(string url)
        {
            return await Add(url, BadRequestResult, CreatedResult);
        }

        [HttpPost(Name = "Post")]
        public async Task<IActionResult> Post(string url)
        {
            return await Add(url, BadRequestView, CreatedView);
        }

        private async Task<IActionResult> Add(string url, Func<IActionResult> failCallback, Func<string, IActionResult> successCallback)
        {
            if (!IsValid(url))
            {
                return failCallback();
            }

            var id = ComputeId(url);

            if (!await storage.ContainsKey(id))
            {
                await storage.Add(id, url);
            }

            return successCallback(id);
        }

        private IActionResult CreatedView(string id)
        {
            ViewBag.Id = id;
            return View("Created");
        }

        private IActionResult BadRequestView()
        {
            return View("WrongDomain");
        }

        private IActionResult CreatedResult(string id)
        {
            var routeValues = new { id };
            var action = nameof(GetById);

            return CreatedAtAction(action, routeValues, Url.Link(action, routeValues));
        }

        private IActionResult BadRequestResult()
        {
            return BadRequest("Only *.parliament.uk URLs please.");
        }

        private static bool IsValid(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }

            var host = new Uri(url).Host;

            return host == "parliament.uk" || host == "parliamentlive.tv" || host.EndsWith(".parliament.uk");
        }

        private static string ComputeId(string url)
        {
            using (var hash = MD5.Create())
            {
                var data = hash.ComputeHash(Encoding.UTF8.GetBytes(url));

                var builder = new StringBuilder();

                for (var i = 0; i < data.Length; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }

                return builder.ToString().Substring(24);
            }
        }
    }
}
