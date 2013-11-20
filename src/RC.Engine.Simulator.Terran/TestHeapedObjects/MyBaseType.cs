using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Terran.TestHeapedObjects
{
    class MyBaseType : HeapedObject
    {
        public MyBaseType()
        {
            this.privateIntMember = this.ConstructField<int>("privateIntMember");
            this.publicIntMember = this.ConstructField<int>("publicIntMember");
            this.protectedLongMember = this.ConstructField<long>("protectedLongMember");
        }

        HeapedValue<int> privateIntMember;

        public HeapedValue<int> publicIntMember;

        protected HeapedValue<long> protectedLongMember;

        short shortNonMember;
    }
}
