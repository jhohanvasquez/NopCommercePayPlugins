using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.PayUColombia.Enumerator
{
    /// <summary>
    /// Represents a payment status enumeration
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Approved = 4,

        /// <summary>
        /// Pending
        /// </summary>
        Pending = 7,

        /// <summary>
        /// Authorized
        /// </summary>
        Declined = 6,

        /// <summary>
        /// Authorized
        /// </summary>
        Error = 104,

        /// <summary>
        /// Paid
        /// </summary>
        Expired = 5
    }
}
