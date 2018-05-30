*******************************************************************************
THIS CODE SAMPLE IS PROVIDED "AS IS". ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO ANY WARRANTY OF NON-INFRINGEMENT, THE IMPLIED
WARRANTIES OF MERCHANTABILITY, SATISFACTORY QUALITY, REASONABLE CARE AND SKILL,
AND FITNESS FOR A PARTICULAR PURPOSE ARE EXPRESSLY DISCLAIMED.

SKILLSOFT MAKES THESE CODE SAMPLES AVAILABLE AS A CONVENIENCE FOR USERS, BUT
DOES NOT PROVIDE SUPPORT FOR CODE SAMPLES.
*******************************************************************************
JANUARY-2018

This project contains a WCF Behavior to support the OLSA Authentication
method of OASIS UserNameToken with PasswordDigest.

It also contains a WCF MessageInspector that is used to fixup the WCF output
message to include, if necessary a reference to the XSD namespace. This is
necessary to correctly support calls to the UD_SubmitReport command. 

===============================================================================
Class: Olsa.WCF.Extensions.AuthenticationBehaviour
-------------------------------------------------------------------------------
This implements:
System.ServiceModel.Dispatcher.IClientMessageInspector
http://msdn.microsoft.com/en-us/library/ms599812

A message inspector object that can be added to the MessageInspectors
collection of a WCF client to modify the message, we use it to inject the
UserNameToken element

System.ServiceModel.Description.IEndpointBehavior
http://msdn.microsoft.com/en-us/library/ms599102

Implements methods that can be used to extend run-time behavior for an endpoint
so allowing us to add the MessageInspector

===============================================================================
Class: Olsa.WCF.Extensions.AuthenticationElement
-------------------------------------------------------------------------------
This inherits:
System.ServiceModel.Configuration.BehaviorExtensionElement
http://msdn.microsoft.com/en-us/library/aa345942

It represents a configuration element that specify behavior extensions, 
allowing us to configure the WCF client in the config file.

===============================================================================
Class: Olsa.WCF.Extensions.AuthenticationMessageHeader
-------------------------------------------------------------------------------
This inherits:
System.ServiceModel.Channels.MessageHeader
http://msdn.microsoft.com/en-us/library/ms405928

This represents the content of a SOAP header element, and is what actually
creates the UserNameToken Header.

===============================================================================
Class: Olsa.WCF.Extensions.NameSpaceFixUpBehavior
-------------------------------------------------------------------------------
This implements:
System.ServiceModel.Dispatcher.IClientMessageInspector
http://msdn.microsoft.com/en-us/library/ms599812

A message inspector object that can be added to the MessageInspectors
collection of a WCF client to modify the message, we use it to modify the
Message body to add the mising XSD namespace.

System.ServiceModel.Description.IEndpointBehavior
http://msdn.microsoft.com/en-us/library/ms599102

Implements methods that can be used to extend run-time behavior for an endpoint
so allowing us to add the MessageInspector

===============================================================================
Class: Olsa.WCF.Extensions.NameSpaceFixUpElement
-------------------------------------------------------------------------------
This inherits:
System.ServiceModel.Configuration.BehaviorExtensionElement
http://msdn.microsoft.com/en-us/library/aa345942

It represents a configuration element that specify behavior extensions, 
allowing us to configure the WCF client in the config file.

===============================================================================

===============================================================================
Configuring an OLSA Proxy Stub via a Web.Config file in a WebApp
-------------------------------------------------------------------------------

Web.config example:
    <system.serviceModel>
    <!-- Register our custom extension the behaviors defined in the Olsa.WCF project-->
    <extensions>
      <behaviorExtensions>
        <!-- See http://msdn.microsoft.com/en-us/library/aa734726(v=vs.100).aspx -->
        <!-- name = unique name we reference in the endpointBehaviours -->
        <!-- type = Olsa.WCF.Extensions.AuthenticationElement classname -->
        <!--        Olsa.WCF the DLL -->
        <!--        Version=1.0.0.0 the DLL version -->
        <!--        Culture=neutral any langauge version the DLL is neutral-->
        <add name="OlsaAuthentication"
          type="Olsa.WCF.Extensions.AuthenticationElement,
          Olsa, 
          Version=1.0.0.0,
          Culture=neutral,
          PublicKeyToken=null" />
        <add name="OlsaNameSpaceFixUp"
          type="Olsa.WCF.Extensions.NameSpaceFixUpElement,
          Olsa, 
          Version=1.0.0.0,
          Culture=neutral
          " />
      </behaviorExtensions>
    </extensions>

    <!-- Define the custom binding to control the message and transport settings -->
    <!-- See http://msdn.microsoft.com/en-us/library/ms731377(v=vs.100) -->
    <bindings>
      <customBinding>
        <binding name="OlsaService_Olsa">
          <textMessageEncoding messageVersion="Soap11" writeEncoding="utf-8" />
          <!-- Increase the default message size to support large OLSa response
          such as SL_* functions -->
          <httpTransport maxBufferPoolSize="1048576"
                         maxReceivedMessageSize="1048576"
                         maxBufferSize="1048576" />
          <!-- Comment out above and use httpsTransport binding if OLSA 
          endpoint is HTTPS-->
          <!--
          <httpsTransport maxBufferPoolSize="1048576"
                              maxReceivedMessageSize="1048576"
                              maxBufferSize="1048576" />
          -->
        </binding>
      </customBinding>
    </bindings>

	<!-- Define our custom behaviors-->
    <!-- The order they are defined is the order they get applied-->
    <behaviors>
      <endpointBehaviors>
        <behavior name="OlsaBehavior">
          <!-- Enable the NameSpaceFixUp -->
          <OlsaNameSpaceFixUp />
          <!-- Configure the credentials for our Olsa Authenticaion behaviour -->
           <OlsaAuthentication customerid="YOURCOMPANYID" sharedsecret="YOURSHAREDSECRET" />
        </behavior>
      </endpointBehaviors>
    </behaviors>

    <!-- Define the OLSA Endpoint -->
    <!-- Using our customBinding defined above -->
    <!-- Using the contract Olsa.OlsaPortType defined in Olsa.dll-->
    <client>
      <endpoint address="YOUROLSAENDPOINT"
                binding="customBinding"
                bindingConfiguration="OlsaService_Olsa"
                contract="Olsa.OlsaPortType"
                name="Olsa"
                behaviorConfiguration="OlsaBehavior" />
    </client>
  </system.serviceModel>

