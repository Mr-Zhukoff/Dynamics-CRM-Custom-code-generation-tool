using GalaSoft.MvvmLight;
using System;

namespace Zhukoff.CRM.SvcUtilExtensions.WpfGuiApp.ViewModel
{
    public class EntityMetaObject : ObservableObject
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { Set(() => IsSelected, ref this._isSelected, value); }
        }

        private Guid _metaId;
        public Guid MetaId
        {
            get { return _metaId; }
            set { Set(() => MetaId, ref this._metaId, value); }
        }

        private string _schemaName;
        public string SchemaName
        {
            get { return _schemaName; }
            set { Set(() => SchemaName, ref this._schemaName, value); }
        }

        private string _logicalName;
        public string LogicalName
        {
            get { return _logicalName; }
            set { Set(() => LogicalName, ref this._logicalName, value); }
        }

        private int _objectTypeCode;
        public int ObjectTypeCode
        {
            get { return _objectTypeCode; }
            set { Set(() => ObjectTypeCode, ref this._objectTypeCode, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { Set(() => DisplayName, ref this._displayName, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { Set(() => Description, ref this._description, value); }
        }

        private bool _isCustomEntity;
        public bool IsCustomEntity
        {
            get { return _isCustomEntity; }
            set { Set(() => IsCustomEntity, ref this._isCustomEntity, value); }
        }

        private bool _isManaged;
        public bool IsManaged
        {
            get { return _isManaged; }
            set { Set(() => IsManaged, ref this._isManaged, value); }
        }

        private bool _isMappable;
        public bool IsMappable
        {
            get { return _isMappable; }
            set { Set(() => IsMappable, ref this._isMappable, value); }
        }

        private bool _isValidForAdvancedFind;
        public bool IsValidForAdvancedFind
        {
            get { return _isValidForAdvancedFind; }
            set { Set(() => IsValidForAdvancedFind, ref this._isValidForAdvancedFind, value); }
        }

        private bool _isValidForQueue;
        public bool IsValidForQueue
        {
            get { return _isValidForQueue; }
            set { Set(() => IsValidForQueue, ref this._isValidForQueue, value); }
        }

        private bool _isVisibleInMobile;
        public bool IsVisibleInMobile
        {
            get { return _isVisibleInMobile; }
            set { Set(() => IsVisibleInMobile, ref this._isVisibleInMobile, value); }
        }
    }
}
