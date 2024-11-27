using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Interfaces
{
    public interface IUser
    {
        public string UserName { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; }
    }
}
