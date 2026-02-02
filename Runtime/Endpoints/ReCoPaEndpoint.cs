using System;
using System.ComponentModel;
using OmiLAXR.Composers;
using OmiLAXR.Endpoints;
using UnityEngine;

namespace OmiLAXR.ReCoPa.Endpoints
{
    [AddComponentMenu("OmiLAXR / 6) Endpoints / ReCoPa Endpoint"),
     Description("Send statements to ReCoPa.")]
    public class ReCoPaEndpoint : Endpoint
    {
        protected override TransferCode HandleSending(IStatement statement)
        {
            return TransferCode.Success;
        }
    }
}