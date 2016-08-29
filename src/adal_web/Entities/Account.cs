using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace adal_web.Entities
{
    /// <summary>
    /// A local in-memory representation of an Account entity.
    /// </summary>
    public class Account
    {
        public Guid accountid { get; set; }
        public string name { get; set; }
        public string telephone1 { get; set; }
    }
}
