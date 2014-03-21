using System;
using System.Text;
using RC.Common.Diagnostics;

namespace RC.Common
{
    /// <summary>
    /// Enumerates the possible types of an RCPackage.
    /// </summary>
    public enum RCPackageType
    {
        /// Custom data package for general usage (ASCII character 'D')
        CUSTOM_DATA_PACKAGE = 0x44,

        /// An empty package that can be used as a ping between the peers in a network environment (ASCII character 'P')
        NETWORK_PING_PACKAGE = 0x50,

        /// Package type for sending control messages between adjacent peers in a network environment (ASCII character 'C')
        NETWORK_CONTROL_PACKAGE = 0x43,

        /// Package type for sending custom messages between not necessarily adjacent peers in a network environment
        /// (ASCII character 'M')
        NETWORK_CUSTOM_PACKAGE = 0x4D,

        /// Used to indicate error cases (for internal use only).
        UNDEFINED = 0xFF
    }

    /// <summary>
    /// All binary informations can be contained by RCPackages. This class represents such a package and provides methods
    /// for build up packages from custom data or deserialize them from incoming byte buffers.
    /// </summary>
    public class RCPackage
    {
        #region Static methods

        /// <summary>
        /// Creates an empty ping package.
        /// </summary>
        /// <returns>The created package.</returns>
        public static RCPackage CreateNetworkPingPackage()
        {
            RCPackage package = new RCPackage(false, RCPackageType.NETWORK_PING_PACKAGE, null);
            return package;
        }

        /// <summary>
        /// Creates a custom data package with the given format.
        /// </summary>
        /// <returns>The created package.</returns>
        /// <exception cref="RCPackageException">In case of unknown package format.</exception>
        public static RCPackage CreateCustomDataPackage(int formatID)
        {
            RCPackageFormat format = RCPackageFormat.GetPackageFormat(formatID);
            if (format != null)
            {
                RCPackage package = new RCPackage(false, RCPackageType.CUSTOM_DATA_PACKAGE, format);
                return package;
            }
            else { throw new RCPackageException("Unknown RCPackageFormat: " + formatID); }
        }

        /// <summary>
        /// Creates a custom package with the given format that can be used in a network environment.
        /// </summary>
        /// <returns>The created package.</returns>
        /// <exception cref="RCPackageException">In case of unknown package format.</exception>
        public static RCPackage CreateNetworkCustomPackage(int formatID)
        {
            RCPackageFormat format = RCPackageFormat.GetPackageFormat(formatID);
            if (format != null)
            {
                RCPackage package = new RCPackage(false, RCPackageType.NETWORK_CUSTOM_PACKAGE, format);
                return package;
            }
            else { throw new RCPackageException("Unknown RCPackageFormat: " + formatID); }
        }

        /// <summary>
        /// Creates a control package with the given format that can be used in a network environment.
        /// </summary>
        /// <returns>The created package.</returns>
        /// <exception cref="RCPackageException">In case of unknown package format.</exception>
        public static RCPackage CreateNetworkControlPackage(int formatID)
        {
            RCPackageFormat format = RCPackageFormat.GetPackageFormat(formatID);
            if (format != null)
            {
                RCPackage package = new RCPackage(false, RCPackageType.NETWORK_CONTROL_PACKAGE, format);
                return package;
            }
            else { throw new RCPackageException("Unknown RCPackageFormat: " + formatID); }
        }

