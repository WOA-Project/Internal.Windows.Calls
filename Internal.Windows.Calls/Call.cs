#nullable enable

using Internal.Windows.Calls.PhoneOm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Globalization.PhoneNumberFormatting;
using Windows.System;

using static Internal.Windows.Calls.PhoneOm.Exports;

namespace Internal.Windows.Calls
{
    public sealed class Call
    {
        private readonly CallManager _CallManager;
        private readonly List<Call> _CallsInConference = new();
        private DateTimeOffset? _StartTime;
        private DateTimeOffset? _EndTime;
        private DateTimeOffset? _LastFlashedTime;
        private DateTimeOffset? _ArrivalTime;
        private CallState _State;
        private uint _ID;
        private uint _ConferenceID;

        [Obsolete]
        public event TypedEventHandler<Call, AvailableActions> AvailableActionsChanged;
        public event TypedEventHandler<Call, CallStateChangedEventArgs> StateChanged;
        public event TypedEventHandler<Call, CallIDChangedEventArgs> IDChanged;
        public event TypedEventHandler<Call, CallIDChangedEventArgs> ConferenceIDChanged;
        public event TypedEventHandler<Call, CallTimeChangedEventArgs> StartTimeChanged;
        public event TypedEventHandler<Call, CallTimeChangedEventArgs> EndTimeChanged;
        public event TypedEventHandler<Call, CallTimeChangedEventArgs> LastFlashedTimeChanged;
        public event TypedEventHandler<Call, CallTimeChangedEventArgs> ArrivalTimeChanged;

        public Contact? Contact
        {
            get; private set;
        }
        public ContactPhone? Phone
        {
            get; private set;
        }
        public Contact? ForwardContact
        {
            get; private set;
        }
        public ContactPhone? ForwardPhone
        {
            get; private set;
        }
        public DateTimeOffset? StartTime
        {
            get => _StartTime;
            private set
            {
                if (value != _StartTime)
                {
                    DateTimeOffset? old = _StartTime;
                    _StartTime = value;
                    StartTimeChanged?.Invoke(this, new CallTimeChangedEventArgs(old, _StartTime));
                }
            }
        }
        public DateTimeOffset? EndTime
        {
            get => _EndTime;
            private set
            {
                if (value != _EndTime)
                {
                    DateTimeOffset? old = _EndTime;
                    _EndTime = value;
                    EndTimeChanged?.Invoke(this, new CallTimeChangedEventArgs(old, _EndTime));
                }
            }
        }
        public DateTimeOffset? LastFlashedTime
        {
            get => _LastFlashedTime;
            private set
            {
                if (value != _LastFlashedTime)
                {
                    DateTimeOffset? old = _LastFlashedTime;
                    _LastFlashedTime = value;
                    LastFlashedTimeChanged?.Invoke(this, new CallTimeChangedEventArgs(old, _LastFlashedTime));
                }
            }
        }
        public DateTimeOffset? CallArrivalTime
        {
            get => _ArrivalTime;
            private set
            {
                if (value != _ArrivalTime)
                {
                    DateTimeOffset? old = _ArrivalTime;
                    _ArrivalTime = value;
                    ArrivalTimeChanged?.Invoke(this, new CallTimeChangedEventArgs(old, _ArrivalTime));
                }
            }
        }
        public CallState State
        {
            get => _State;
            private set
            {
                if (value != State)
                {
                    CallState state = State;
                    _State = value;
                    StateChanged?.Invoke(this, new CallStateChangedEventArgs(state, State, StateReason));
                }
            }
        }
        public CallStateReason StateReason
        {
            get; private set;
        }
        public CallDirection Direction
        {
            get; private set;
        }
        public uint ID
        {
            get => _ID;
            private set
            {
                if (value != ID)
                {
                    uint id = ID;
                    _ID = value;
                    IDChanged?.Invoke(this, new CallIDChangedEventArgs(id, ID));
                }
            }
        }
        public uint ConferenceID
        {
            get => _ConferenceID;
            private set
            {
                if (value != _ConferenceID)
                {
                    uint id = _ConferenceID;
                    _ConferenceID = value;
                    ConferenceIDChanged?.Invoke(this, new CallIDChangedEventArgs(id, _ConferenceID));
                }
            }
        }
        public PhoneLine? Line
        {
            get; private set;
        }
        public CallFlags field_BF0
        {
            get; private set;
        }
        public CallUpgradeState UpgradeState
        {
            get; private set;
        }
        public AppInfo? OwningApplication
        {
            get; private set;
        }
        public CallAudioQuality AudioQuality
        {
            get; private set;
        }
        public CallAudioFlags AudioFlags
        {
            get; private set;
        }
        public CallVideoFlags VideoFlags
        {
            get; private set;
        }
        public CallVideoTransitionState VideoTransitionState
        {
            get; private set;
        }
        public bool RemotePartyIsVideoCapable
        {
            get; private set;
        }
        public CallVideoConferenceState VideoConferenceState
        {
            get; private set;
        }
        public CallInteractor ActionByExternalDevice
        {
            get; private set;
        }
        public bool IsConference => _ConferenceID > 0U;
        public bool IsHandoverMerged
        {
            get; private set;
        }
        public bool IsRoaming
        {
            get; private set;
        }
        public bool IsBareCall
        {
            get; private set;
        }
        public bool UseCallWaiting
        {
            get; private set;
        }
        public bool SupportsHold
        {
            get; private set;
        }
        public bool IsDtmfWaitPending
        {
            get; private set;
        }
        public AvailableActions AvailableActions
        {
            get; private set;
        }
        public IEnumerable<Call> CallsInConference => _CallsInConference.ToList().AsReadOnly();

#nullable disable
        internal Call(CallManager manager, PH_CALL_INFO callInfo)
        {
            _CallManager = manager;
            Task.Run(() => UpdateState(callInfo)).Wait();
        }
#nullable enable

