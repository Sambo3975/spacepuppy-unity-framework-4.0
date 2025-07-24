using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Utils;


namespace com.spacepuppy.Statistics
{

    [System.Serializable]
    public class SimpleTokenLedgerService : ServiceComponent<IStatisticsTokenLedgerService>, IStatisticsTokenLedgerService
    {

        #region Fields

        [Space(10f)]
        [SerializeField, DisplayFlat(DisplayBox = true)]
        private Messaging.MessageSendCommand _onChangedMessageSettings;

        [System.NonSerialized]
        private Ledger _ledger = new Ledger();

        #endregion

        #region CONSTRUCTOR

        public SimpleTokenLedgerService()
            : base(Services.AutoRegisterOption.Register, Services.MultipleServiceResolutionOption.UnregisterSelf, Services.UnregisterResolutionOption.DestroySelf)
        {

        }

        public SimpleTokenLedgerService(Services.AutoRegisterOption autoRegister, Services.MultipleServiceResolutionOption multipleServiceResolution, Services.UnregisterResolutionOption unregisterResolution)
            : base(autoRegister, multipleServiceResolution, unregisterResolution)
        {

        }

        protected override void OnValidAwake()
        {
            base.OnValidAwake();

            _ledger.Changed -= _ledger_Changed;
            _ledger.Changed += _ledger_Changed;
        }

        protected override void OnServiceUnregistered()
        {
            base.OnServiceUnregistered();

            _ledger.Changed -= _ledger_Changed;
        }

        #endregion

        #region Properties

        public Ledger Ledger => _ledger;

        public bool SignalTokenLedgerChanged { get; set; } = true;

        #endregion

        #region Methods

        private void _ledger_Changed(object sender, LedgerChangedEventArgs ev)
        {
            this.OnLedgerChanged();
            if (this.SignalTokenLedgerChanged)
            {
                _onChangedMessageSettings.Send<IStatisticsTokenLedgerChangedHandler, System.ValueTuple<SimpleTokenLedgerService, LedgerChangedEventArgs>>(this.gameObject, (this, ev), (o, a) => o.OnChanged(a.Item1, a.Item2));
                Messaging.Broadcast<IStatisticsTokenLedgerChangedGlobalHandler, System.ValueTuple<SimpleTokenLedgerService, LedgerChangedEventArgs>>((this, ev), (o, a) => o.OnChanged(a.Item1, a.Item2));
            }
        }

        protected virtual void OnLedgerChanged()
        {

        }

        #endregion

        #region IStatisticsTokenLedger Interface

        public virtual void AdjustStat(StatId stat, double amount)
        {
            _ledger.AdjustStat(stat, amount);
        }

        public virtual void SetStat(StatId stat, double amount)
        {
            _ledger.SetStat(stat, amount, true);
        }

        public virtual bool TryGetStat(StatId stat, out double? value)
        {
            return _ledger.TryGetStat(stat, out value);
        }

        public virtual void ClearStat(StatId stat)
        {
            _ledger.ClearStat(stat);
        }

        public virtual void ResetStats()
        {
            _ledger.Reset();
        }

        public IEnumerable<LedgerStatData> EnumerateStats(string filtercategory = null)
        {
            return string.IsNullOrEmpty(filtercategory) ? _ledger.GetAllStatTokenEntries() : _ledger.GetStatTokenEntries(filtercategory);
        }

        #endregion

    }

}
