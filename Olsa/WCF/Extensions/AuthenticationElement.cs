using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Olsa.WCF.Extensions
{
    public class AuthenticationElement : BehaviorExtensionElement
    {
        /// <summary>
        /// 
        /// </summary>
        const string CustomerIdPropertyName = "customerid";
        /// <summary>
        /// 
        /// </summary>
        const string SharedSecretPropertyName = "sharedsecret";
        /// <summary>
        /// 
        /// </summary>
        const string ValidityPropertyName = "validity";

        /// <summary>
        /// 
        /// </summary>
        private ConfigurationPropertyCollection _prop = null;

        /// <summary>
        /// Gets the collection of properties.
        /// </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection" /> of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this._prop == null)
                {
                    this._prop = new ConfigurationPropertyCollection();
                    this._prop.Add(new ConfigurationProperty("customerid", typeof(string), "", ConfigurationPropertyOptions.IsRequired));
                    this._prop.Add(new ConfigurationProperty("sharedsecret", typeof(string), "", ConfigurationPropertyOptions.IsRequired));
                    this._prop.Add(new ConfigurationProperty("validity", typeof(int), 5, ConfigurationPropertyOptions.None));
                }
                return this._prop;
            }
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" />.</returns>
        public override Type BehaviorType
        {
            get { return typeof(AuthenticationBehavior); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new AuthenticationBehavior(this.CustomerId, this.SharedSecret, this.Validity);
        }

        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        /// <value>
        /// The customer ID.
        /// </value>
        [ConfigurationProperty(CustomerIdPropertyName, IsRequired = true)]
        public string CustomerId
        {
            get
            {
                return (string)base[CustomerIdPropertyName];
            }
            set
            {
                base[CustomerIdPropertyName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the shared secret.
        /// </summary>
        /// <value>
        /// The shared secret.
        /// </value>
        [ConfigurationProperty(SharedSecretPropertyName, IsRequired = true)]
        public string SharedSecret
        {
            get
            {
                return (string)base[SharedSecretPropertyName];
            }
            set
            {
                base[SharedSecretPropertyName] = value;
            }
        }

        /// <summary>
        /// Gets or sets the validity.
        /// </summary>
        /// <value>
        /// The validity.
        /// </value>
        [ConfigurationProperty(ValidityPropertyName)]
        public int Validity
        {
            get
            {
                return (int)base[ValidityPropertyName];
            }
            set
            {
                base[ValidityPropertyName] = value;
            }
        }
    }
}
