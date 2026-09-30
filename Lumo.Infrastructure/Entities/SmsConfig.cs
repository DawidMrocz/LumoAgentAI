using System;
using System.Collections.Generic;
using System.Text;

namespace Lumo.Infrastructure.Entities
{
    public sealed class SmsConfig
    {
        public int Id { get; set; }

        public string AccountSid { get; set; } = string.Empty;

        public string AuthToken { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public bool TestMode { get; set; } = true;

        public int MaxRetries { get; set; } = 3;

        public int TimeoutSeconds { get; set; } = 30;
    }
}
