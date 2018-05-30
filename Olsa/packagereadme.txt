== Skillport OLSA Package ==
The Skillport OLSA package adds the libraries to allow the developer to use
Skillsoft Automation API (OLSA) via WCF.

== Supported Skillport Version ==
8.0.7831

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
This example functions shows configuring the OLSA Client inline

        /// <summary>
        /// Gets the olsa client
        /// </summary>
        /// <param name="olsaServerEndpoint">The olsa server endpoint.</param>
        /// <param name="olsaCustomerId">The olsa customer identifier.</param>
        /// <param name="olsaSharedSecret">The olsa shared secret.</param>
        /// <returns></returns>
        private static OlsaPortTypeClient GetOLSAClient(Uri olsaServerEndpoint, string olsaCustomerId, string olsaSharedSecret)
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

== Example call to OLSA Function ==
This example illustrate calling the AS_GetCatalogAssignment function.

The example also automatically extracts the CustomerId value from the
OLSAAuthentication setting.

	    private static GetCatalogAssignmentResponse GetCatalogs(string groupCode)
        {
			//Ensure that we have enabled TLS 1.2 Communications as mandatory for Skillsoft from 31st May 2018
			//See here for more information on enabling TLS 1.2 https://docs.microsoft.com/en-us/dotnet/framework/network-programming/tls
			System.Net.ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;

            //Create the OLSA Web Services Client (using config file settinngs)
            OlsaPortTypeClient client = new OlsaPortTypeClient("Olsa");

			//Create OLSA Web Services Client inline
			//OlsaPortTypeClient client = GetOLSAClient(new Uri(YOUROLSASERVER, YOURCUSTOMERID, YOURSHAREDSECRET);

            //Set up our response object
            GetCatalogAssignmentResponse response = null;

            try
            {
                //Create our request
                GetCatalogAssignmentRequest request = new GetCatalogAssignmentRequest();

                //Pull the OlsaAuthenticationBehviour so we can extract the customerid
                Olsa.WCF.Extensions.AuthenticationBehavior olsaCredentials = (Olsa.WCF.Extensions.AuthenticationBehavior)client.ChannelFactory.Endpoint.Behaviors.Where(p => p.GetType() == typeof(Olsa.WCF.Extensions.AuthenticationBehavior)).FirstOrDefault();
                request.customerId = olsaCredentials.UserName;

                request.groupCode = groupCode;

                response = client.AS_GetCatalogAssignment(request);
            }
            catch (WebException)
            {
                // This captures any Web Exceptions such as proxy errors etc
                // See http://msdn.microsoft.com/en-us/library/48ww3ee9(VS.80).aspx
                throw;
            }
            catch (TimeoutException)
            {
                //This captures the WCF timeout exception
                throw;
            }
            //WCF fault exception will be thrown for any other issues such as Security
            catch (FaultException fe)
            {
                if (fe.Message.ToLower(CultureInfo.InvariantCulture).Contains("the security token could not be authenticated or authorized"))
                {
                    //The OLSA Credentials specified could not be authenticated
                    throw;
                }
                throw;
            }
            catch (Exception)
            {
                //Any other type of exception, perhaps out of memory
                throw;
            }
            finally
            {
                //Shutdown and dispose of the client
                if (client != null)
                {
                    if (client.State == CommunicationState.Faulted)
                    {
                        client.Abort();
                    }
                    client.Close();
                }
            }
            return response;
        }


=== END ===