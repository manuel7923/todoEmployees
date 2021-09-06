using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace todoEmployees.Tests.Helpers
{
    public class MockCloudTableTodos : CloudTable
    {
        public MockCloudTableTodos(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableTodos(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableTodos(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }
    }
}
