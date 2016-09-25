using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp.Model;

namespace Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected OrganizationServiceProxy _serviceProxy;
        protected ClientCredentials _clientCreds;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnectToCrm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginWindow loginWnd = new LoginWindow();
                bool? result = loginWnd.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    ConnectToCrm();
                    WhoAmIResponse whoAmI = _serviceProxy.Execute(new WhoAmIRequest()) as WhoAmIResponse;

                    txtStatus.Text = "Conneted to: " + _serviceProxy.ServiceManagement.CurrentServiceEndpoint.Address.Uri.AbsolutePath + " as " + GetUserLogin(whoAmI.UserId);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectToCrm()
        {
            _clientCreds = new ClientCredentials();
            _clientCreds.Windows.ClientCredential.UserName = Properties.Settings.Default["CrmUsername"].ToString();
            _clientCreds.Windows.ClientCredential.Password = Properties.Settings.Default["CrmPassword"].ToString();
            _clientCreds.Windows.ClientCredential.Domain = Properties.Settings.Default["CrmDomain"].ToString();

            string orgServiceUrl = Properties.Settings.Default["CrmUrl"].ToString().TrimEnd('/') + "/XRMServices/2011/Organization.svc";
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
        }

        private string GetUserLogin(Guid userId)
        {
            Entity user = _serviceProxy.Retrieve("systemuser", userId, new ColumnSet("domainname"));
            return "\\\\" + user.GetAttributeValue<string>("domainname");
        }

        private List<EntityMeta> GetAllEntities()
        {
            RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest()
            {
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            };

            // Retrieve the MetaData.
            RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)_serviceProxy.Execute(request);

            List<EntityMeta> entitiesList = new List<EntityMeta>();
            foreach (EntityMetadata currentEntity in response.EntityMetadata)
            {
                EntityMeta entityMeta = new EntityMeta();
                entityMeta.MetaId = currentEntity.MetadataId.HasValue ? currentEntity.MetadataId.Value : Guid.Empty;
                entityMeta.SchemaName = currentEntity.SchemaName;
                entityMeta.LogicalName = currentEntity.LogicalName;
                entityMeta.ObjectTypeCode = currentEntity.ObjectTypeCode.HasValue ? currentEntity.ObjectTypeCode.Value : 0;
                entityMeta.DisplayName = (currentEntity.DisplayName.UserLocalizedLabel != null) ? currentEntity.DisplayName.UserLocalizedLabel.Label : currentEntity.SchemaName;
                entityMeta.Description = (currentEntity.Description.UserLocalizedLabel != null) ? currentEntity.Description.UserLocalizedLabel.Label : "";
                entityMeta.IsCustomEntity = currentEntity.IsCustomEntity.Value;
                entityMeta.IsManaged = currentEntity.IsManaged.Value;
                entityMeta.IsValidForAdvancedFind = currentEntity.IsValidForAdvancedFind.Value;
                entityMeta.IsValidForQueue = currentEntity.IsValidForQueue.Value;
                entityMeta.IsVisibleInMobile = currentEntity.IsVisibleInMobile.Value;
                entitiesList.Add(entityMeta);
            }
            return entitiesList;
        }

        private void btnGetEntities_Click(object sender, RoutedEventArgs e)
        {
            List<EntityMeta> entities = GetAllEntities();
            grdEntities.ItemsSource = entities;
        }

        private void grdEntities_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName != "SchemaName" && e.PropertyName != "LogicalName" && e.PropertyName != "ObjectTypeCode" && e.PropertyName != "DisplayName" && e.PropertyName != "Description")
            {
                e.Cancel = true;
            }
        }
    }
}
