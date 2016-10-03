using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Zhukoff.CRM.SvcUtilExtensions
{
    public class CrmService
    {
        protected OrganizationServiceProxy _serviceProxy;
        protected ClientCredentials _clientCreds;
        public bool IsConnected { get; set; }
        public string ConnectedTo { get; set; }
        public string ConnectedAs { get; set; }
        public CrmService(string domain, string username, string password, string crmurl)
        {
            _clientCreds = new ClientCredentials();
            _clientCreds.Windows.ClientCredential.UserName = username;
            _clientCreds.Windows.ClientCredential.Password = password;
            _clientCreds.Windows.ClientCredential.Domain = domain;

            string orgServiceUrl = crmurl.TrimEnd('/') + "/XRMServices/2011/Organization.svc";
            _serviceProxy = new OrganizationServiceProxy(new Uri(orgServiceUrl), null, _clientCreds, null);
            _serviceProxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());
            if (_serviceProxy.ServiceConfiguration.CurrentServiceEndpoint.Binding is CustomBinding)
            {
                CustomBinding cb = (CustomBinding)_serviceProxy.ServiceConfiguration.CurrentServiceEndpoint.Binding;
                cb.SendTimeout = new TimeSpan(0, 10, 0);
                cb.ReceiveTimeout = new TimeSpan(0, 10, 0);
                foreach (BindingElement be in cb.Elements)
                {
                    if (be is HttpTransportBindingElement)
                    {
                        ((HttpTransportBindingElement)be).UnsafeConnectionNtlmAuthentication = true;
                    }
                }
            }
            IsConnected = true;
            ConnectedTo = _serviceProxy.ServiceManagement.CurrentServiceEndpoint.Address.Uri.AbsoluteUri.Replace("XRMServices/2011/Organization.svc", "");
            WhoAmIResponse whoAmI = _serviceProxy.Execute(new WhoAmIRequest()) as WhoAmIResponse;
            ConnectedAs = GetUserLogin(whoAmI.UserId);
        }

        private string GetUserLogin(Guid userId)
        {
            Entity user = _serviceProxy.Retrieve("systemuser", userId, new ColumnSet("domainname"));
            return "\\\\" + user.GetAttributeValue<string>("domainname");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter">Entity = 1, Default = 1, Attributes = 2, Privileges = 4, Relationships = 8, All = 15</param>
        /// <returns></returns>
        public List<EntityMetadata> GetAllEntitiesMeta(int filter = 1)
        {
            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            // Retrieve the MetaData.
            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)_serviceProxy.Execute(request);
            return response.EntityMetadata.ToList();
        }
    }
}
