using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Maps.PublicInterfaces
{
    public class TileSetException : Exception
    {
        public TileSetException() { }
        public TileSetException(string message) : base(message) { }
        public TileSetException(string message, Exception inner) : base(message, inner) { }
        protected TileSetException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