        /// <summary>
        /// Compares to RCPackages and checks whether they are equal or not.
        /// </summary>
        /// <param name="first">The first RCPackage to compare.</param>
        /// <param name="second">The second RCPackage to compare.</param>
        /// <returns>True if the given RCPackages are equal, false otherwise.</returns>
        /// <exception cref="RCPackageException">If one of the given RCPackages is not committed.</exception>
        public static bool IsEqual(RCPackage first, RCPackage second)
        {
            if (first.IsCommitted && second.IsCommitted)
            {
                if (first.packageBuffer.Length == second.packageBuffer.Length)
                {
                    for (int i = 0; i < first.packageBuffer.Length; i++)
                    {
                        if (first.packageBuffer[i] != second.packageBuffer[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else { throw new RCPackageException("Cannot compare non-committed RCPackages!"); }
        }

        #endregion

        #region Parse methods

        /// <summary>
        /// Begins to parse the given source buffer and starts to build up an RCPackage from it.
        /// </summary>
        /// <param name="sourceBuffer">The buffer we want to build from.</param>
        /// <param name="offset">The offset inside the source buffer where the parse should be started.</param>
        /// <param name="count">The maximum number of bytes that should be parsed.</param>
        /// <param name="parsedBytes">The actual number of parsed bytes after the call returned.</param>
        /// <returns>
        /// An RCPackage if there was no syntax error during the parse. In case of syntax error, a null reference
        /// is returned and parsedBytes == count.
        /// If an RCPackage is returned successfully, then you have to check whether the RCPackage has been built up
        /// successfully. You can do this by checking the RCPackage.IsCommitted property. If you get false, that means
        /// that the RCPackage has not yet been built up completely and need more bytes to parse. To do this, you can
        /// call the RCPackage.ContinueParse with new bytes on the returned instance until the RCPackage.IsCommitted
        /// property becomes true.
        /// </returns>
        public static RCPackage Parse(byte[] sourceBuffer, int offset, int count, out int parsedBytes)
        {
            RCPackage package = new RCPackage(true, RCPackageType.UNDEFINED, null);
            bool syntaxOK = package.ContinueParse(sourceBuffer, offset, count, out parsedBytes);
            if (syntaxOK) { return package; }
            else { return null; }
        }

        /// <summary>
        /// Asks this RCPackage to continue parsing the given source buffer and build up itself from it.
        /// </summary>
        /// <param name="sourceBuffer">The buffer we want to build from.</param>
        /// <param name="offset">The offset inside the source buffer where the parse should be started.</param>
        /// <param name="count">The maximum number of bytes that should be parsed.</param>
        /// <param name="parsedBytes">The actual number of parsed bytes after the call returned.</param>
        /// <returns>
        /// True if there was no syntax error during the parse. In case of syntax error, false is returned and
        /// parsedBytes == count.
        /// If true has been returned, then you have to check whether the RCPackage has been built up successfully.
        /// You can do this by checking the RCPackage.IsCommitted property. If you get false, that means that the
        /// RCPackage has not yet been built up completely and need more bytes to parse. To do this, you can call
        /// the RCPackage.ContinueParse with new bytes on the returned instance until the RCPackage.IsCommitted
        /// property becomes true.
        /// </returns>
        /// <exception cref="RCPackageException">
        /// If you call this function on an outgoing or committed RCPackage.
        /// </exception>
        public bool ContinueParse(byte[] sourceBuffer, int offset, int count, out int parsedBytes)
        {
            if (sourceBuffer == null) { throw new ArgumentNullException("targetBuffer"); }
            if (offset < 0 || offset >= sourceBuffer.Length) { throw new ArgumentOutOfRangeException("offset"); }
            if (offset + count > sourceBuffer.Length) { throw new RCPackageException("Parse error: out of bounds!"); }
            if (count < 1) { throw new ArgumentOutOfRangeException("count"); }

            if (this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (!this.parseError)
                    {
                        this.parseHelper.Reset(sourceBuffer, offset, count);
                        parsedBytes = 0;

                        while (true)
                        {
                            if (this.currentlyParsedField >= 0)
                            {
                                /// Try to parse the current field.
                                RCPackageFieldType currFieldType = this.format.GetFieldType(this.currentlyParsedField);
                                if (currFieldType == RCPackageFieldType.BYTE)
                                {
                                    /// Try to read a byte
                                    if (parseHelper.LengthFromMarker >= 1)
                                    {
                                        this.rawFields[this.currentlyParsedField] = this.parseHelper.GetBytesFromMarker(1);
                                        this.fieldsInitialized[this.currentlyParsedField] = true;
                                        parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(1);
                                        this.parseHelper.MoveMarkerForward(1);
                                        this.currentlyParsedField++;
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// No more bytes available, we should continue the parse later.
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.SHORT)
                                {
                                    /// Try to read a short
                                    if (parseHelper.LengthFromMarker >= 2)
                                    {
                                        this.rawFields[this.currentlyParsedField] = this.parseHelper.GetBytesFromMarker(2);
                                        this.shortFields[this.currentlyParsedField] = BitConverter.ToInt16(this.rawFields[this.currentlyParsedField], 0);
                                        this.fieldsInitialized[this.currentlyParsedField] = true;
                                        parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(2);
                                        this.parseHelper.MoveMarkerForward(2);
                                        this.currentlyParsedField++;
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.INT)
                                {
                                    /// Try to read an int
                                    if (parseHelper.LengthFromMarker >= 4)
                                    {
                                        this.rawFields[this.currentlyParsedField] = this.parseHelper.GetBytesFromMarker(4);
                                        this.intFields[this.currentlyParsedField] = BitConverter.ToInt32(this.rawFields[this.currentlyParsedField], 0);
                                        this.fieldsInitialized[this.currentlyParsedField] = true;
                                        parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                        this.parseHelper.MoveMarkerForward(4);
                                        this.currentlyParsedField++;
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.LONG)
                                {
                                    /// Try to read a long
                                    if (parseHelper.LengthFromMarker >= 8)
                                    {
                                        this.rawFields[this.currentlyParsedField] = this.parseHelper.GetBytesFromMarker(8);
                                        this.longFields[this.currentlyParsedField] = BitConverter.ToInt64(this.rawFields[this.currentlyParsedField], 0);
                                        this.fieldsInitialized[this.currentlyParsedField] = true;
                                        parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(8);
                                        this.parseHelper.MoveMarkerForward(8);
                                        this.currentlyParsedField++;
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.STRING)
                                {
                                    /// First try to read the length of the string
                                    if (parseHelper.LengthFromMarker >= 4)
                                    {
                                        byte[] strLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                        int strLen = BitConverter.ToInt32(strLenBytes, 0);
                                        if (parseHelper.LengthFromMarker >= 4 + strLen)
                                        {
                                            /// We can read the whole string now.
                                            int readNewBytes = this.parseHelper.GetNumOfNewBytesFromMarker(4 + strLen);
                                            this.parseHelper.MoveMarkerForward(4);
                                            byte[] strBytes = (strLen > 0) ? (this.parseHelper.GetBytesFromMarker(strLen)) : (new byte[0]);
                                            /// Write the string length and the string to the current field
                                            this.rawFields[this.currentlyParsedField] = new byte[strLenBytes.Length + strBytes.Length];
                                            for (int i = 0; i < this.rawFields[this.currentlyParsedField].Length; ++i)
                                            {
                                                if (i < strLenBytes.Length)
                                                {
                                                    this.rawFields[this.currentlyParsedField][i] = strLenBytes[i];
                                                }
                                                else
                                                {
                                                    this.rawFields[this.currentlyParsedField][i] = strBytes[i - strLenBytes.Length];
                                                }
                                            }
                                            this.parseHelper.MoveMarkerForward(strLen);
                                            try
                                            {
                                                this.stringFields[this.currentlyParsedField] =
                                                    ASCIIEncoding.UTF8.GetString(strBytes);
                                                this.fieldsInitialized[this.currentlyParsedField] = true;
                                            }
                                            catch (Exception ex)
                                            {
                                                TraceManager.WriteExceptionAllTrace(ex, false);
                                                this.parseError = true;
                                                parsedBytes = count;
                                                return false;
                                            }
                                            this.currentlyParsedField++;
                                            parsedBytes += readNewBytes;
                                            TryCommit(); if (this.committed) { return true; }
                                            continue;
                                        }
                                        else
                                        {
                                            /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                            parsedBytes = count;
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.BYTE_ARRAY)
                                {
                                    /// Read the next byte in the array or the length of the array if necessary.
                                    if (parseHelper.LengthFromMarker >= ((this.currentlyParsedArrayElement == -1) ? (4) : (1)))
                                    {
                                        if (this.currentlyParsedArrayElement == -1)
                                        {
                                            /// Write in the length bytes
                                            byte[] arrLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.currentlyParsedArrayLength = BitConverter.ToInt32(arrLenBytes, 0);
                                            this.rawFields[this.currentlyParsedField] = new byte[4 + this.currentlyParsedArrayLength];
                                            this.rawFields[this.currentlyParsedField][0] = arrLenBytes[0];
                                            this.rawFields[this.currentlyParsedField][1] = arrLenBytes[1];
                                            this.rawFields[this.currentlyParsedField][2] = arrLenBytes[2];
                                            this.rawFields[this.currentlyParsedField][3] = arrLenBytes[3];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);
                                        }
                                        else
                                        {
                                            /// Write in the next byte
                                            byte[] arrElemBytes = this.parseHelper.GetBytesFromMarker(1);
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement] = arrElemBytes[0];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(1);
                                            this.parseHelper.MoveMarkerForward(1);
                                        }

                                        this.currentlyParsedArrayElement++;
                                        if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                        {
                                            this.currentlyParsedArrayLength = 0;
                                            this.currentlyParsedArrayElement = -1;
                                            this.fieldsInitialized[this.currentlyParsedField] = true;
                                            this.currentlyParsedField++;
                                        }
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.SHORT_ARRAY)
                                {
                                    /// Read the next short in the array or the length of the array if necessary.
                                    if (parseHelper.LengthFromMarker >= ((this.currentlyParsedArrayElement == -1) ? (4) : (2)))
                                    {
                                        if (this.currentlyParsedArrayElement == -1)
                                        {
                                            /// Write in the length bytes
                                            byte[] arrLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.currentlyParsedArrayLength = BitConverter.ToInt32(arrLenBytes, 0);
                                            this.rawFields[this.currentlyParsedField] = new byte[4 + this.currentlyParsedArrayLength * 2];
                                            this.rawFields[this.currentlyParsedField][0] = arrLenBytes[0];
                                            this.rawFields[this.currentlyParsedField][1] = arrLenBytes[1];
                                            this.rawFields[this.currentlyParsedField][2] = arrLenBytes[2];
                                            this.rawFields[this.currentlyParsedField][3] = arrLenBytes[3];
                                            this.shortArrayFields[this.currentlyParsedField] = new short[this.currentlyParsedArrayLength];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);
                                        }
                                        else
                                        {
                                            /// Write in the next short
                                            byte[] arrElemBytes = this.parseHelper.GetBytesFromMarker(2);
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 2 + 0] = arrElemBytes[0];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 2 + 1] = arrElemBytes[1];
                                            this.shortArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement] = BitConverter.ToInt16(arrElemBytes, 0);
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(2);
                                            this.parseHelper.MoveMarkerForward(2);
                                        }

                                        this.currentlyParsedArrayElement++;
                                        if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                        {
                                            this.currentlyParsedArrayLength = 0;
                                            this.currentlyParsedArrayElement = -1;
                                            this.fieldsInitialized[this.currentlyParsedField] = true;
                                            this.currentlyParsedField++;
                                        }
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.INT_ARRAY)
                                {
                                    /// Read the next int in the array or the length of the array if necessary.
                                    if (parseHelper.LengthFromMarker >= ((this.currentlyParsedArrayElement == -1) ? (4) : (4)))
                                    {
                                        if (this.currentlyParsedArrayElement == -1)
                                        {
                                            /// Write in the length bytes
                                            byte[] arrLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.currentlyParsedArrayLength = BitConverter.ToInt32(arrLenBytes, 0);
                                            this.rawFields[this.currentlyParsedField] = new byte[4 + this.currentlyParsedArrayLength * 4];
                                            this.rawFields[this.currentlyParsedField][0] = arrLenBytes[0];
                                            this.rawFields[this.currentlyParsedField][1] = arrLenBytes[1];
                                            this.rawFields[this.currentlyParsedField][2] = arrLenBytes[2];
                                            this.rawFields[this.currentlyParsedField][3] = arrLenBytes[3];
                                            this.intArrayFields[this.currentlyParsedField] = new int[this.currentlyParsedArrayLength];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);
                                        }
                                        else
                                        {
                                            /// Write in the next int
                                            byte[] arrElemBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 4 + 0] = arrElemBytes[0];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 4 + 1] = arrElemBytes[1];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 4 + 2] = arrElemBytes[2];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 4 + 3] = arrElemBytes[3];
                                            this.intArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement] = BitConverter.ToInt32(arrElemBytes, 0);
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);
                                        }

                                        this.currentlyParsedArrayElement++;
                                        if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                        {
                                            this.currentlyParsedArrayLength = 0;
                                            this.currentlyParsedArrayElement = -1;
                                            this.fieldsInitialized[this.currentlyParsedField] = true;
                                            this.currentlyParsedField++;
                                        }
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.LONG_ARRAY)
                                {
                                    /// Read the next long in the array or the length of the array if necessary.
                                    if (parseHelper.LengthFromMarker >= ((this.currentlyParsedArrayElement == -1) ? (4) : (8)))
                                    {
                                        if (this.currentlyParsedArrayElement == -1)
                                        {
                                            /// Write in the length bytes
                                            byte[] arrLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.currentlyParsedArrayLength = BitConverter.ToInt32(arrLenBytes, 0);
                                            this.rawFields[this.currentlyParsedField] = new byte[4 + this.currentlyParsedArrayLength * 8];
                                            this.rawFields[this.currentlyParsedField][0] = arrLenBytes[0];
                                            this.rawFields[this.currentlyParsedField][1] = arrLenBytes[1];
                                            this.rawFields[this.currentlyParsedField][2] = arrLenBytes[2];
                                            this.rawFields[this.currentlyParsedField][3] = arrLenBytes[3];
                                            this.longArrayFields[this.currentlyParsedField] = new long[this.currentlyParsedArrayLength];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);
                                        }
                                        else
                                        {
                                            /// Write in the next long
                                            byte[] arrElemBytes = this.parseHelper.GetBytesFromMarker(8);
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 0] = arrElemBytes[0];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 1] = arrElemBytes[1];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 2] = arrElemBytes[2];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 3] = arrElemBytes[3];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 4] = arrElemBytes[4];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 5] = arrElemBytes[5];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 6] = arrElemBytes[6];
                                            this.rawFields[this.currentlyParsedField][4 + this.currentlyParsedArrayElement * 8 + 7] = arrElemBytes[7];
                                            this.longArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement] = BitConverter.ToInt32(arrElemBytes, 0);
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(8);
                                            this.parseHelper.MoveMarkerForward(8);
                                        }

