using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Properties
        public IDialogService dlgService;
        private INavigationService navService;
        private CrmService crmService;

        public ObservableCollection<EntityMetaObject> EntitiesMetaList { get; private set; }

        private ICollectionView _entitiesMetaListView;
        public ICollectionView EntitiesMetaListView
        {
            get { return _entitiesMetaListView; }
            set
            {
                Set(() => EntitiesMetaListView, ref this._entitiesMetaListView, value);
            }
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                Set(() => IsLoaded, ref this._isLoaded, value);
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                Set(() => IsLoading, ref this._isLoading, value);
            }
        }

        private string _connectedTo;
        public string ConnectedTo
        {
            get { return _connectedTo; }
            set
            {
                this.IsLoaded = !String.IsNullOrEmpty(value);
                Set(() => ConnectedTo, ref this._connectedTo, value);
            }
        }

        private bool _isCustom;
        public bool IsCustom
        {
            get { return _isCustom; }
            set
            {
                Set(() => IsCustom, ref this._isCustom, value);
            }
        }

        private bool _isValidForAdvancedFind;
        public bool IsValidForAdvancedFind
        {
            get { return _isValidForAdvancedFind; }
            set
            {
                Set(() => IsValidForAdvancedFind, ref this._isValidForAdvancedFind, value);
            }
        }

        private int _count;
        public int Count
        {
            get { return _count; }
            set
            {
                Set(() => Count, ref this._count, value);
            }
        }
        #endregion

        #region Commands
        public RelayCommand ConnectToCrmCommand
        {
            get;
            private set;
        }
        public RelayCommand RefreshEntitiesMetaListCommand
        {
            get;
            private set;
        }
        public RelayCommand FilterEntitiesMetaListCommand
        {
            get;
            private set;
        }

        public RelayCommand<string> ShowErrorCommand
        {
            get;
            private set;
        }
        public RelayCommand<string> NavigateToPageCommand
        {
            get;
            private set;
        }
        public RelayCommand<string> ShowMessageCommand
        {
            get;
            private set;
        }
        #endregion
        /// <summary>
        /// Empty constructor for Design Mode
        /// </summary>
        public MainViewModel()
        {
            Messenger.Default.Register<NotificationMessage<Exception>>(this, HandleExceptionMessage);
            Messenger.Default.Register<NotificationMessage>(this, HandleMessage);

            //this.dlgService = new DialogService();
            //this.navService = new NavigationService();

            this.IsLoaded = false;

            ShowErrorCommand = new RelayCommand<string>(ShowErrorMessage);
            ShowMessageCommand = new RelayCommand<string>(ShowMessage);
            NavigateToPageCommand = new RelayCommand<string>(page =>
            {
                if (!String.IsNullOrEmpty(page))
                {
                    navService.NavigateTo(page);
                }
            });
            RefreshEntitiesMetaListCommand = new RelayCommand(FillEntitiesMetaList, CanFillEntitiesMetaList);
            ConnectToCrmCommand = new RelayCommand(ConnectToCrm);
            FilterEntitiesMetaListCommand = new RelayCommand(FilterEntitiesMetaList);
        }

        private void ConnectToCrm()
        {
            if (!String.IsNullOrEmpty(Properties.Settings.Default.CrmUrl)
                && !String.IsNullOrEmpty(Properties.Settings.Default.CrmUsername)
                && !String.IsNullOrEmpty(Properties.Settings.Default.CrmDomain)
                && !String.IsNullOrEmpty(Properties.Settings.Default.CrmPassword))
            {
                this.crmService = new CrmService(Properties.Settings.Default.CrmDomain,
                    Properties.Settings.Default.CrmUsername,
                    Properties.Settings.Default.CrmPassword,
                    Properties.Settings.Default.CrmUrl);
                this.ConnectedTo = String.Format("Connected to {0} as {1}", crmService.ConnectedTo, crmService.ConnectedAs);
            }
            else
            {
                this.ConnectedTo = null;
                ShowErrorMessage("Connection failed!");
            }
        }

        private void FillEntitiesMetaList()
        {
            if (crmService == null || !crmService.IsConnected)
            {
                MessageBox.Show("Not connected to CRM.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var entities = crmService.GetAllEntitiesMeta();
            EntitiesMetaList = new ObservableCollection<EntityMetaObject>();

            foreach (EntityMetadata entity in entities)
            {
                EntityMetaObject entityMeta = new EntityMetaObject();
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
                EntitiesMetaList.Add(entityMeta);
            }
            this.Count = EntitiesMetaList.Count;
            EntitiesMetaListView = CollectionViewSource.GetDefaultView(EntitiesMetaList);
            EntitiesMetaListView.Refresh();
        }

        private bool CanFillEntitiesMetaList()
        {
            return this.IsLoaded;
        }

        private void FilterEntitiesMetaList()
        {
            if (!EntitiesMetaListView.CanFilter)
            {
                return;
            }
            ListCollectionView collectionView = new ListCollectionView(EntitiesMetaList);
            collectionView.Filter = (e) =>
            {
                EntityMetaObject em = e as EntityMetaObject;
                bool filterCustom = true;
                bool filterAdvancedFind = true;
                if (this.IsCustom)
                {
                    filterCustom = (em.IsCustomEntity == true);
                }
                if (this.IsValidForAdvancedFind)
                {
                    filterAdvancedFind = (em.IsValidForAdvancedFind == true);
                }
                return (filterCustom || filterAdvancedFind);
            };
            this.Count = collectionView.Count;
            EntitiesMetaListView = collectionView;
            EntitiesMetaListView.Refresh();
        }

        private void NavigateToPage(string page)
        {
            navService.NavigateTo(page);
        }

        private void ShowErrorMessage(string text)
        {
            MessageBox.Show(text, "Error");
            //dlgService.ShowMessage(text, resLdr.GetString("MessageErrorTitle"));
        }

        private void HandleExceptionMessage(NotificationMessage<Exception> message)
        {
            //dlgService.ShowError(message.Content, resLdr.GetString("MessageErrorTitle"), resLdr.GetString("ButtonOkText"), null);
        }

        private void HandleMessage(NotificationMessage message)
        {
            //dlgService.ShowMessage(message.Notification, resLdr.GetString("MessageWarningTitle"));
        }

        private void ShowMessage(string message)
        {
            // dlgService.ShowMessage(message, resLdr.GetString("MessageWarningTitle"));
        }

        private async void HandleException(Exception ex)
        {
            //string errorType = ex.GetType().Name;
            //if (errorType == "HttpRequestException" && ex.InnerException != null && ex.InnerException.Message.Contains("The server name or address could not be resolved"))
            //{
            //    dlgService.ShowMessage(resLdr.GetString("MessageErrorServiceUnavailableText"), resLdr.GetString("MessageErrorTitle"), resLdr.GetString("ButtonOkText"), Application.Current.Exit);
            //    return;
            //}

            //StringBuilder content = new StringBuilder();

            //content.AppendFormat("DeviceModel: {0}\n", EnvironmentInfo.DeviceModel);
            //content.AppendFormat("DeviceManufacturer: {0}\n", EnvironmentInfo.DeviceManufacturer);
            //content.AppendFormat("SystemFamily: {0}\n", EnvironmentInfo.SystemFamily);
            //content.AppendFormat("SystemArchitecture: {0}\n", EnvironmentInfo.SystemArchitecture);
            //content.AppendFormat("SystemVersion: {0}\n", EnvironmentInfo.SystemVersion);
            //content.AppendFormat("FirmwareVersion: {0}\n", EnvironmentInfo.FirmwareVersion);
            //content.AppendFormat("ApplicationVersion: {0}\n", EnvironmentInfo.ApplicationVersion);

            //content.AppendFormat("Exception: {0}\n", ex.Message);
            //content.AppendFormat("StackTrace: {0}\n", ex.StackTrace);

            //if (ex.InnerException != null)
            //{
            //    content.AppendFormat("Inner Exception: {0}\n", ex.InnerException.Message);
            //    content.AppendFormat("Inner StackTrace: {0}\n", ex.InnerException.StackTrace);
            //}



            //var result = await dlgService.ShowMessage(resLdr.GetString("MessageErrorText"), resLdr.GetString("MessageErrorTitle"), resLdr.GetString("ButtonOkText"), resLdr.GetString("ButtonCancelText"), null);
            //if (result)
            //{
            //    string title = resLdr.GetString("AppName") + " " + resLdr.GetString("MessageErrorTitle");
            //    await SendEmail(title, content.ToString(), "grigory@zhukoff.pro");
            //    Application.Current.Exit();
            //}
        }
    }
}