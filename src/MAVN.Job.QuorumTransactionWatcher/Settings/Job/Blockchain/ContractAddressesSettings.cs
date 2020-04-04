using JetBrains.Annotations;

namespace MAVN.Job.QuorumTransactionWatcher.Settings.Job.Blockchain
{
    [UsedImplicitly]
    public class ContractAddressesSettings
    {
        public string CustomerRegistry { get; set; }

        public string PartnersPayments { get; set; }

        public string PaymentTransfers { get; set; }

        public string RoleRegistry { get; set; }

        public string Token { get; set; }

        public string Gateway { get; set; }

        public string MVNVouchersPayments { get; set; }
    }
}