===============================================================================

===============================================================================
Configuring an OLSA Proxy Stub via code.
-------------------------------------------------------------------------------

Example Suggestions, assuming references to OLSA.WCF.dll and the OLSA.dll
libraries added.

1. Create a function to return an OLSA Binding Configuration.
This accepts a bool that indicates whether to use HTTPS (true)
or HTTP (false) transport. 

private static Binding GetOLSAWCFBinding(bool secure)
{
    //Set the encoding to SOAP 1.1, Disable Addressing and set encoding to UTF8
    TextMessageEncodingBindingElement me = new TextMessageEncodingBindingElement();
    me.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
    me.WriteEncoding = Encoding.UTF8;

    if (secure)
    {
        HttpsTransportBindingElement myBindingElement = new HttpsTransportBindingElement();
        //Set the maximum received messages sizes to 1Mb
        myBindingElement.MaxReceivedMessageSize = 1024 * 1024;
        myBindingElement.MaxBufferPoolSize = 1024 * 1024;
        return new CustomBinding(me, myBindingElement);
    }
    else
    {
        // NOTE: we use a non-secure transport here, which means the message will be visible to others.
        HttpTransportBindingElement myBindingElement = new HttpTransportBindingElement();
        //Set the maximum received messages sizes to 1Mb
        myBindingElement.MaxReceivedMessageSize = 1024 * 1024;
        myBindingElement.MaxBufferPoolSize = 1024 * 1024;
        return new CustomBinding(me, myBindingElement);
    }
}

2. Create the OLSA service proxy.
This accepts the Endpoint URL, CustomerID and Password

private static OlsaPortTypeClient _OlsaService = null;
public static OlsaPortTypeClient OlsaService(string EndPoint, string CustomerID, string Password)
{
        if (_OlsaService == null)
        {
            //Create the OLSA Service
            EndpointAddress serviceAddress = new EndpointAddress(EndPoint);
            Binding customBinding = GetOLSAWCFBinding(EndPoint.StartsWith("https", StringComparison.InvariantCultureIgnoreCase));
            OlsaPortTypeClient service = new OlsaPortTypeClient(customBinding, serviceAddress);
            AuthenticationBehavior behavior1 = new AuthenticationBehavior(CustomerID, Password);
			NameSpaceFixUpBehavior behavior2 = new NameSpaceFixUpBehavior();
            service.Endpoint.Behaviors.Add(behavior1);
			service.Endpoint.Behaviors.Add(behavior2);
            _OlsaService = service;
        }
        return _OlsaService;
}

===============================================================================

===============================================================================
Notes about specifying a proxy server to use by applications to connect
to internet
-------------------------------------------------------------------------------
The WCF client uses the System.Net.WebRequest.DefaultWebProxy settings. This
means the proxy configuration can be set in the config file or in code.

This example code snippet below demonstrates how you could set the proxy.

options is a class with string properties for the:
proxyserver (i.e. proxyserver:8080) and
the credentials (proxyusername, proxypassword and proxydomain).

System.Net.WebProxy _proxy = null;
if (!String.IsNullOrEmpty(options.proxyserver))
{
    //We are not using the default proxy so we need to create a new proxy
    //Create the proxy server
    _proxy = new System.Net.WebProxy(options.proxyserver);

    //Create the credentials if we have them set
    if (!String.IsNullOrEmpty(options.proxyusername))
    {
        System.Net.NetworkCredential _proxyCredentials =
			new System.Net.NetworkCredential(options.proxyusername,
											 options.proxypassword,
										     options.proxydomain);
        _proxy.Credentials = _proxyCredentials;
        //Set as the default for all WCF and System.Net.WebRequest
        System.Net.WebRequest.DefaultWebProxy = _proxy;  
    }
}
===============================================================================
