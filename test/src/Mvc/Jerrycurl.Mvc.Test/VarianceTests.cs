using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jerrycurl.Mvc.Test
{
    public class VarianceTests
    {
        public void Page_Always_HasCorrectProjectionType()
        {
            // Format:
            // (PAGE TYPE, ARGS TYPE) -> PROJECTION TYPE
            // Model:
            // (User, User) -> User
            // (IUser, User) -> User
            // (User, IEnumerable<User>) -> IEnumerable<User>.Item
            // (IEnumerable<User>, User) -> IEnumerable<User>
            // Result:
            // (User, User) -> User
            // (IUser, User) -> User
            // (User, IEnumerable<User>) -> IEnumerable<User>.Item
            // (IEnumerable<User>, User) -> IEnumerable<User>
        }
    }
}