        private async Task<Tuple<Contact?, ContactPhone?>> FindInfoByNumber(string name, string number)
        {
            Contact? contact = null;
            ContactPhone? phone = null;
            ContactBatch batch = await _CallManager.ContactStore.GetContactReader(new ContactQueryOptions(number, ContactQuerySearchFields.Phone)).ReadBatchAsync();
            if (batch.Status == ContactBatchStatus.Success && batch.Contacts.Count > 0)
            {
                contact = batch.Contacts.First();
                PhoneNumberInfo internalNumber = new(number);
                phone = contact.Phones.First(x => internalNumber.CheckNumberMatch(new PhoneNumberInfo(x.Number)) != PhoneNumberMatchResult.NoMatch);
            }
            else
            {
                phone = new ContactPhone()
                {
                    Kind = ContactPhoneKind.Other,
                    Number = number
                };
                contact = new Contact()
                {
                    Phones = { phone },
                    DisplayNameOverride = name
                };
            }
            return new Tuple<Contact?, ContactPhone?>(contact, phone);
        }

        private async Task<Contact?> FindInfoByName(string name)
        {
            ContactBatch batch = await _CallManager.ContactStore.GetContactReader(new ContactQueryOptions(name)).ReadBatchAsync();
            return batch.Status == ContactBatchStatus.Success && batch.Contacts.Count > 0
                ? batch.Contacts.First()
                : !string.IsNullOrEmpty(name)
                    ? new Contact()
                    {
                        DisplayNameOverride = name
                    }
                    : null;
        }

        private void UpdateAvailableActions(PH_AVAILABLE_ACTIONS actions)
        {
            AvailableActions = new AvailableActions(actions);
        }

