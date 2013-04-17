using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using SourceFEWSAdapter.FEWSPI;
using SourceFEWSAdapter.SourceService;

namespace SourceFEWSAdapter
{
    class SourceServiceUtils
    {
        public static bool SourceServerExists(string endpoint)
        {
            try
            {
                Binding b = SourceServerBinding();
                EndpointAddress ea = new EndpointAddress(endpoint);
                InstanceContext ic = new InstanceContext(new SourceServiceCallback());
                var client = new RiverSystemServiceClient(ic, b, ea);

                client.Open();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static NetTcpBinding SourceServerBinding()
        {
            NetTcpBinding result = new NetTcpBinding();
            //result.TransferMode = TransferMode.Buffered;
            //result.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            //result.TransactionProtocol = TransactionProtocol.OleTransactions;
            //result.TransactionFlow = false;
            result.Security = new NetTcpSecurity() {Mode = SecurityMode.None};
            return result;
        }
    }
}
