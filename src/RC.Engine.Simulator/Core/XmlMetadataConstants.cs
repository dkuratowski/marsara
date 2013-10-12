using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RC.Engine.Simulator.Core
{
    /// <summary>
    /// Constants defined for reading and writing metadata XML descriptors.
    /// </summary>
    static class XmlMetadataConstants
    {
        public const string SIMMETADATA_ELEM = "simulationMetadata";
        public const string DATATYPE_ELEM = "dataType";
        public const string DATATYPE_NAME_ATTR = "name";
        public const string FIELD_ELEM = "field";
        public const string FIELD_NAME_ATTR = "name";
        public const string FIELD_TYPE_ATTR = "type";
        public const string FIELDBIT_ELEM = "bit";
        public const string FIELDBIT_NAME_ATTR = "name";
        public const string ELEMENTTYPE_ELEM = "elementType";
        public const string ELEMENTTYPE_NAME_ATTR = "name";
        public const string INDICATOR_ELEM = "indicator";
        public const string INDICATOR_IMAGE_ATTR = "image";
        public const string INDICATOR_TRANSPCOLOR_ATTR = "transparentColor";
        public const string INDICATOR_OWNERMASKCOLOR_ATTR = "ownerMaskColor";
        public const string ANIMATION_ELEM = "animation";
        public const string ANIMATION_NAME_ATTR = "name";
        public const string FRAME_ELEM = "frame";
        public const string FRAME_SOURCEREGION_ATTR = "sourceRegion";
        public const string FRAME_OFFSET_ATTR = "offset";
        public const string FRAME_REPEAT_ATTR = "repeat";
        public const string BEHAVIOR_ELEM = "behavior";
        public const string BEHAVIOR_TYPE_ATTR = "type";
        public const string BEHAVIORTYPE_ELEM = "behaviorType";
        public const string BEHAVIORTYPE_NAME_ATTR = "name";
        public const string FACTORY_ELEM = "factory";
        public const string ASSEMBLY_ELEM = "assembly";
        public const string CLASS_ELEM = "class";
    }
}
