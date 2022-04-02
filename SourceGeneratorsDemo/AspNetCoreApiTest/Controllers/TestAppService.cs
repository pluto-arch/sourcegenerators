using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AspNetCoreApiTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AspNetCoreApiTest.Controllers
{
    [Route("[controller]")]
    public class TestAppService
    {
        private readonly ILogger<TestAppService> _logger;
        private readonly IDemoService _demo;

        public TestAppService(ILogger<TestAppService> logger,IDemoService demo)
        {
            _logger = logger;
            _demo = demo.WithLogging(logger);
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("123123123");
            _demo.Demo(new A());
            return "123123";
        }


        public class Demo
        {
            public string Name { get; set; }
        }
    }
}