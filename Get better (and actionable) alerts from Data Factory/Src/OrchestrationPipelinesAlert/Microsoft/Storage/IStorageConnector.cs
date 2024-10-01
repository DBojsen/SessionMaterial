using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrchestrationPipelinesAlert.Microsoft.Storage
{
    public interface IStorageConnector
    {
        public TableClient TableClient { get; }
    }
}