        private async Task UpdateState(PH_CALL_INFO callInfo)
        {
            ActionByExternalDevice = callInfo.InitialInteractor;
            AudioFlags = callInfo.AudioFlags;
            AudioQuality = callInfo.AudioQuality;
            try
            {
                StartTime = DateTimeOffset.FromFileTime((((long)callInfo.CallStartTime.dwHighDateTime) << 32) | callInfo.CallStartTime.dwLowDateTime);
            }
            catch
            {

            }
            try
            {
                EndTime = DateTimeOffset.FromFileTime((((long)callInfo.CallEndTime.dwHighDateTime) << 32) | callInfo.CallEndTime.dwLowDateTime);
            }
            catch
            {

            }
            try
            {
                LastFlashedTime = DateTimeOffset.FromFileTime((((long)callInfo.LastFlashedTime.dwHighDateTime) << 32) | callInfo.LastFlashedTime.dwLowDateTime);
            }
            catch
            {

            }
            try
            {
                CallArrivalTime = DateTimeOffset.FromFileTime((((long)callInfo.CallArrivalTime.dwHighDateTime) << 32) | callInfo.CallArrivalTime.dwLowDateTime);
            }
            catch
            {

            }
            ID = callInfo.CallID;
            ConferenceID = callInfo.ConferenceID;
            IsBareCall = callInfo.IsBareCall;
            IsDtmfWaitPending = callInfo.IsDtmfWaitPending;
            IsHandoverMerged = callInfo.IsHandoverMerged;
            IsRoaming = callInfo.IsRoaming;
            RemotePartyIsVideoCapable = callInfo.RemotePartyIsVideoCapable;
            Direction = callInfo.CallDirection;
            AvailableActions = new AvailableActions(callInfo.AvailableActions);
            try
            {
                if ((Line?.Id ?? Guid.Empty) != callInfo.PhoneLineID)
                {
                    Line = await PhoneLine.FromIdAsync(callInfo.PhoneLineID);
                }
            }
            catch
            {

            }
            if (callInfo.OwningApplicationType > 0)
            {
                OwningApplication = (await AppDiagnosticInfo.RequestInfoForAppAsync(callInfo.OwningApplicationId)).FirstOrDefault().AppInfo;
            }
            if (IsConference)
            {
                _CallsInConference.Clear();
                try
                {
                    PhoneGetCallsInConference(ConferenceID, out PH_CALL_INFO[] calls, out uint count);
                    _CallsInConference.AddRange(calls.Select(x => _CallManager.GetCallByID(x.CallID)));
                }
                catch
                {

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(callInfo.DisplayNumber))
                {
                    Tuple<Contact?, ContactPhone?> info = await FindInfoByNumber(callInfo.DisplayName, callInfo.DisplayNumber);
                    Contact = info.Item1;
                    Phone = info.Item2;
                }
                if (Contact == null && !string.IsNullOrEmpty(callInfo.DisplayName))
                {
                    Contact = await FindInfoByName(callInfo.DisplayName);
                }
                if (!string.IsNullOrEmpty(callInfo.ForwardNumber))
                {
                    Tuple<Contact?, ContactPhone?> info = await FindInfoByNumber(string.Empty, callInfo.ForwardNumber);
                    ForwardContact = info.Item1;
                    ForwardPhone = info.Item2;
                }
            }
            StateReason = callInfo.CallStateReason;
            State = callInfo.CallState;
        }

        internal void UpdateAvailableActions()
        {
            PhoneGetAvailableActions(ID, out PH_AVAILABLE_ACTIONS availableActions);
            UpdateAvailableActions(availableActions);
        }

        internal void UpdateID()
        {
            uint id = ID;

            //Simone - This might need to be fixed. In 22000 this export was removed,
            //but I don't have a way to test eventual fixes needed. This comment fixes the crash at startup. 

            //PhoneReinitiateCallerIdLookup(ref id);

            ID = id;
        }

        internal async Task UpdateState()
        {
            PhoneGetCallInfo(ID, out PH_CALL_INFO callInfo);
            await UpdateState(callInfo);
        }

        public void AcceptIncomingEx()
        {
            PhoneAcceptIncomingEx(ID);
        }

        public void AcceptVideo()
        {
            PhoneAcceptVideo(ID);
        }

        public void DropVideo()
        {
            PhoneDropVideo(ID);
        }

        public void End()
        {
            PhoneEnd(ID);
        }

        public void RejectIncoming()
        {
            PhoneRejectIncoming(ID);
        }

        public void SetHold(bool state)
        {
            PhoneSetHold(ID, state);
        }
    }
}
