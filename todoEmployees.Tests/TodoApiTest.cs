using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using todoEmployees.Tests.Helpers;

namespace todoEmployees.Tests
{
    class TodoApiTest
    {
        public readonly ILogger logger = TestFactory.CreateLogger();
    }
}
