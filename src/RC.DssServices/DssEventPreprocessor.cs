using RC.Common;

namespace RC.DssServices
{
    /// <summary>
    /// Base class of objects that perform DSS-event preprocessing.
    /// </summary>
    /// <remarks>
    /// When an event arrives to the event queue, the function corresponding to the event will be called on every
    /// registered preprocessor object. The event will only be inserted into the event queue if every registered
    /// preprocessor returns true. This base class provides the default implementation of event preprocessing.
    /// </remarks>
    abstract class DssEventPreprocessor
    {
        public virtual bool PackageArrivedPre(RCPackage package, int senderID) { return true; }
        public virtual bool ControlPackageFromServerPre(RCPackage package) { return true; }
        public virtual bool ControlPackageFromClientPre(RCPackage package, int senderID) { return true; }
        public virtual bool LineOpenedPre(int lineIdx) { return true; }
        public virtual bool LineClosedPre(int lineIdx) { return true; }
        public virtual bool LineEngagedPre(int lineIdx) { return true; }
        public virtual bool LobbyLostPre() { return true; }
        public virtual bool LobbyIsRunningPre(int idOfThisPeer, int opCount) { return true; }
    }
}
