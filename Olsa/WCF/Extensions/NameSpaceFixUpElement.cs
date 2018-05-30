using System;
using System.ServiceModel.Configuration;

namespace Olsa.WCF.Extensions
{
    public class NameSpaceFixUpElement : BehaviorExtensionElement
    {
        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" />.</returns>
        public override Type BehaviorType
        {
            get { return typeof(NameSpaceFixUpBehavior); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return new NameSpaceFixUpBehavior();
        }
    }
}
