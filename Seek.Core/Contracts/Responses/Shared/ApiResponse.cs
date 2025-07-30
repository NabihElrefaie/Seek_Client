using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Shared
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Payload type</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Response payload
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Response status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Request correlation ID
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Response timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
