using Microsoft.Extensions.Logging;
using todoEmployees.Tests.Helpers;

namespace todoEmployees.Tests
{
    internal class TodoApiTest
    {
        public readonly ILogger logger = TestFactory.CreateLogger();
    }
}
