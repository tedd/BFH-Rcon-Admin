using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.BFHAdmin.Common
{
    public abstract class ObjectCloningBase<T>
    {
        public T ShallowCopy<T>()
        {
            return (T)this.MemberwiseClone();
        }
        // Add this if using nested MemberwiseClone.
        // This is a class, which is a reference type, so cloning is more difficult.
        public abstract T DeepCopy<T>();

    }
}
