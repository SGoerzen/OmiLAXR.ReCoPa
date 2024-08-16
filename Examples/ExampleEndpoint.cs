using OmiLAXR.Composers;
using OmiLAXR.Endpoints;
using UnityEngine;

namespace OmiLAXR.Adapters.YOUR_ADAPTER_NAME
{
    [AddComponentMenu("OmiLAXR / 6) Endpoints / Example Endpoint (Adapter.YOUR_ADAPTER_NAME)")]
    public sealed class ExampleEndpoint : DataEndpoint
    {
        protected override TransferCode HandleSending(IStatement statement)
        {
            // do something here e.g. saving in LRS, locally or MongoDB
            Debug.Log(statement);
            return TransferCode.Success;
        }
    }
}
