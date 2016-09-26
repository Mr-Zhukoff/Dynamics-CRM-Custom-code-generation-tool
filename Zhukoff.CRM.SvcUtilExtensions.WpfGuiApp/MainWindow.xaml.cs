using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        CrmService crmSvc = null;
        List<EntityMeta> entitiesMetaList = new List<EntityMeta>();

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
                    crmSvc = new CrmService(Properties.Settings.Default["CrmDomain"].ToString(), 
                        Properties.Settings.Default["CrmUsername"].ToString(), 
                        Properties.Settings.Default["CrmPassword"].ToString(), 
                        Properties.Settings.Default["CrmUrl"].ToString());

                    txtStatus.Text = "Conneted to: " + crmSvc.ConnectedTo + " as " + crmSvc.ConnectedAs;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void FillEntitiesMetaList()
        {
            if(crmSvc == null || !crmSvc.IsConnected)
            {
                MessageBox.Show("Not connected to CRM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var entities = crmSvc.GetAllEntitiesMeta();
            entitiesMetaList.Clear();

            foreach (EntityMetadata entity in entities)
            {
                EntityMeta entityMeta = new EntityMeta();
                entityMeta.MetaId = entity.MetadataId.HasValue ? entity.MetadataId.Value : Guid.Empty;
                entityMeta.SchemaName = entity.SchemaName;
                entityMeta.LogicalName = entity.LogicalName;
                entityMeta.DisplayName = (entity.DisplayName.UserLocalizedLabel != null) ? entity.DisplayName.UserLocalizedLabel.Label : entity.SchemaName;
                entityMeta.ObjectTypeCode = entity.ObjectTypeCode.HasValue ? entity.ObjectTypeCode.Value : 0;
                entityMeta.Description = (entity.Description.UserLocalizedLabel != null) ? entity.Description.UserLocalizedLabel.Label : "";
                entityMeta.IsCustomEntity = entity.IsCustomEntity.Value;
                entityMeta.IsManaged = entity.IsManaged.Value;
                entityMeta.IsValidForAdvancedFind = entity.IsValidForAdvancedFind.Value;
                entityMeta.IsValidForQueue = entity.IsValidForQueue.Value;
                entityMeta.IsVisibleInMobile = entity.IsVisibleInMobile.Value;
                entitiesMetaList.Add(entityMeta);
            }
        }

        private void btnGetEntities_Click(object sender, RoutedEventArgs e)
        {
            Window waitWindow = new Window { Height = 100, Width = 200, WindowStartupLocation = WindowStartupLocation.CenterScreen, WindowStyle = WindowStyle.None };
            waitWindow.Content = new TextBlock { Text = "Please Wait", FontSize = 30, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate
            {
                Dispatcher.BeginInvoke(new Action(delegate { waitWindow.ShowDialog(); }));

                DataLoader dataLoader = new DataLoader(); // I made this class up
                dataLoader.DataLoaded += delegate
                {
                    Dispatcher.BeginInvoke(new Action(delegate () { waitWindow.Close(); }));
                };

                dataLoader.LoadData();
            };

            worker.RunWorkerAsync();
            FillEntitiesMetaList();
            grdEntities.ItemsSource = entitiesMetaList;
        }

        private void grdEntities_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName != "IsSelected" && e.PropertyName != "LogicalName" && e.PropertyName != "ObjectTypeCode" && e.PropertyName != "DisplayName" && e.PropertyName != "Description")
            {
                e.Cancel = true;
            }
        }

        private void applyFilter(bool isCustom)
        {
            ListCollectionView collectionView = new ListCollectionView(entitiesMetaList);
            collectionView.Filter = (e) =>
            {
                EntityMeta em = e as EntityMeta;
                return (em.IsCustomEntity == isCustom);
            };
            grdEntities.ItemsSource = collectionView;
        }

        private void cboxOnlyCustom_Checked(object sender, RoutedEventArgs e)
        {
            applyFilter(true);
        }

        private void cboxOnlyCustom_Unchecked(object sender, RoutedEventArgs e)
        {
            applyFilter(false);
        }
    }
}
