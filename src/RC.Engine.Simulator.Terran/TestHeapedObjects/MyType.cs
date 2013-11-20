using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Terran.TestHeapedObjects
{
    class MyType : MyBaseType
    {
        public MyType()
        {
            this.privateShortMember = this.ConstructField<short>("privateShortMember");
            this.publicShortMember = this.ConstructField<short>("publicShortMember");
            this.protectedIntMember = this.ConstructField<int>("protectedIntMember");
            this.privateReference = this.ConstructField<MyBaseType>("privateReference");
            this.privateArray = this.ConstructArrayField<MyBaseType>("privateArray");
        }

        HeapedValue<short> privateShortMember;

        public HeapedValue<short> publicShortMember;

        protected HeapedValue<int> protectedIntMember;

        HeapedValue<MyBaseType> privateReference;

        HeapedArray<MyBaseType> privateArray;

        short shortNonMember;
    }
}
