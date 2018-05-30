using System;
using System.ServiceModel.Channels;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Olsa.WCF.Extensions
{
    class AuthenticationMessageHeader : System.ServiceModel.Channels.MessageHeader
    {
        /// <summary>
        /// 
        /// </summary>
        internal const string UsernameTokenType = "uri:usernameTokenSample";

        /// <summary>
        /// 
        /// </summary>
        internal const string UsernameTokenPrefix = "wsse";
        /// <summary>
        /// 
        /// </summary>
        internal const string UsernameTokenNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        /// <summary>
        /// 
        /// </summary>
        internal const string UsernameTokenName = "UsernameToken";

        /// <summary>
        /// 
        /// </summary>
        internal const string IdAttributeName = "Id";
        /// <summary>
        /// 
        /// </summary>
        internal const string WSUtilityPrefix = "wsu";
        /// <summary>
        /// 
        /// </summary>
        internal const string WSUtilityNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        /// <summary>
        /// 
        /// </summary>
        internal const string UsernameElementName = "Username";
        /// <summary>
        /// 
        /// </summary>
        internal const string PasswordElementName = "Password";
        /// <summary>
        /// 
        /// </summary>
        internal const string NonceElementName = "Nonce";
        /// <summary>
        /// 
        /// </summary>
        internal const string CreatedElementName = "Created";

        /// <summary>
        /// 
        /// </summary>
        internal const string TypeAttributeName = "Type";
        /// <summary>
        /// 
        /// </summary>
        internal const string PasswordDigestType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest";

        /// <summary>
        /// 
        /// </summary>
        internal const string TimeStamp = "Timestamp";
        /// <summary>
        /// 
        /// </summary>
        internal const string ExpiresElementName = "Expires";


        /// <summary>
        /// 
        /// </summary>
        private string UserName;
        /// <summary>
        /// 
        /// </summary>
        private string Created;
        /// <summary>
        /// 
        /// </summary>
        private string Expires;
        /// <summary>
        /// 
        /// </summary>
        private string Nonce;
        /// <summary>
        /// 
        /// </summary>
        private string PasswordDigest;
        /// <summary>
        /// 
        /// </summary>
        private Guid GUID;




        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMessageHeader" /> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public AuthenticationMessageHeader(string userName, string password) :
            this(userName, password, 5)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMessageHeader" /> class.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="validity">The validity.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public AuthenticationMessageHeader(string userName, string password, int validity)
        {
            if ((validity < 5) || (validity > 59))
            {
                throw new ArgumentOutOfRangeException("validity",
                    validity,
                   "validity must be between 5 and 59");
            }

            this.UserName = userName;

            DateTime instant = DateTime.Now;
            DateTime expire = DateTime.Now + new TimeSpan(0, validity, 0);
            this.Created = XmlConvert.ToString(instant.ToUniversalTime(), "yyyy-MM-ddTHH:mm:ssZ");
            this.Expires = XmlConvert.ToString(expire.ToUniversalTime(), "yyyy-MM-ddTHH:mm:ssZ");

            this.GUID = Guid.NewGuid();

            // Generate a cryptographically strong random value
            byte[] nonce = new byte[16];
            using (RandomNumberGenerator rndGenerator = new RNGCryptoServiceProvider())
            {
                rndGenerator.GetBytes(nonce);
            }
            this.Nonce = Convert.ToBase64String(nonce);

            //Generate the PasswordDigest
            // get other operands to the right format
            byte[] time = Encoding.UTF8.GetBytes(this.Created);
            byte[] pwd = Encoding.UTF8.GetBytes(password);
            byte[] operand = new byte[nonce.Length + time.Length + pwd.Length];
            Array.Copy(nonce, operand, nonce.Length);
            Array.Copy(time, 0, operand, nonce.Length, time.Length);
            Array.Copy(pwd, 0, operand, nonce.Length + time.Length, pwd.Length);

            // create the hash
            SHA1 sha1 = SHA1.Create();
            PasswordDigest = Convert.ToBase64String(sha1.ComputeHash(operand));
        }


        /// <summary>
        /// Called when the header content is serialized using the specified XML writer.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Xml.XmlDictionaryWriter" />.</param>
        /// <param name="messageVersion">Contains information related to the version of SOAP associated with a message and its exchange.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            //TimeStamp Header
            writer.WriteStartElement(WSUtilityPrefix, TimeStamp, WSUtilityNamespace);
            writer.WriteAttributeString(WSUtilityPrefix, IdAttributeName, WSUtilityNamespace, "Timestamp-" + this.GUID.ToString());
            writer.WriteElementString(CreatedElementName, WSUtilityNamespace, Created);
            writer.WriteElementString(ExpiresElementName, WSUtilityNamespace, Expires);
            writer.WriteEndElement();

            //UserNameToken Header
            writer.WriteStartElement(UsernameTokenPrefix, UsernameTokenName, UsernameTokenNamespace);
            writer.WriteAttributeString(WSUtilityPrefix, IdAttributeName, WSUtilityNamespace, "SecurityToken-" + this.GUID.ToString());
            writer.WriteElementString(UsernameElementName, UsernameTokenNamespace, this.UserName);
            writer.WriteStartElement(UsernameTokenPrefix, PasswordElementName, UsernameTokenNamespace);
            writer.WriteAttributeString(TypeAttributeName, PasswordDigestType);
            writer.WriteValue(PasswordDigest);
            writer.WriteEndElement();
            writer.WriteElementString(NonceElementName, UsernameTokenNamespace, Nonce);
            writer.WriteElementString(CreatedElementName, WSUtilityNamespace, Created);
            writer.WriteEndElement();
        }


        /// <summary>
        /// Gets the name of the message header.
        /// </summary>
        /// <returns>The name of the message header.</returns>
        public override string Name
        {
            get { return "Security"; }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether the header must be understood, according to SOAP 1.1/1.2 specification.
        /// </summary>
        /// <returns>true if the header must be understood; otherwise, false.</returns>
        public override bool MustUnderstand
        {
            get { return false; }
        }


        /// <summary>
        /// Gets the namespace of the message header.
        /// </summary>
        /// <returns>The namespace of the message header.</returns>
        public override string Namespace
        {
            get { return UsernameTokenNamespace; }
        }
    }
}
