== Skillport OLSA Package ==
The Skillport OLSA package adds the libraries to allow the developer to use
Skillsoft Automation API (OLSA) via WCF.

== Supported Skillport Version ==
8.0.9751

== Supported .NET Frameworks ==
.NET 4.0 or later

== Configuration ==
Once the libraries have been added to your project the app.config or web.config
as appropriate will have been added/updated to add the relevant WCF bindings and
behaviours.

Add the specific details for your OLSA site:
YOUROLSASERVER - The OLSA server endpoint
YOURSHAREDSECRET - OLSA Shared Secret
YOURCUSTOMERID - OLSA Customer ID

== TLS 1.2 Support ==
Skillsoft enforce TLS 1.2 for HTTPS connections.
Ensure you have followed Microsoft Guidance on enabling this
https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls

== Example configuring OLSA inline, instead of via config file ==
This example class implements a static OlsaHelpers class, with a function to get the OlsaPortTypeClient

        namespace Examples
        {
            using System;
            using System.Text;
            using System.ServiceModel;
            using System.ServiceModel.Channels;
            using Olsa;
            using Olsa.WCF.Extensions;

            public static class OlsaHelpers
            {
                /// <summary>
                /// Gets the olsa client
                /// </summary>
                /// <param name="olsaServerEndpoint">The olsa server endpoint.</param>
                /// <param name="olsaCustomerId">The olsa customer identifier.</param>
                /// <param name="olsaSharedSecret">The olsa shared secret.</param>
                /// <returns></returns>
                public static OlsaPortTypeClient GetOLSAClient(Uri olsaServerEndpoint, string olsaCustomerId, string olsaSharedSecret)
                {
                    //Set the encoding to SOAP 1.1, Disable Addressing and set encoding to UTF8
                    TextMessageEncodingBindingElement messageEncoding = new TextMessageEncodingBindingElement();
                    messageEncoding.MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.None);
                    messageEncoding.WriteEncoding = Encoding.UTF8;

                    //Setup Binding Elemment
                    HttpTransportBindingElement transportBinding = new HttpsTransportBindingElement();

                    //Set the maximum received messages sizes to 1Mb
                    transportBinding.MaxReceivedMessageSize = 1024 * 1024;
                    transportBinding.MaxBufferPoolSize = 1024 * 1024;

                    //Create the CustomBinding
                    Binding customBinding = new CustomBinding(messageEncoding, transportBinding);

                    //Create the OLSA Service
                    EndpointAddress serviceAddress = new EndpointAddress(olsaServerEndpoint);

                    //Set the endPoint URL YOUROLSASERVER/olsa/services/Olsa has to be HTTPS
                    OlsaPortTypeClient service = new OlsaPortTypeClient(customBinding, serviceAddress);

                    //Add Behaviour to support SOAP UserNameToken Password Digest
                    AuthenticationBehavior behavior1 = new AuthenticationBehavior(olsaCustomerId, olsaSharedSecret);
                    service.Endpoint.Behaviors.Add(behavior1);

                    //Add Behaviour to support fix of Namespaces to address AXIS2 / VWCF incompatability
                    NameSpaceFixUpBehavior behavior2 = new NameSpaceFixUpBehavior();
                    service.Endpoint.Behaviors.Add(behavior2);

                    return service;
                }
            }
        }

== Example Project to use OLSA Function ==
A sample project showing how to use the packaged to call the OLSA Web Services to generate and download a report.
https://github.com/martinholden-skillsoft/SkillsoftReportConsole


=== END ===