                                        this.currentlyParsedArrayElement++;
                                        if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                        {
                                            this.currentlyParsedArrayLength = 0;
                                            this.currentlyParsedArrayElement = -1;
                                            this.fieldsInitialized[this.currentlyParsedField] = true;
                                            this.currentlyParsedField++;
                                        }
                                        TryCommit(); if (this.committed) { return true; }
                                        continue;
                                    }
                                    else
                                    {
                                        /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                        parsedBytes = count;
                                        return true;
                                    }
                                }
                                else if (currFieldType == RCPackageFieldType.STRING_ARRAY)
                                {
                                    /// Read the next string in the array or the length of the array if necessary.
                                    if (this.currentlyParsedArrayElement == -1)
                                    {
                                        /// Read the length of the array
                                        if (parseHelper.LengthFromMarker >= 4)
                                        {
                                            byte[] arrLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            this.currentlyParsedArrayLength = BitConverter.ToInt32(arrLenBytes, 0);
                                            this.rawStringArrayFields[this.currentlyParsedField] = new byte[this.currentlyParsedArrayLength][];
                                            this.stringArrayFields[this.currentlyParsedField] = new string[this.currentlyParsedArrayLength];
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(4);
                                            this.parseHelper.MoveMarkerForward(4);

                                            this.currentlyParsedArrayElement++;
                                            if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                            {
                                                this.currentlyParsedArrayLength = 0;
                                                this.currentlyParsedArrayElement = -1;
                                                this.fieldsInitialized[this.currentlyParsedField] = true;
                                                this.currentlyParsedField++;
                                            }
                                            TryCommit(); if (this.committed) { return true; }
                                            continue;
                                        }
                                        else
                                        {
                                            /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                            parsedBytes = count;
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        /// Read the next string in the array
                                        /// First try to read the length of the string
                                        if (parseHelper.LengthFromMarker >= 4)
                                        {
                                            byte[] strLenBytes = this.parseHelper.GetBytesFromMarker(4);
                                            int strLen = BitConverter.ToInt32(strLenBytes, 0);
                                            if (parseHelper.LengthFromMarker >= 4 + strLen)
                                            {
                                                /// We can read the whole string now.
                                                int readNewBytes = this.parseHelper.GetNumOfNewBytesFromMarker(4 + strLen);
                                                this.parseHelper.MoveMarkerForward(4);
                                                byte[] strBytes = (strLen > 0) ? (this.parseHelper.GetBytesFromMarker(strLen)) : (new byte[0]);
                                                /// Write the string length and the string to the current field
                                                this.rawStringArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement] = new byte[strLenBytes.Length + strBytes.Length];
                                                for (int i = 0; i < this.rawStringArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement].Length; ++i)
                                                {
                                                    if (i < strLenBytes.Length)
                                                    {
                                                        this.rawStringArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement][i] = strLenBytes[i];
                                                    }
                                                    else
                                                    {
                                                        this.rawStringArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement][i] = strBytes[i - strLenBytes.Length];
                                                    }
                                                }
                                                this.parseHelper.MoveMarkerForward(strLen);
                                                try
                                                {
                                                    this.stringArrayFields[this.currentlyParsedField][this.currentlyParsedArrayElement] =
                                                        ASCIIEncoding.UTF8.GetString(strBytes);
                                                }
                                                catch (Exception ex)
                                                {
                                                    TraceManager.WriteExceptionAllTrace(ex, false);
                                                    this.parseError = true;
                                                    parsedBytes = count;
                                                    return false;
                                                }
                                                parsedBytes += readNewBytes;

                                                this.currentlyParsedArrayElement++;
                                                if (this.currentlyParsedArrayLength == this.currentlyParsedArrayElement)
                                                {
                                                    this.currentlyParsedArrayLength = 0;
                                                    this.currentlyParsedArrayElement = -1;
                                                    this.fieldsInitialized[this.currentlyParsedField] = true;
                                                    this.currentlyParsedField++;
                                                }
                                                TryCommit(); if (this.committed) { return true; }
                                                continue;
                                            }
                                            else
                                            {
                                                /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                                parsedBytes = count;
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                            parsedBytes = count;
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    /// Unknown field type --> parse error
                                    this.parseError = true;
                                    parsedBytes = count;
                                    return false;
                                }
                            }
                            else if (this.currentlyParsedField == -1)
                            {
                                /// The format indicator has not yet been parsed, so we try to parse it.
                                if (this.parseHelper.LengthFromMarker >= ((this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (3) : (2)))
                                {
                                    /// Eat the format indicator and the sender byte if this is a NETWORK_CUSTOM_PACKAGE
                                    byte[] formatAndSenderBytes = this.parseHelper.GetBytesFromMarker((this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (3) : (2));
                                    int formatID = BitConverter.ToUInt16(formatAndSenderBytes, 0);
                                    if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) { this.senderTmp = formatAndSenderBytes[2]; }
                                    this.format = RCPackageFormat.GetPackageFormat(formatID);
                                    if (this.format != null)
                                    {
                                        /// Create the arrays for the incoming fields.
                                        this.rawFields = new byte[this.format.NumOfFields][];
                                        this.rawStringArrayFields = new byte[this.format.NumOfFields][][];
                                        this.shortFields = new short[this.format.NumOfFields];
                                        this.shortArrayFields = new short[this.format.NumOfFields][];
                                        this.intFields = new int[this.format.NumOfFields];
                                        this.intArrayFields = new int[this.format.NumOfFields][];
                                        this.longFields = new long[this.format.NumOfFields];
                                        this.longArrayFields = new long[this.format.NumOfFields][];
                                        this.stringFields = new string[this.format.NumOfFields];
                                        this.stringArrayFields = new string[this.format.NumOfFields][];
                                        this.fieldsInitialized = new bool[this.format.NumOfFields];
                                        for (int i = 0; i < this.fieldsInitialized.Length; ++i) { this.fieldsInitialized[i] = false; }
                                        parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker((this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (3) : (2));
                                        this.parseHelper.MoveMarkerForward((this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (3) : (2));
                                        this.currentlyParsedField = 0; /// Try to parse the first field in the next loop
                                        this.currentlyParsedArrayLength = 0;
                                        this.currentlyParsedArrayElement = -1;
                                        continue;
                                    }
                                    else
                                    {
                                        /// Unknown format ID --> parse error
                                        this.parseError = true;
                                        parsedBytes = count;
                                        return false;
                                    }
                                }
                                else  /// (this.parseHelper.LengthFromMarker >= 1)
                                {
                                    /// No more bytes available, we should continue the parse later.
                                    parsedBytes = count;
                                    return true;
                                }
                            }
                            else if (this.currentlyParsedField == -2)
                            {
                                /// The header has not yet been parsed, so we try to parse the header first.
                                if (this.parseHelper.LengthFromMarker >= 3)
                                {
                                    byte[] headerBytes = this.parseHelper.GetBytesFromMarker(3);
                                    if (headerBytes[0] == MAGIC_NUMBER[0] && headerBytes[1] == MAGIC_NUMBER[1])
                                    {
                                        if (headerBytes[2] == (byte)RCPackageType.NETWORK_PING_PACKAGE)
                                        {
                                            /// This is a ping package, end the parse.
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(3);
                                            this.type = RCPackageType.NETWORK_PING_PACKAGE;
                                            this.packageBuffer = headerBytes;
                                            this.committed = true;
                                            return true;
                                        }
                                        else if (headerBytes[2] == (byte)RCPackageType.CUSTOM_DATA_PACKAGE)
                                        {
                                            /// This is a custom data package, continue parsing
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(3);
                                            this.parseHelper.MoveMarkerForward(3);
                                            this.type = RCPackageType.CUSTOM_DATA_PACKAGE;
                                            this.currentlyParsedField = -1; /// Try to parse the format in the next loop
                                            continue;
                                        }
                                        else if (headerBytes[2] == (byte)RCPackageType.NETWORK_CONTROL_PACKAGE)
                                        {
                                            /// This is a control package, continue parsing
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(3);
                                            this.parseHelper.MoveMarkerForward(3);
                                            this.type = RCPackageType.NETWORK_CONTROL_PACKAGE;
                                            this.currentlyParsedField = -1; /// Try to parse the format in the next loop
                                            continue;
                                        }
                                        else if (headerBytes[2] == (byte)RCPackageType.NETWORK_CUSTOM_PACKAGE)
                                        {
                                            /// This is a custom network package, continue parsing
                                            parsedBytes += this.parseHelper.GetNumOfNewBytesFromMarker(3);
                                            this.parseHelper.MoveMarkerForward(3);
                                            this.type = RCPackageType.NETWORK_CUSTOM_PACKAGE;
                                            this.currentlyParsedField = -1; /// Try to parse the format and sender in the next loop
                                            continue;
                                        }
                                        else
                                        {
                                            /// Package type mismatch --> parse error
                                            this.parseError = true;
                                            parsedBytes = count;
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        /// Magic number mismatch --> parse error
                                        this.parseError = true;
                                        parsedBytes = count;
                                        return false;
                                    }
                                }
                                else /// (this.parseHelper.LengthFromMarker >= 3)
                                {
                                    /// Indicate that we have buffered the bytes, but we need more bytes to do anything else.
                                    parsedBytes = count;
                                    return true;
                                }
                            }
                            else
                            {
                                /// Inpossible situation --> parse error
                                this.parseError = true;
                                parsedBytes = count;
                                return false;
                            }
                        } /// end-while
                    }
                    else
                    {
                        /// In case of earlier parse error we simply ignore these new bytes.
                        parsedBytes = count;
                        return false;
                    }
                }
                else { throw new RCPackageException("Parsing a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Parsing an outgoing RCPackage is not possible!"); }
        }

        #endregion

        #region Read methods

        /// <summary>
        /// Reads a byte from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public byte ReadByte(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.BYTE)
                {
                    return this.rawFields[fieldIndex][0];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public short ReadShort(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.SHORT)
                {
                    return this.shortFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public int ReadInt(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.INT)
                {
                    return this.intFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a 64-bit signed integer from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public long ReadLong(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.LONG)
                {
                    return this.longFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads an UTF-8 string from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public string ReadString(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.STRING)
                {
                    return this.stringFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        #endregion

        #region Read array methods

        /// <summary>
        /// Reads a byte array from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public byte[] ReadByteArray(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.BYTE_ARRAY)
                {
                    int arrayLength = BitConverter.ToInt32(this.rawFields[fieldIndex], 0);
                    byte[] retArray = new byte[arrayLength];
                    int i = 0;
                    for (; i < arrayLength; i++)
                    {
                        retArray[i] = this.rawFields[fieldIndex][4 + i];
                    }
                    if (4 + i != this.rawFields[fieldIndex].Length)
                    {
                        throw new RCPackageException("Unexpected length of rawFields[" + fieldIndex + "]!");
                    }
                    return retArray;
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a short array from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public short[] ReadShortArray(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.SHORT_ARRAY)
                {
                    return this.shortArrayFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads an integer array from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public int[] ReadIntArray(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.INT_ARRAY)
                {
                    return this.intArrayFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a long array from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public long[] ReadLongArray(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.LONG_ARRAY)
                {
                    return this.longArrayFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Reads a string array from the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to read from.</param>
        /// <returns>The result data in case of success.</returns>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to read from an uncommitted RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public string[] ReadStringArray(int fieldIndex)
        {
            if (this.committed)
            {
                if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.STRING_ARRAY)
                {
                    return this.stringArrayFields[fieldIndex];
                }
                else { throw new RCPackageException("Type mismatch when reading RCPackage!"); }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        #endregion

        #region Write methods

        /// <summary>
        /// Writes a byte to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteByte(int fieldIndex, byte data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.BYTE)
                    {
                        this.rawFields[fieldIndex] = new byte[1];
                        this.rawFields[fieldIndex][0] = data;
                        this.fieldsInitialized[fieldIndex] = true;
                        TryCommit();
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteShort(int fieldIndex, short data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.SHORT)
                    {
                        this.rawFields[fieldIndex] = BitConverter.GetBytes(data);
                        this.shortFields[fieldIndex] = data;
                        this.fieldsInitialized[fieldIndex] = true;
                        TryCommit();
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteInt(int fieldIndex, int data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.INT)
                    {
                        this.rawFields[fieldIndex] = BitConverter.GetBytes(data);
                        this.intFields[fieldIndex] = data;
                        this.fieldsInitialized[fieldIndex] = true;
                        TryCommit();
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// </exception>
        public void WriteLong(int fieldIndex, long data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.LONG)
                    {
                        this.rawFields[fieldIndex] = BitConverter.GetBytes(data);
                        this.longFields[fieldIndex] = data;
                        this.fieldsInitialized[fieldIndex] = true;
                        TryCommit();
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes an UTF-8 string to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the UTF-8 encoded bytes of the string is greater than int.MaxValue.
        /// </exception>
        public void WriteString(int fieldIndex, string data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }

                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.STRING)
                    {
                        byte[] strBytes = System.Text.ASCIIEncoding.UTF8.GetBytes(data);
                        if (strBytes.Length >= 0 && strBytes.Length <= int.MaxValue)
                        {
                            byte[] strLengthBytes = BitConverter.GetBytes(strBytes.Length);
                            this.rawFields[fieldIndex] = new byte[strLengthBytes.Length + strBytes.Length];
                            for (int i = 0; i < this.rawFields[fieldIndex].Length; ++i)
                            {
                                if (i < strLengthBytes.Length)
                                {
                                    this.rawFields[fieldIndex][i] = strLengthBytes[i];
                                }
                                else
                                {
                                    this.rawFields[fieldIndex][i] = strBytes[i - strLengthBytes.Length];
                                }
                            }
                            this.stringFields[fieldIndex] = data;
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("String is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        #endregion

        #region Write array methods

        /// <summary>
        /// Writes a byte array to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the byte array is greater than int.MaxValue.
        /// </exception>
        public void WriteByteArray(int fieldIndex, byte[] data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.BYTE_ARRAY)
                    {
                        if (data.Length >= 0 && data.Length <= int.MaxValue)
                        {
                            byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
                            this.rawFields[fieldIndex] = new byte[dataLengthBytes.Length + data.Length];
                            for (int i = 0; i < this.rawFields[fieldIndex].Length; ++i)
                            {
                                if (i < dataLengthBytes.Length)
                                {
                                    this.rawFields[fieldIndex][i] = dataLengthBytes[i];
                                }
                                else
                                {
                                    this.rawFields[fieldIndex][i] = data[i - dataLengthBytes.Length];
                                }
                            }
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("Array is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a short array to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the short array is greater than int.MaxValue.
        /// </exception>
        public void WriteShortArray(int fieldIndex, short[] data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.SHORT_ARRAY)
                    {
                        if (data.Length >= 0 && data.Length <= int.MaxValue)
                        {
                            byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
                            this.rawFields[fieldIndex] = new byte[dataLengthBytes.Length + data.Length * 2];
                            this.rawFields[fieldIndex][0] = dataLengthBytes[0];
                            this.rawFields[fieldIndex][1] = dataLengthBytes[1];
                            this.rawFields[fieldIndex][2] = dataLengthBytes[2];
                            this.rawFields[fieldIndex][3] = dataLengthBytes[3];
                            int currByte = 4;
                            for (int i = 0; i < data.Length; i++)
                            {
                                byte[] elementBytes = BitConverter.GetBytes(data[i]);
                                this.rawFields[fieldIndex][currByte + 0] = elementBytes[0];
                                this.rawFields[fieldIndex][currByte + 1] = elementBytes[1];
                                currByte += 2;
                            }
                            this.shortArrayFields[fieldIndex] = data;
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("Array is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes an integer array to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the integer array is greater than int.MaxValue.
        /// </exception>
        public void WriteIntArray(int fieldIndex, int[] data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.INT_ARRAY)
                    {
                        if (data.Length >= 0 && data.Length <= int.MaxValue)
                        {
                            byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
                            this.rawFields[fieldIndex] = new byte[dataLengthBytes.Length + data.Length * 4];
                            this.rawFields[fieldIndex][0] = dataLengthBytes[0];
                            this.rawFields[fieldIndex][1] = dataLengthBytes[1];
                            this.rawFields[fieldIndex][2] = dataLengthBytes[2];
                            this.rawFields[fieldIndex][3] = dataLengthBytes[3];
                            int currByte = 4;
                            for (int i = 0; i < data.Length; i++)
                            {
                                byte[] elementBytes = BitConverter.GetBytes(data[i]);
                                this.rawFields[fieldIndex][currByte + 0] = elementBytes[0];
                                this.rawFields[fieldIndex][currByte + 1] = elementBytes[1];
                                this.rawFields[fieldIndex][currByte + 2] = elementBytes[2];
                                this.rawFields[fieldIndex][currByte + 3] = elementBytes[3];
                                currByte += 4;
                            }
                            this.intArrayFields[fieldIndex] = data;
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("Array is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a long array to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the long array is greater than int.MaxValue.
        /// </exception>
        public void WriteLongArray(int fieldIndex, long[] data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.LONG_ARRAY)
                    {
                        if (data.Length >= 0 && data.Length <= int.MaxValue)
                        {
                            byte[] dataLengthBytes = BitConverter.GetBytes(data.Length);
                            this.rawFields[fieldIndex] = new byte[dataLengthBytes.Length + data.Length * 8];
                            this.rawFields[fieldIndex][0] = dataLengthBytes[0];
                            this.rawFields[fieldIndex][1] = dataLengthBytes[1];
                            this.rawFields[fieldIndex][2] = dataLengthBytes[2];
                            this.rawFields[fieldIndex][3] = dataLengthBytes[3];
                            int currByte = 4;
                            for (int i = 0; i < data.Length; i++)
                            {
                                byte[] elementBytes = BitConverter.GetBytes(data[i]);
                                this.rawFields[fieldIndex][currByte + 0] = elementBytes[0];
                                this.rawFields[fieldIndex][currByte + 1] = elementBytes[1];
                                this.rawFields[fieldIndex][currByte + 2] = elementBytes[2];
                                this.rawFields[fieldIndex][currByte + 3] = elementBytes[3];
                                this.rawFields[fieldIndex][currByte + 4] = elementBytes[4];
                                this.rawFields[fieldIndex][currByte + 5] = elementBytes[5];
                                this.rawFields[fieldIndex][currByte + 6] = elementBytes[6];
                                this.rawFields[fieldIndex][currByte + 7] = elementBytes[7];
                                currByte += 8;
                            }
                            this.longArrayFields[fieldIndex] = data;
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("Array is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        /// <summary>
        /// Writes a string array to the given field of this package.
        /// </summary>
        /// <param name="fieldIndex">The index of the field you want to write to.</param>
        /// <param name="data">The data you want to write.</param>
        /// <exception cref="NetworkingSystemExcepton">
        /// If you want to write the fields of an incoming RCPackage. Such packages should be built up from a byte
        /// sequence using the RCPackage.Parse and RCPackage.ContinueParse methods.
        /// If you want to write the fields of a committed RCPackage.
        /// In case of type mismatch or invalid field index.
        /// If the length of the string array or of any element in the array is greater than int.MaxValue.
        /// </exception>
        public void WriteStringArray(int fieldIndex, string[] data)
        {
            if (!this.incomingPackage)
            {
                if (!this.committed)
                {
                    if (data == null) { throw new ArgumentNullException("data"); }
                    if (this.format != null && this.format.GetFieldType(fieldIndex) == RCPackageFieldType.STRING_ARRAY)
                    {
                        if (data.Length >= 0 && data.Length <= int.MaxValue)
                        {
                            this.rawStringArrayFields[fieldIndex] = new byte[data.Length][];
                            for (int strIdx = 0; strIdx < data.Length; ++strIdx)
                            {
                                string currStr = data[strIdx];
                                if (currStr == null) { throw new RCPackageException("No null references allowed in string arrays!"); }

                                byte[] strBytes = System.Text.ASCIIEncoding.UTF8.GetBytes(currStr);
                                if (strBytes.Length >= 0 && strBytes.Length <= int.MaxValue)
                                {
                                    byte[] strLengthBytes = BitConverter.GetBytes(strBytes.Length);
                                    this.rawStringArrayFields[fieldIndex][strIdx] = new byte[strLengthBytes.Length + strBytes.Length];
                                    for (int i = 0; i < this.rawStringArrayFields[fieldIndex][strIdx].Length; ++i)
                                    {
                                        if (i < strLengthBytes.Length)
                                        {
                                            this.rawStringArrayFields[fieldIndex][strIdx][i] = strLengthBytes[i];
                                        }
                                        else
                                        {
                                            this.rawStringArrayFields[fieldIndex][strIdx][i] = strBytes[i - strLengthBytes.Length];
                                        }
                                    }
                                }
                                else { throw new RCPackageException("String is too long to write to an RCPackage!"); }
                            }
                            this.stringArrayFields[fieldIndex] = data;
                            this.fieldsInitialized[fieldIndex] = true;
                            TryCommit();
                        }
                        else { throw new RCPackageException("Array is too long to write to an RCPackage!"); }
                    }
                    else { throw new RCPackageException("Type mismatch when writing RCPackage!"); }
                }
                else { throw new RCPackageException("Writing the fields of a committed RCPackage is not possible!"); }
            }
            else { throw new RCPackageException("Writing the fields of an incoming RCPackage is not possible!"); }
        }

        #endregion

        #region Access methods and properties

        /// <summary>
        /// Copies the bytes of this RCPackage to the given target buffer.
        /// </summary>
        /// <param name="targetBuffer">The target buffer you want to write to.</param>
        /// <param name="offset">The offset inside the target buffer where to start the writing.</param>
        /// <returns>The number of written bytes.</returns>
        /// <exception cref="RCPackageException">
        /// If this package has not been committed.
        /// If there is not enough free space in the target buffer to copy the bytes.
        /// </exception>
        public int WritePackageToBuffer(byte[] targetBuffer, int offset)
        {
            if (targetBuffer == null) { throw new ArgumentNullException("targetBuffer"); }
            if (offset < 0 || offset >= targetBuffer.Length) { throw new ArgumentOutOfRangeException("offset"); }

            if (this.committed)
            {
                if (offset + this.packageBuffer.Length <= targetBuffer.Length)
                {
                    for (int i = 0; i < this.packageBuffer.Length; i++)
                    {
                        targetBuffer[offset + i] = this.packageBuffer[i];
                    }
                    return this.packageBuffer.Length;
                }
                else
                {
                    throw new RCPackageException("Not enough space in the target buffer to copy the bytes of this RCPackage!");
                }
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Returns the string representation of this RCPackage.
        /// </summary>
        /// <returns>The string representation of this RCPackage.</returns>
        /// <exception cref="RCPackageException">If this package has not been committed.</exception>
        /// <remarks>
        /// You can use this function for debugging.
        /// </remarks>
        public override string ToString()
        {
            if (this.committed)
            {
                string retStr = string.Empty;

                /// Writing the package type
                if (this.type == RCPackageType.NETWORK_PING_PACKAGE) { retStr += "|PING"; return retStr; }
                else if (this.type == RCPackageType.CUSTOM_DATA_PACKAGE) { retStr += "|DATA("; }
                else if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) { retStr += "|CUSTOM("; }
                else if (this.type == RCPackageType.NETWORK_CONTROL_PACKAGE) { retStr += "|CONTROL("; }
                else { throw new RCPackageException("Unknown package type!"); }

                /// Writing the format ID of format name (if given)
                retStr += this.format.Name;
                if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE)
                {
                    retStr += " from " + (int)this.packageBuffer[5] + "): ";
                }
                else
                {
                    retStr += "): ";
                }

                /// Writing the fields and their datatypes
                for (int i = 0; i < this.format.NumOfFields; i++)
                {
                    RCPackageFieldType currFieldType = this.format.GetFieldType(i);
                    if (currFieldType == RCPackageFieldType.BYTE) { retStr += "BYTE(" + this.rawFields[i][0] + "); "; }
                    else if (currFieldType == RCPackageFieldType.SHORT) { retStr += "SHORT(" + this.shortFields[i] + "); "; }
                    else if (currFieldType == RCPackageFieldType.INT) { retStr += "INT(" + this.intFields[i] + "); "; }
                    else if (currFieldType == RCPackageFieldType.LONG) { retStr += "LONG(" + this.longFields[i] + "); "; }
                    else if (currFieldType == RCPackageFieldType.STRING) { retStr += "STRING(" + this.stringFields[i] + "); "; }
                    else if (currFieldType == RCPackageFieldType.BYTE_ARRAY)
                    {
                        retStr += "BYTE_ARRAY(";
                        for (int j = 4; j < this.rawFields[i].Length; ++j)
                        {
                            retStr += this.rawFields[i][j];
                            if (j < this.rawFields[i].Length - 1) { retStr += ","; }
                        }
                        retStr += "); ";
                    }
                    else if (currFieldType == RCPackageFieldType.SHORT_ARRAY)
                    {
                        retStr += "SHORT_ARRAY(";
                        for (int j = 0; j < this.shortArrayFields[i].Length; ++j)
                        {
                            retStr += this.shortArrayFields[i][j];
                            if (j < this.shortArrayFields[i].Length - 1) { retStr += ","; }
                        }
                        retStr += "); ";
                    }
                    else if (currFieldType == RCPackageFieldType.INT_ARRAY)
                    {
                        retStr += "INT_ARRAY(";
                        for (int j = 0; j < this.intArrayFields[i].Length; ++j)
                        {
                            retStr += this.intArrayFields[i][j];
                            if (j < this.intArrayFields[i].Length - 1) { retStr += ","; }
                        }
                        retStr += "); ";
                    }
                    else if (currFieldType == RCPackageFieldType.LONG_ARRAY)
                    {
                        retStr += "LONG_ARRAY(";
                        for (int j = 0; j < this.longArrayFields[i].Length; ++j)
                        {
                            retStr += this.longArrayFields[i][j];
                            if (j < this.longArrayFields[i].Length - 1) { retStr += ","; }
                        }
                        retStr += "); ";
                    }
                    else if (currFieldType == RCPackageFieldType.STRING_ARRAY)
                    {
                        retStr += "STRING_ARRAY(";
                        for (int j = 0; j < this.stringArrayFields[i].Length; ++j)
                        {
                            retStr += this.stringArrayFields[i][j];
                            if (j < this.stringArrayFields[i].Length - 1) { retStr += ","; }
                        }
                        retStr += "); ";
                    }
                    else { throw new RCPackageException("Unknown datatype!"); }
                }

                retStr += "|";
                return retStr;
            }
            else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
        }

        /// <summary>
        /// Gets the type of this package.
        /// </summary>
        /// <exception cref="RCPackageException">
        /// If you want to query the type of an uncommitted RCPackage.
        /// </exception>
        public RCPackageType PackageType
        {
            get
            {
                if (this.committed) { return this.type; }
                else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
            }
        }

        /// <summary>
        /// Gets the package format of this package.
        /// </summary>
        /// <exception cref="RCPackageException">
        /// If you want to query the format of an uncommitted RCPackage.
        /// </exception>
        public RCPackageFormat PackageFormat
        {
            get
            {
                if (this.committed) { return this.format; }
                else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
            }
        }

        /// <summary>
        /// Gets the length of this package in bytes.
        /// </summary>
        /// <exception cref="RCPackageException">
        /// If you want to query the length of an uncommitted RCPackage.
        /// </exception>
        public int PackageLength
        {
            get
            {
                if (this.committed) { return this.packageBuffer.Length; }
                else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
            }
        }

        /// <summary>
        /// Gets or sets the sender property of this package if it has one.
        /// </summary>
        /// <exception cref="RCPackageException">
        /// If you want to get or set the sender of an uncommitted RCPackage.
        /// If the RCPackageType is not NETWORK_CUSTOM_PACKAGE.
        /// </exception>
        public int Sender
        {
            get
            {
                if (this.committed)
                {
                    if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE)
                    {
                        return this.packageBuffer[5];
                    }
                    else { throw new RCPackageException("The type of the RCPackage is not NETWORK_CUSTOM_PACKAGE!"); }
                }
                else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
            }
            set
            {
                if (this.committed)
                {
                    if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE)
                    {
                        if (value >= 0 && value <= byte.MaxValue)
                        {
                            this.packageBuffer[5] = (byte)value;
                        }
                        else { throw new ArgumentOutOfRangeException("value"); }
                    }
                    else { throw new RCPackageException("The type of the RCPackage is not NETWORK_CUSTOM_PACKAGE!"); }
                }
                else { throw new RCPackageException("Using of uncommitted RCPackage is not possible!"); }
            }
        }

        /// <summary>
        /// Gets whether this RCPackage is ready to read and/or send over the wire.
        /// </summary>
        /// <remarks>
        /// An RCPackage is automatically committed as soon as it's every field has been initialized (in case of outgoing
        /// packages) or as soon as it has been successfully built up from a byte sequence (in case of incoming packages).
        /// </remarks>
        public bool IsCommitted { get { return this.committed; } }

        #endregion

        #region ParseHelper

        /// <summary>
        /// Helper class used in continuous parse.
        /// </summary>
        /// <remarks>WARNING!!! This class doesn't provide any error handling. Only for internal use!!!</remarks>
        private class ParseHelper
        {
            /// <summary>
            /// Constructs a ParseHelper class.
            /// </summary>
            /// <param name="remainingBytes">The bytes remained from the previous step of parse.</param>
            /// <param name="newBuffer">The buffer that contains the new bytes.</param>
            /// <param name="offset">The offset inside the new buffer to read from.</param>
            /// <param name="length">The number of bytes we can read from the new buffer.</param>
            public ParseHelper()
            {
                this.remainingBytes = null;
                this.newBuffer = null;
                this.offset = -1;
                this.length = -1;
                this.marker = -1;
            }

            /// Resets the state of this helper.
            public void Reset(byte[] newBuffer, int offset, int length)
            {
                if (-1 == marker)
                {
                    /// First reset
                    this.remainingBytes = null;
                    this.newBuffer = newBuffer;
                    this.offset = offset;
                    this.length = length;
                    this.marker = 0;
                }
                else
                {
                    /// Non-first reset
                    this.remainingBytes = GetBytesFromMarker(LengthFromMarker);
                    this.newBuffer = newBuffer;
                    this.offset = offset;
                    this.length = length;
                    this.marker = 0;
                }
            }

            /// Forwards the marker with the given amount of bytes.
            public void MoveMarkerForward(int count) { this.marker += count; }

            /// Gets the given count of bytes from the marker.
            public byte[] GetBytesFromMarker(int count)
            {
                int remainigBytesLength = (this.remainingBytes != null) ? (this.remainingBytes.Length) : (0);
                if (this.marker >= 0 && this.marker < remainigBytesLength)
                {
                    /// Marker is inside this.remainingBytes
                    byte[] retArray = new byte[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (this.marker + i < remainigBytesLength)
                        {
                            /// Get the next byte from the this.remainingBytes
                            retArray[i] = this.remainingBytes[this.marker + i];
                        }
                        else
                        {
                            if (this.offset - remainigBytesLength + this.marker + i < this.newBuffer.Length)
                            {
                                /// Get the next byte from the this.newBuffer
                                retArray[i] = this.newBuffer[this.offset - remainigBytesLength + this.marker + i];
                            }
                            else
                            {
                                /// Out of bounds.
                                return null;
                            }
                        }
                    }
                    return retArray;
                }
                else if (this.marker >= remainigBytesLength && this.marker < remainigBytesLength + this.length)
                {
                    /// Marker is inside this.newBuffer
                    byte[] retArray = new byte[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (this.offset - remainigBytesLength + this.marker + i < this.newBuffer.Length)
                        {
                            /// Get the next byte from the this.newBuffer
                            retArray[i] = this.newBuffer[this.offset - remainigBytesLength + this.marker + i];
                        }
                        else
                        {
                            /// Out of bounds.
                            return null;
                        }
                    }
                    return retArray;
                }
                else
                {
                    /// Marker is out of bounds.
                    return null;
                }
            }

            /// Returns the number of new bytes in the 'count'-long sequence from the marker.
            public int GetNumOfNewBytesFromMarker(int count)
            {
                int remainigBytesLength = (this.remainingBytes != null) ? (this.remainingBytes.Length) : (0);
                if (this.marker >= 0 && this.marker < remainigBytesLength)
                {
                    /// Marker is inside this.remainingBytes
                    if (count < this.remainingBytes.Length - this.marker)
                    {
                        return 0;
                    }
                    else
                    {
                        return count - this.remainingBytes.Length + this.marker;
                    }
                }
                else if (this.marker >= remainigBytesLength && this.marker < remainigBytesLength + this.length)
                {
                    /// Marker is inside this.newBuffer
                    return count;
                }
                else
                {
                    /// Marker is out of bounds.
                    return 0;
                }
            }

            /// Query remaining bytes from the marker.
            public int LengthFromMarker
            {
                get
                {
                    return (this.remainingBytes != null) ? (this.remainingBytes.Length + this.length - this.marker)
                                                         : (this.length - this.marker);
                }
            }

            /// Helper class data fields
            private byte[] remainingBytes;
            private byte[] newBuffer;
            private int offset;
            private int length;
            private int marker;
        }

        #endregion ParseHelper

        #region Private fields

        /// <summary>
        /// Constructs an empty RCPackage. For internal use only!
        /// </summary>
        private RCPackage(bool incomingPackage, RCPackageType type, RCPackageFormat format)
        {
            if (!incomingPackage)
            {
                if (type == RCPackageType.NETWORK_PING_PACKAGE)
                {
                    this.type = type;
                    this.format = null;
                    this.packageBuffer = new byte[3];
                    this.packageBuffer[0] = MAGIC_NUMBER[0];
                    this.packageBuffer[1] = MAGIC_NUMBER[1];
                    this.packageBuffer[2] = (byte)RCPackageType.NETWORK_PING_PACKAGE;
                    this.committed = true;
                    this.incomingPackage = incomingPackage;
                    this.rawFields = null;
                    this.rawStringArrayFields = null;
                    this.shortFields = null;
                    this.shortArrayFields = null;
                    this.intFields = null;
                    this.intArrayFields = null;
                    this.longFields = null;
                    this.longArrayFields = null;
                    this.stringFields = null;
                    this.stringArrayFields = null;
                    this.fieldsInitialized = null;
                    this.senderTmp = byte.MaxValue;
                    this.parseError = false;
                    this.currentlyParsedField = -2;
                    this.currentlyParsedArrayElement = -1;
                    this.currentlyParsedArrayLength = 0;
                    this.parseHelper = null;
                }
                else
                {
                    this.type = type;
                    this.format = format;
                    this.packageBuffer = null;
                    this.committed = false;
                    this.incomingPackage = incomingPackage;
                    this.rawFields = new byte[this.format.NumOfFields][];
                    this.rawStringArrayFields = new byte[this.format.NumOfFields][][];
                    this.shortFields = new short[this.format.NumOfFields];
                    this.shortArrayFields = new short[this.format.NumOfFields][];
                    this.intFields = new int[this.format.NumOfFields];
                    this.intArrayFields = new int[this.format.NumOfFields][];
                    this.longFields = new long[this.format.NumOfFields];
                    this.longArrayFields = new long[this.format.NumOfFields][];
                    this.stringFields = new string[this.format.NumOfFields];
                    this.stringArrayFields = new string[this.format.NumOfFields][];
                    this.fieldsInitialized = new bool[this.format.NumOfFields];
                    for (int i = 0; i < this.fieldsInitialized.Length; ++i) { this.fieldsInitialized[i] = false; }
                    this.senderTmp = byte.MaxValue;
                    this.parseError = false;
                    this.currentlyParsedField = -2;
                    this.currentlyParsedArrayElement = -1;
                    this.currentlyParsedArrayLength = 0;
                    this.parseHelper = null;
                }
            }
            else
            {
                /// Incoming package: the members will be initialized when parsing the incoming byte sequence.
                this.type = RCPackageType.UNDEFINED;
                this.format = null;
                this.packageBuffer = null;
                this.committed = false;
                this.incomingPackage = incomingPackage;
                this.rawFields = null;
                this.rawStringArrayFields = null;
                this.shortFields = null;
                this.shortArrayFields = null;
                this.intFields = null;
                this.intArrayFields = null;
                this.longFields = null;
                this.longArrayFields = null;
                this.stringFields = null;
                this.stringArrayFields = null;
                this.fieldsInitialized = null;
                this.senderTmp = byte.MaxValue;
                this.parseError = false;
                this.currentlyParsedField = -2;
                this.currentlyParsedArrayElement = -1;
                this.currentlyParsedArrayLength = 0;
                this.parseHelper = new ParseHelper();
            }
        }

        /// <summary>
        /// Checks whether every field of this package has been initialized and commit this package if yes.
        /// </summary>
        private void TryCommit()
        {
            if (this.type != RCPackageType.UNDEFINED)
            {
                for (int i = 0; i < this.fieldsInitialized.Length; ++i)
                {
                    if (!this.fieldsInitialized[i])
                    {
                        /// A field is not initialized --> don't continue
                        return;
                    }
                }

                /// Every field initialized so we can start to create and fill the package buffer.
                /// First we compute the length of the package buffer.
                /// There is an additional sender byte at position 5 in case of NETWORK_CUSTOM_PACKAGEs
                int bufferLength = (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (6) : (5);
                for (int i = 0; i < this.format.NumOfFields; ++i)
                {
                    RCPackageFieldType fieldType = this.format.GetFieldType(i);
                    if (fieldType != RCPackageFieldType.STRING_ARRAY)
                    {
                        /// Non string array
                        bufferLength += this.rawFields[i].Length;
                    }
                    else
                    {
                        /// String array
                        bufferLength += 4; /// The bytes that describe the length of the string array
                        for (int j = 0; j < this.rawStringArrayFields[i].Length; j++)
                        {
                            bufferLength += this.rawStringArrayFields[i][j].Length;
                        }
                    }
                }
                /// Create and fill the package buffer
                this.packageBuffer = new byte[bufferLength];
                this.packageBuffer[0] = MAGIC_NUMBER[0];
                this.packageBuffer[1] = MAGIC_NUMBER[1];
                this.packageBuffer[2] = (byte)this.type;
                byte[] formatIDBytes = BitConverter.GetBytes((ushort)this.format.ID);
                this.packageBuffer[3] = formatIDBytes[0];
                this.packageBuffer[4] = formatIDBytes[1];
                /// There is an additional sender byte at position 5 in case of NETWORK_CUSTOM_PACKAGEs
                if (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) { this.packageBuffer[5] = this.senderTmp; }
                int currByte = (this.type == RCPackageType.NETWORK_CUSTOM_PACKAGE) ? (6) : (5);
                for (int i = 0; i < this.format.NumOfFields; ++i)
                {
                    RCPackageFieldType fieldType = this.format.GetFieldType(i);
                    if (fieldType != RCPackageFieldType.STRING_ARRAY)
                    {
                        /// Non string array
                        /// Writing the bytes of the field
                        for (int j = 0; j < this.rawFields[i].Length; ++j)
                        {
                            this.packageBuffer[currByte] = this.rawFields[i][j];
                            currByte++;
                        }
                    }
                    else
                    {
                        /// String array
                        /// Writing the array length indicator bytes
                        byte[] strArrayLenBytes = BitConverter.GetBytes(this.rawStringArrayFields[i].Length);
                        this.packageBuffer[currByte + 0] = strArrayLenBytes[0];
                        this.packageBuffer[currByte + 1] = strArrayLenBytes[1];
                        this.packageBuffer[currByte + 2] = strArrayLenBytes[2];
                        this.packageBuffer[currByte + 3] = strArrayLenBytes[3];
                        currByte += 4;
                        /// Writing the bytes of the strings
                        for (int j = 0; j < this.rawStringArrayFields[i].Length; j++)
                        {
                            for (int k = 0; k < this.rawStringArrayFields[i][j].Length; k++)
                            {
                                this.packageBuffer[currByte] = this.rawStringArrayFields[i][j][k];
                                currByte++;
                            }
                        }
                    }
                }
                this.committed = true;
            }
        }

        /// <summary>
        /// Type of this package.
        /// </summary>
        private RCPackageType type;

        /// <summary>
        /// The format definition of this package.
        /// </summary>
        private RCPackageFormat format;

        /// <summary>
        /// The byte sequence that represents this package when it has been serialized.
        /// </summary>
        private byte[] packageBuffer;

        /// <summary>
        /// This flag is true if the package has been committed. You can only read informations from committed packages.
        /// </summary>
        private bool committed;

        /// <summary>
        /// This flag is true if this is an incoming RCPackage that should be built up from a byte sequence.
        /// Otherwise this is an outgoing RCPackage and it's fields have to be initialized using the appropriate
        /// RCPackage.WriteXXX methods.
        /// </summary>
        private bool incomingPackage;

        /// <summary>
        /// This array contains the raw byte sequences of the fields in this package.
        /// </summary>
        private byte[][] rawFields;

        /// <summary>
        /// This array contains the raw byte sequences of the string array fields in this package.
        /// </summary>
        private byte[][][] rawStringArrayFields;

        /// <summary>
        /// An array that contains the short fields in this package.
        /// </summary>
        private short[] shortFields;

        /// <summary>
        /// An array that contains the short array fields in this package.
        /// </summary>
        private short[][] shortArrayFields;

        /// <summary>
        /// An array that contains the int fields in this package.
        /// </summary>
        private int[] intFields;

        /// <summary>
        /// An array that contains the int array fields in this package.
        /// </summary>
        private int[][] intArrayFields;

        /// <summary>
        /// An array that contains the long fields in this package.
        /// </summary>
        private long[] longFields;

        /// <summary>
        /// An array that contains the long array fields in this package.
        /// </summary>
        private long[][] longArrayFields;

        /// <summary>
        /// An array that contains the string fields in this package.
        /// </summary>
        private string[] stringFields;

        /// <summary>
        /// An array that contains the string array fields in this package.
        /// </summary>
        private string[][] stringArrayFields;

        /// <summary>
        /// An array that contains an initialization flag for each fields.
        /// </summary>
        private bool[] fieldsInitialized;

        /// <summary>
        /// Temporary storage for the sender byte.
        /// </summary>
        private byte senderTmp;

        /// <summary>
        /// This flag becomes true in case of the first parsing error.
        /// </summary>
        private bool parseError;

        /// <summary>
        /// The index of the currently parsed field, -2 if the header is being parsed currently or -1 if the
        /// format and sender indicator bytes are being parsed currently.
        /// </summary>
        private int currentlyParsedField;

        /// <summary>
        /// The index of the currently parsed element in the currently parsed array field.
        /// </summary>
        private int currentlyParsedArrayElement;

        /// <summary>
        /// The length of the currently parsed array field.
        /// </summary>
        private int currentlyParsedArrayLength;

        /// <summary>
        /// Helper class to execute continuous parse.
        /// </summary>
        private ParseHelper parseHelper;

        /// <summary>
        /// The "magic number" that is written at the beginning of every RCPackage.
        /// </summary>
        /// <remarks>This is the character sequence "RC".</remarks>
        private static readonly byte[] MAGIC_NUMBER = new byte[2] { 0x52, 0x43 };

        #endregion
    }
}
