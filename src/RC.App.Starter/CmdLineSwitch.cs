using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RC.Common.Configuration;
using System.Reflection;

namespace RC.App.Starter
{
    /// <summary>
    /// Abstract base class of the command line switches.
    /// </summary>
    abstract class CmdLineSwitch
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static CmdLineSwitch()
        {
            switchTypes = new Dictionary<string, Type>();
            switches = new List<CmdLineSwitch>();

            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types)
            {
                if (t.IsSubclassOf(typeof(CmdLineSwitch)))
                {
                    try
                    {
                        FieldInfo field = t.GetField("SIGNATURE");
                        string cmdName = (string)field.GetValue(null);
                        switchTypes.Add(cmdName, t);
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine("Unable to register switch '" + t.Name + "'");
                        Console.WriteLine("Error: " + e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="tokens">The tokens of the command line.</param>
        public static void ParseCommandLine(string[] tokens)
        {
            List<string> args = new List<string>();
            Type switchType = null;
            foreach (string token in tokens)
            {
                string tokenStr = token.Trim();
                if (switchTypes.ContainsKey(tokenStr))
                {
                    /// Beginning of a new switch has been found.
                    if (switchType != null)
                    {
                        object[] constructorArgs = new object[1] { args.ToArray() };
                        CmdLineSwitch sw = (CmdLineSwitch)Activator.CreateInstance(switchType, constructorArgs);
                        switches.Add(sw);
                        args.Clear();
                    }
                    switchType = switchTypes[tokenStr];
                }
                else
                {
                    args.Add(tokenStr);
                }
            }

            /// End of last switch
            if (switchType != null)
            {
                object[] constructorArgs = new object[1] { args.ToArray() };
                CmdLineSwitch sw = (CmdLineSwitch)Activator.CreateInstance(switchType, constructorArgs);
                switches.Add(sw);
                args.Clear();
            }
        }

        /// <summary>
        /// Executes the found command line switches.
        /// </summary>
        public static void ExecuteSwitches()
        {
            foreach (CmdLineSwitch sw in switches)
            {
                sw.Execute();
            }
        }

        /// <summary>
        /// Constructs a CmdLineSwitch object.
        /// </summary>
        /// <param name="args">The arguments of the switch.</param>
        public CmdLineSwitch(string[] args)
        {
            this.arguments = args;
        }

        /// <summary>
        /// Executes the switch.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Gets the arguments of the switch.
        /// </summary>
        protected string[] Arguments { get { return this.arguments; } }

        /// <summary>
        /// List of the possible command line switches.
        /// </summary>
        private static Dictionary<string, Type> switchTypes;

        /// <summary>
        /// List of the found command line switches.
        /// </summary>
        private static List<CmdLineSwitch> switches;

        /// <summary>
        /// The arguments of the switch.
        /// </summary>
        private readonly string[] arguments;
    }

    /// <summary>
    /// Command line switch for setting a configuration root file different from the default (RC.root).
    /// </summary>
    class ConfigFileSwitch : CmdLineSwitch
    {
        /// <summary>
        /// The signature of the command line switch.
        /// </summary>
        public static readonly string SIGNATURE = "/cfg";

        /// <summary>
        /// Constructs a ConfigFileSwitch object.
        /// </summary>
        /// <param name="args">The arguments of the switch.</param>
        public ConfigFileSwitch(string[] args) : base(args)
        {
            if (args == null || args.Length != 1) { throw new Exception("/cfg switch usage: '/cfg filename'"); }
            if (args[0] == null || args[0].Length == 0) { throw new Exception("/cfg switch usage: '/cfg filename'"); }
        }

        /// <see cref="CmdLineSwitch.Execute"/>
        public override void Execute()
        {
            ConfigurationManager.Initialize(this.Arguments[0]);
        }
    }

    /// <summary>
    /// Command line switch for running RC.exe with a console window (hidden by default).
    /// </summary>
    class ConsoleSwitch : CmdLineSwitch
    {
        /// <summary>
        /// The signature of the command line switch.
        /// </summary>
        public static readonly string SIGNATURE = "/c";

        /// <summary>
        /// Constructs a ConsoleSwitch object.
        /// </summary>
        /// <param name="args">The arguments of the switch.</param>
        public ConsoleSwitch(string[] args) : base(args)
        {
        }

        /// <see cref="CmdLineSwitch.Execute"/>
        public override void Execute()
        {
            ConsoleHelper.ShowConsole();
        }
    }

    /// <summary>
    /// Command line switch for start the map editor with a new map.
    /// </summary>
    class NewMapSwitch : CmdLineSwitch
    {
        /// <summary>
        /// The signature of the command line switch.
        /// </summary>
        public static readonly string SIGNATURE = "/newmap";

        /// <summary>
        /// The indices of the arguments.
        /// </summary>
        public const int MAP_FILE = 0;
        public const int TILESET_NAME = 1;
        public const int DEFAULT_TERRAIN = 2;
        public const int SIZE = 3;

        /// <summary>
        /// Constructs a NewMapSwitch object.
        /// </summary>
        /// <param name="args">The arguments of the switch.</param>
        public NewMapSwitch(string[] args) : base(args)
        {
            if (args == null || args.Length != 4) { throw new Exception("/newmap switch usage: '/newmap map-file tileset-name default-terrain size'. Example: '/newmap mymap.rcm Bandlands Dirt 64;32'."); }
            if (args[0] == null || args[0].Length == 0) { throw new Exception("/newmap switch usage: '/newmap map-file tileset-name default-terrain size'. Example: '/newmap mymap.rcm Bandlands Dirt 64;32'."); }
            if (args[1] == null || args[1].Length == 0) { throw new Exception("/newmap switch usage: '/newmap map-file tileset-name default-terrain size'. Example: '/newmap mymap.rcm Bandlands Dirt 64;32'."); }
            if (args[2] == null || args[2].Length == 0) { throw new Exception("/newmap switch usage: '/newmap map-file tileset-name default-terrain size'. Example: '/newmap mymap.rcm Bandlands Dirt 64;32'."); }
            if (args[3] == null || args[3].Length == 0) { throw new Exception("/newmap switch usage: '/newmap map-file tileset-name default-terrain size'. Example: '/newmap mymap.rcm Bandlands Dirt 64;32'."); }
        }

        /// <see cref="CmdLineSwitch.Execute"/>
        public override void Execute()
        {
            MapEditorSetup.Mode = MapEditorMode.NewMap;
            MapEditorSetup.MapFile = this.Arguments[MAP_FILE];
            MapEditorSetup.TilesetName = this.Arguments[TILESET_NAME];
            MapEditorSetup.DefaultTerrain = this.Arguments[DEFAULT_TERRAIN];
            MapEditorSetup.MapSize = XmlHelper.LoadVector(this.Arguments[SIZE]);
        }
    }

    /// <summary>
    /// Command line switch for start the map editor an load an existing map.
    /// </summary>
    class LoadMapSwitch : CmdLineSwitch
    {
        /// <summary>
        /// The signature of the command line switch.
        /// </summary>
        public static readonly string SIGNATURE = "/loadmap";

        /// <summary>
        /// The indices of the arguments.
        /// </summary>
        public const int MAP_FILE = 0;

        /// <summary>
        /// Constructs a LoadMapSwitch object.
        /// </summary>
        /// <param name="args">The arguments of the switch.</param>
        public LoadMapSwitch(string[] args) : base(args)
        {
            if (args == null || args.Length != 1) { throw new Exception("/loadmap switch usage: '/loadmap map-file'. Example: '/loadmap mymap.rcm'."); }
            if (args[0] == null || args[0].Length == 0) { throw new Exception("/loadmap switch usage: '/loadmap map-file'. Example: '/loadmap mymap.rcm'."); }
        }

        /// <see cref="CmdLineSwitch.Execute"/>
        public override void Execute()
        {
            MapEditorSetup.Mode = MapEditorMode.LoadMap;
            MapEditorSetup.MapFile = this.Arguments[MAP_FILE];
        }
    }
}
