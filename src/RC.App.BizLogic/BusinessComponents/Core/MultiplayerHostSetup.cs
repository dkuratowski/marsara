using System;
using System.Collections.Generic;
using System.Threading;
using RC.Common;
using RC.Common.Configuration;
using RC.Common.ComponentModel;
using RC.Common.Diagnostics;
using RC.App.BizLogic.BusinessComponents;
using RC.DssServices;
using RC.Engine.Simulator.Engine;

namespace RC.App.BizLogic.BusinessComponents.Core
{
    class MultiplayerHostSetup : IDssHostSetup, IDisposable
    {
        public MultiplayerHostSetup(string hostName, byte[] mapBytes, IPlayerManager playerManager)
        {
            this.mapBytes = mapBytes;

            this.playerNames = new string[playerManager.NumberOfSlots];
            this.playerNames[0] = hostName;
            for (int i = 1; i < playerManager.NumberOfSlots; i++)
            {
                this.playerNames[i] = null;
            }

            this.playerManager = playerManager;
            this.playerManager[0].ConnectRandomPlayer(RaceEnum.Terran);

            this.marshal = new Marshal();
        }

        #region IDisposable

        public void Dispose()
        {
            this.scheduler.Dispose();
        }

        #endregion IDisposable

        #region IDssHostSetup methods

        /// <see cref="IDssHostSetup.GuestConnectionLost"/>
        bool IDssHostSetup.GuestConnectionLost(int guestIndex)
        {
            return this.marshal.Invoke(() => this.GuestConnectionLost(guestIndex));
        }

        /// <see cref="IDssHostSetup.GuestLeftDss"/>
        bool IDssHostSetup.GuestLeftDss(int guestIndex)
        {
            return this.marshal.Invoke(() => this.GuestLeftDss(guestIndex));
        }

        /// <see cref="IDssHostSetup.ExecuteNextStep"/>
        DssSetupResult IDssHostSetup.ExecuteNextStep(IDssGuestChannel[] channelsToGuests)
        {
            //return this.marshal.Invoke(() => this.ExecuteNextStep(channelsToGuests));
            HashSet<IDssGuestChannel> newGuests = new HashSet<IDssGuestChannel>();
            for (int guestIdx = 0; guestIdx < channelsToGuests.Length; guestIdx++)
            {
                int slotIdx = guestIdx + 1;
                IDssGuestChannel channel = channelsToGuests[guestIdx];
                IPlayerSlot slot = this.playerManager[slotIdx];
                if (this.IsNewGuest(channel, slot))
                {
                    newGuests.Add(channel);
                    slot.ConnectRandomPlayer(RaceEnum.Terran);
                }
                else if (this.IsExistingGuest(channel, slot))
                {
                    if (this.playerNames[slotIdx] == null)
                    {
                        string guestName = this.ReadGuestName(channel);
                        if (guestName != null)
                        {
                            this.playerNames[slotIdx] = guestName;
                        }
                        else
                        {
                            slot.DisconnectPlayer();
                            channel.DropGuest(true);
                        }
                    }
                }
                else if (this.IsDisconnectedGuest(channel, slot))
                {
                    slot.DisconnectPlayer();
                }
            }

            for (int guestIdx = 0; guestIdx < channelsToGuests.Length; guestIdx++)
            {
                int slotIdx = guestIdx + 1;
                IDssGuestChannel channel = channelsToGuests[guestIdx];
                if (channel.ChannelState == DssChannelState.GUEST_CONNECTED)
                {
                    List<RCPackage> packagesToSend = new List<RCPackage>(this.CreateStatusPackages());
                    if (newGuests.Contains(channel))
                    {
                        packagesToSend.Add(this.CreateGameInfoPackage());
                    }
                    channel.RequestToGuest = packagesToSend.ToArray();
                }
            }

            return DssSetupResult.CONTINUE_SETUP;
        }

        #endregion IDssHostSetup methods

        private bool IsNewGuest(IDssGuestChannel channel, IPlayerSlot slot)
        {
            return channel.ChannelState == DssChannelState.GUEST_CONNECTED &&
                   slot.State != PlayerSlotStateEnum.Connected;
        }

        private bool IsExistingGuest(IDssGuestChannel channel, IPlayerSlot slot)
        {
            return channel.ChannelState == DssChannelState.GUEST_CONNECTED &&
                   slot.State == PlayerSlotStateEnum.Connected;
        }

        private bool IsDisconnectedGuest(IDssGuestChannel channel, IPlayerSlot slot)
        {
            return channel.ChannelState != DssChannelState.GUEST_CONNECTED &&
                   slot.State == PlayerSlotStateEnum.Connected;
        }

        private string ReadGuestName(IDssGuestChannel channel)
        {
            if (channel.AnswerFromGuest == null || channel.AnswerFromGuest.Length != 1)
            {
                return null;
            }

            RCPackage guestLoginPackage = channel.AnswerFromGuest[0];
            if (!guestLoginPackage.IsCommitted || guestLoginPackage.PackageFormat.ID != MULTIPLAYER_GUEST_LOGIN_FORMAT)
            {
                return null;
            }

            return guestLoginPackage.ReadString(0);
        }

        private RCPackage[] CreateStatusPackages()
        {
            RCPackage[] slotPackages = new RCPackage[this.playerManager.NumberOfSlots];
            for (int slotIdx = 0; slotIdx < this.playerManager.NumberOfSlots; slotIdx++)
            {
                IPlayerSlot slot = this.playerManager[slotIdx];

                RCPackage slotPackage = RCPackage.CreateNetworkControlPackage(MULTIPLAYER_SLOT_INFO_FORMAT);
                slotPackage.WriteByte(0, (byte)slotIdx);
                slotPackage.WriteByte(1, (byte)slot.State);
                slotPackage.WriteByte(2, slot.State == PlayerSlotStateEnum.Connected ? (byte)slot.Player : (byte)0xFF);
                slotPackage.WriteByte(3, slot.State == PlayerSlotStateEnum.Connected ? (byte)slot.Race : (byte)0xFF);
                slotPackage.WriteString(4, slot.State == PlayerSlotStateEnum.Connected && this.playerNames[slotIdx] != null ? this.playerNames[slotIdx] : string.Empty);
                slotPackage.WriteByte(5, slot.State == PlayerSlotStateEnum.Connected ? (this.playerNames[slotIdx] != null ? (byte)0x01 : (byte)0x00) : (byte)0xFF);
                slotPackage.WriteInt(6, slot.State == PlayerSlotStateEnum.Connected ? slot.StartLocation.ID.Read() : -1);

                slotPackages[slotIdx] = slotPackage;
            }
            return slotPackages;
        }

        private RCPackage CreateGameInfoPackage()
        {
            RCPackage gameInfoPackage = RCPackage.CreateNetworkControlPackage(MULTIPLAYER_GAME_INFO_FORMAT);
            gameInfoPackage.WriteByteArray(0, this.mapBytes);
            return gameInfoPackage;
        }

        private IPlayerManager playerManager;

        private string[] playerNames;

        private byte[] mapBytes;

        private Marshal marshal;

        private static int MULTIPLAYER_GUEST_LOGIN_FORMAT = RCPackageFormatMap.Get("RC.App.BizLogic.MultiplayerGuestLogin");
        private static int MULTIPLAYER_SLOT_INFO_FORMAT = RCPackageFormatMap.Get("RC.App.BizLogic.MultiplayerSlotInfo");
        private static int MULTIPLAYER_GAME_INFO_FORMAT = RCPackageFormatMap.Get("RC.App.BizLogic.MultiplayerGameInfo");
    }
}
