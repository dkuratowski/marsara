using System;
using System.Collections.Generic;

namespace RC.Common
{
    /// <summary>
    /// Static class for managing Random objects.
    /// </summary>
    public static class RandomService
    {
        /// <summary>
        /// Static constructor of this class.
        /// </summary>
        static RandomService()
        {
            randomGenerators = new Dictionary<int, Random>();
            defaultGenerator = new Random();
        }

        /// <summary>
        /// Gets the random generator with the given seed number.
        /// </summary>
        /// <param name="seedNum">The seed number of the random generator you want to get.</param>
        /// <returns>The random generator with the given seed number.</returns>
        /// <remarks>If no such random generator exists then this function will create one.</remarks>
        public static Random GetGenerator(int seedNum)
        {
            if (!randomGenerators.ContainsKey(seedNum))
            {
                randomGenerators.Add(seedNum, new Random(seedNum));
            }
            return randomGenerators[seedNum];
        }

        /// <summary>
        /// Gets the default random generator object that has been initialized with the time at the beginning
        /// of the application.
        /// </summary>
        public static Random DefaultGenerator { get { return defaultGenerator; } }

        /// <summary>
        /// The default random generator.
        /// </summary>
        private static Random defaultGenerator;

        /// <summary>
        /// List of the random generator objects mapped by their seed numbers.
        /// </summary>
        private static Dictionary<int, Random> randomGenerators;
    }
}
