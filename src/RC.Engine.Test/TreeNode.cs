using RC.Engine.Simulator.PublicInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Test
{
    class TreeNode : HeapedObject
    {
        public TreeNode()
        {
            this.ID = this.ConstructField<int>("ID");
        }

        public HeapedValue<int> ID;

        protected static int nextId = 0;
    }

    class TreeLeaf : TreeNode
    {
        public TreeLeaf()
        {
            this.Values = this.ConstructArrayField<int>("Values");
            this.ID.Write(nextId);
            nextId++;
        }

        public HeapedArray<int> Values;
    }

    class TreeBranch : TreeNode
    {
        public TreeBranch()
        {
            this.Children = this.ConstructArrayField<TreeNode>("Children");
            this.ID.Write(nextId);
            nextId++;
        }

        public HeapedArray<TreeNode> Children;
    }
}
