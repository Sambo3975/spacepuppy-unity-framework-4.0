using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy;
using com.spacepuppy.Utils;
using com.spacepuppy.Events;
using com.spacepuppy.Async;
using com.spacepuppy.Collections;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.DataBinding
{

    public class LayoutGroupDataBindingContext : SPComponent, IDataBindingContext, IDataProvider, IMActivateOnReceiver
    {

        #region Fields

        [SerializeField]
        private int _bindOrder;

        [SerializeField()]
        private ActivateEvent _activateOn = ActivateEvent.None;

        [SerializeField]
        [SelectableObject(AllowProxy = true)]
        [Tooltip("A datasource to be binded to during the 'ActivateOn' event or if calling 'BindConfiguredDataSource'.")]
        private UnityEngine.Object _dataSource;

        [SerializeField]
        private bool _respectProxySources = false;

        [SerializeField]
        private int _maxVisible = 100;

        [SerializeField]
        [DefaultFromSelf]
        private Transform _container;

        [SerializeField]
        private Messaging.MessageSendCommand _bindMessageSettings = new Messaging.MessageSendCommand()
        {
            SendMethod = Messaging.MessageSendMethod.Broadcast,
            IncludeDisabledComponents = true,
            IncludeInactiveObjects = true,
        };

        [SerializeReference, SerializeRefPicker(typeof(IStampSource), AllowNull = false, AlwaysExpanded = true, DisplayBox = true)]
        private IStampSource _stampSource = new GameObjectStampSource();

        #endregion

        #region Properties

        public int BindOrder
        {
            get => _bindOrder;
            set => _bindOrder = value;
        }

        public ActivateEvent ActivateOn
        {
            get => _activateOn;
            set => _activateOn = value;
        }

        public UnityEngine.Object ConfiguredDataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        public bool RespectProxySources
        {
            get => _respectProxySources;
            set => _respectProxySources = value;
        }

        public int MaxVisible
        {
            get => _maxVisible;
            set => _maxVisible = value;
        }

        public Transform Container
        {
            get => _container;
            set => _container = value;
        }

        public Messaging.MessageSendCommand BindMessageSettings
        {
            get => _bindMessageSettings;
            set => _bindMessageSettings = value;
        }

        public IStampSource StampSource
        {
            get => _stampSource;
            set => _stampSource = value;
        }

        #endregion

        #region Methods

        public void ClearStamps()
        {
            if (_container && _container.childCount > 0)
            {
                foreach (Transform t in _container)
                {
                    t.gameObject.Kill();
                }
            }
        }

        public void BindConfiguredDataSource()
        {
            this.Bind(_dataSource);
        }

        public void Bind(object source)
        {
            _ = this.DoBindAsync(source, null);
        }

        public AsyncWaitHandle BindAsync(object source)
        {
            return this.DoBindAsync(source, null).AsAsyncWaitHandle();
        }

        public AsyncWaitHandle<int> BindAsync(object source, ICollection<GameObject> output)
        {
            return this.DoBindAsync(source, output).AsAsyncWaitHandle();
        }

#if SP_UNITASK
        private async UniTask<int> DoBindAsync(object source, ICollection<GameObject> output)
#else
        private async System.Threading.Tasks.Task<int> DoBindAsync(object source, ICollection<GameObject> output)
#endif
        {
            var dataprovider = DataProviderUtils.GetAsDataProvider(source, _respectProxySources);
            this.DataSource = dataprovider;

            this.ClearStamps();

            if (dataprovider == null) return 0;

            if (_stampSource == null)
            {
                return 0;
            }
            else if (_stampSource.IsAsync)
            {
                int index = 0;
                foreach (var item in dataprovider.Cast<object>().Take(_maxVisible))
                {
                    GameObject inst = await _stampSource.InstantiateStampAsync(_container, item);
                    if (inst == null) continue;

                    DataBindingContext.SendBindMessage(_bindMessageSettings, inst, item, index);
                    index++;
                    output?.Add(inst);
                }
                return index;
            }
            else
            {
                int index = 0;
                foreach (var item in dataprovider.Cast<object>().Take(_maxVisible))
                {
                    GameObject inst = _stampSource.InstantiateStamp(_container, item);
                    DataBindingContext.SendBindMessage(_bindMessageSettings, inst, item, index);
                    index++;
                    output?.Add(inst);
                }
                return index;
            }
        }

        #endregion

        #region IDataBindingContext Interface

        int IDataBindingMessageHandler.BindOrder => _bindOrder;

        public object DataSource { get; private set; }

        public void Bind(object source, int index)
        {
            this.Bind(source);
        }

        #endregion

        #region IDataProvider Interface

        System.Type IDataProvider.ElementType => typeof(object);

        object IDataProvider.FirstElement => DataProviderUtils.GetFirstElementOfDataProvider(this.DataSource);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return DataProviderUtils.GetAsDataProvider(this.DataSource)?.GetEnumerator() ?? Enumerable.Empty<object>().GetEnumerator();
        }

        #endregion

        #region IMActivateOnReceiver Interface

        void IMActivateOnReceiver.Activate()
        {
            if (_dataSource) this.BindConfiguredDataSource();
        }

        #endregion

    }
}
