using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.DataFactory;
using Azure.ResourceManager.Resources;
using Microsoft.Graph.Models;

namespace OrchestrationPipelinesAlert.Microsoft.DataFactory
{
    internal class ServiceConnector
    {
        internal readonly DefaultAzureCredential AzureCredential;
        internal readonly ArmClient ArmClient;
        internal readonly Dictionary<string, DataFactoryResource> DataFactoryResources = new Dictionary<string, DataFactoryResource>();

        public ServiceConnector()
        {
            DefaultAzureCredentialOptions options;

#if DEBUG
            options = new DefaultAzureCredentialOptions()
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = false,
                ExcludeVisualStudioCredential = false,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeAzureCliCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeWorkloadIdentityCredential = true,
                ExcludeManagedIdentityCredential = true
            };
#else
            options = new DefaultAzureCredentialOptions()
            {
                // Only include Managed Identity Credentials when in production
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeAzureCliCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeWorkloadIdentityCredential = true,
                ExcludeManagedIdentityCredential = false,
            };
            
#endif

            AzureCredential = new DefaultAzureCredential(options);
            ArmClient = new ArmClient(AzureCredential);
        }

        internal DataFactoryResource Connect(string subscriptionId, string resourceGroupName, string dataFactoryResourceName)
        {   
            if (DataFactoryResources.TryGetValue($"{subscriptionId}|{resourceGroupName}|{dataFactoryResourceName}", out var factoryResource)) return factoryResource;

            var dataFactoryResource = ArmClient.GetDataFactoryResource(DataFactoryResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, dataFactoryResourceName));
            DataFactoryResources.Add($"{subscriptionId}|{resourceGroupName}|{dataFactoryResourceName}", dataFactoryResource);
            return dataFactoryResource;
        }
    }
}
