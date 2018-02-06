// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: messages.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Bearded.TD.Proto {

  /// <summary>Holder for reflection information generated from messages.proto</summary>
  public static partial class MessagesReflection {

    #region Descriptor
    /// <summary>File descriptor for messages.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MessagesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5tZXNzYWdlcy5wcm90bxIKYmVhcmRlZC50ZBoKZ2FtZS5wcm90bxoLbG9i",
            "YnkucHJvdG8igAIKE01hc3RlclNlcnZlck1lc3NhZ2USJwoJZ2FtZV9pbmZv",
            "GAEgASgLMhQuYmVhcmRlZC50ZC5HYW1lSW5mbxI6Cg5yZWdpc3Rlcl9sb2Ji",
            "eRgCIAEoCzIgLmJlYXJkZWQudGQuUmVnaXN0ZXJMb2JieVJlcXVlc3RIABI2",
            "CgxsaXN0X2xvYmJpZXMYAyABKAsyHi5iZWFyZGVkLnRkLkxpc3RMb2JiaWVz",
            "UmVxdWVzdEgAEkEKEmludHJvZHVjZV90b19sb2JieRgEIAEoCzIjLmJlYXJk",
            "ZWQudGQuSW50cm9kdWNlVG9Mb2JieVJlcXVlc3RIAEIJCgdyZXF1ZXN0IlcK",
            "FFJlZ2lzdGVyTG9iYnlSZXF1ZXN0EiAKBWxvYmJ5GAEgASgLMhEuYmVhcmRl",
            "ZC50ZC5Mb2JieRIPCgdhZGRyZXNzGAIgASgMEgwKBHBvcnQYAyABKAUiFAoS",
            "TGlzdExvYmJpZXNSZXF1ZXN0IlkKF0ludHJvZHVjZVRvTG9iYnlSZXF1ZXN0",
            "EhAKCGxvYmJ5X2lkGAEgASgDEg0KBXRva2VuGAIgASgJEg8KB2FkZHJlc3MY",
            "AyABKAwSDAoEcG9ydBgEIAEoBUITqgIQQmVhcmRlZC5URC5Qcm90b2IGcHJv",
            "dG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Bearded.TD.Proto.GameReflection.Descriptor, global::Bearded.TD.Proto.LobbyReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Bearded.TD.Proto.MasterServerMessage), global::Bearded.TD.Proto.MasterServerMessage.Parser, new[]{ "GameInfo", "RegisterLobby", "ListLobbies", "IntroduceToLobby" }, new[]{ "Request" }, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bearded.TD.Proto.RegisterLobbyRequest), global::Bearded.TD.Proto.RegisterLobbyRequest.Parser, new[]{ "Lobby", "Address", "Port" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bearded.TD.Proto.ListLobbiesRequest), global::Bearded.TD.Proto.ListLobbiesRequest.Parser, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Bearded.TD.Proto.IntroduceToLobbyRequest), global::Bearded.TD.Proto.IntroduceToLobbyRequest.Parser, new[]{ "LobbyId", "Token", "Address", "Port" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class MasterServerMessage : pb::IMessage<MasterServerMessage> {
    private static readonly pb::MessageParser<MasterServerMessage> _parser = new pb::MessageParser<MasterServerMessage>(() => new MasterServerMessage());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MasterServerMessage> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bearded.TD.Proto.MessagesReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MasterServerMessage() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MasterServerMessage(MasterServerMessage other) : this() {
      GameInfo = other.gameInfo_ != null ? other.GameInfo.Clone() : null;
      switch (other.RequestCase) {
        case RequestOneofCase.RegisterLobby:
          RegisterLobby = other.RegisterLobby.Clone();
          break;
        case RequestOneofCase.ListLobbies:
          ListLobbies = other.ListLobbies.Clone();
          break;
        case RequestOneofCase.IntroduceToLobby:
          IntroduceToLobby = other.IntroduceToLobby.Clone();
          break;
      }

    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MasterServerMessage Clone() {
      return new MasterServerMessage(this);
    }

    /// <summary>Field number for the "game_info" field.</summary>
    public const int GameInfoFieldNumber = 1;
    private global::Bearded.TD.Proto.GameInfo gameInfo_;
    /// <summary>
    /// Game info of the requester.
    /// Should always be set.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Bearded.TD.Proto.GameInfo GameInfo {
      get { return gameInfo_; }
      set {
        gameInfo_ = value;
      }
    }

    /// <summary>Field number for the "register_lobby" field.</summary>
    public const int RegisterLobbyFieldNumber = 2;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Bearded.TD.Proto.RegisterLobbyRequest RegisterLobby {
      get { return requestCase_ == RequestOneofCase.RegisterLobby ? (global::Bearded.TD.Proto.RegisterLobbyRequest) request_ : null; }
      set {
        request_ = value;
        requestCase_ = value == null ? RequestOneofCase.None : RequestOneofCase.RegisterLobby;
      }
    }

    /// <summary>Field number for the "list_lobbies" field.</summary>
    public const int ListLobbiesFieldNumber = 3;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Bearded.TD.Proto.ListLobbiesRequest ListLobbies {
      get { return requestCase_ == RequestOneofCase.ListLobbies ? (global::Bearded.TD.Proto.ListLobbiesRequest) request_ : null; }
      set {
        request_ = value;
        requestCase_ = value == null ? RequestOneofCase.None : RequestOneofCase.ListLobbies;
      }
    }

    /// <summary>Field number for the "introduce_to_lobby" field.</summary>
    public const int IntroduceToLobbyFieldNumber = 4;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Bearded.TD.Proto.IntroduceToLobbyRequest IntroduceToLobby {
      get { return requestCase_ == RequestOneofCase.IntroduceToLobby ? (global::Bearded.TD.Proto.IntroduceToLobbyRequest) request_ : null; }
      set {
        request_ = value;
        requestCase_ = value == null ? RequestOneofCase.None : RequestOneofCase.IntroduceToLobby;
      }
    }

    private object request_;
    /// <summary>Enum of possible cases for the "request" oneof.</summary>
    public enum RequestOneofCase {
      None = 0,
      RegisterLobby = 2,
      ListLobbies = 3,
      IntroduceToLobby = 4,
    }
    private RequestOneofCase requestCase_ = RequestOneofCase.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RequestOneofCase RequestCase {
      get { return requestCase_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearRequest() {
      requestCase_ = RequestOneofCase.None;
      request_ = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MasterServerMessage);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MasterServerMessage other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(GameInfo, other.GameInfo)) return false;
      if (!object.Equals(RegisterLobby, other.RegisterLobby)) return false;
      if (!object.Equals(ListLobbies, other.ListLobbies)) return false;
      if (!object.Equals(IntroduceToLobby, other.IntroduceToLobby)) return false;
      if (RequestCase != other.RequestCase) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (gameInfo_ != null) hash ^= GameInfo.GetHashCode();
      if (requestCase_ == RequestOneofCase.RegisterLobby) hash ^= RegisterLobby.GetHashCode();
      if (requestCase_ == RequestOneofCase.ListLobbies) hash ^= ListLobbies.GetHashCode();
      if (requestCase_ == RequestOneofCase.IntroduceToLobby) hash ^= IntroduceToLobby.GetHashCode();
      hash ^= (int) requestCase_;
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (gameInfo_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(GameInfo);
      }
      if (requestCase_ == RequestOneofCase.RegisterLobby) {
        output.WriteRawTag(18);
        output.WriteMessage(RegisterLobby);
      }
      if (requestCase_ == RequestOneofCase.ListLobbies) {
        output.WriteRawTag(26);
        output.WriteMessage(ListLobbies);
      }
      if (requestCase_ == RequestOneofCase.IntroduceToLobby) {
        output.WriteRawTag(34);
        output.WriteMessage(IntroduceToLobby);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (gameInfo_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(GameInfo);
      }
      if (requestCase_ == RequestOneofCase.RegisterLobby) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(RegisterLobby);
      }
      if (requestCase_ == RequestOneofCase.ListLobbies) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ListLobbies);
      }
      if (requestCase_ == RequestOneofCase.IntroduceToLobby) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(IntroduceToLobby);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MasterServerMessage other) {
      if (other == null) {
        return;
      }
      if (other.gameInfo_ != null) {
        if (gameInfo_ == null) {
          gameInfo_ = new global::Bearded.TD.Proto.GameInfo();
        }
        GameInfo.MergeFrom(other.GameInfo);
      }
      switch (other.RequestCase) {
        case RequestOneofCase.RegisterLobby:
          RegisterLobby = other.RegisterLobby;
          break;
        case RequestOneofCase.ListLobbies:
          ListLobbies = other.ListLobbies;
          break;
        case RequestOneofCase.IntroduceToLobby:
          IntroduceToLobby = other.IntroduceToLobby;
          break;
      }

    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (gameInfo_ == null) {
              gameInfo_ = new global::Bearded.TD.Proto.GameInfo();
            }
            input.ReadMessage(gameInfo_);
            break;
          }
          case 18: {
            global::Bearded.TD.Proto.RegisterLobbyRequest subBuilder = new global::Bearded.TD.Proto.RegisterLobbyRequest();
            if (requestCase_ == RequestOneofCase.RegisterLobby) {
              subBuilder.MergeFrom(RegisterLobby);
            }
            input.ReadMessage(subBuilder);
            RegisterLobby = subBuilder;
            break;
          }
          case 26: {
            global::Bearded.TD.Proto.ListLobbiesRequest subBuilder = new global::Bearded.TD.Proto.ListLobbiesRequest();
            if (requestCase_ == RequestOneofCase.ListLobbies) {
              subBuilder.MergeFrom(ListLobbies);
            }
            input.ReadMessage(subBuilder);
            ListLobbies = subBuilder;
            break;
          }
          case 34: {
            global::Bearded.TD.Proto.IntroduceToLobbyRequest subBuilder = new global::Bearded.TD.Proto.IntroduceToLobbyRequest();
            if (requestCase_ == RequestOneofCase.IntroduceToLobby) {
              subBuilder.MergeFrom(IntroduceToLobby);
            }
            input.ReadMessage(subBuilder);
            IntroduceToLobby = subBuilder;
            break;
          }
        }
      }
    }

  }

  public sealed partial class RegisterLobbyRequest : pb::IMessage<RegisterLobbyRequest> {
    private static readonly pb::MessageParser<RegisterLobbyRequest> _parser = new pb::MessageParser<RegisterLobbyRequest>(() => new RegisterLobbyRequest());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<RegisterLobbyRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bearded.TD.Proto.MessagesReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegisterLobbyRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegisterLobbyRequest(RegisterLobbyRequest other) : this() {
      Lobby = other.lobby_ != null ? other.Lobby.Clone() : null;
      address_ = other.address_;
      port_ = other.port_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public RegisterLobbyRequest Clone() {
      return new RegisterLobbyRequest(this);
    }

    /// <summary>Field number for the "lobby" field.</summary>
    public const int LobbyFieldNumber = 1;
    private global::Bearded.TD.Proto.Lobby lobby_;
    /// <summary>
    /// Info of the lobby to register.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Bearded.TD.Proto.Lobby Lobby {
      get { return lobby_; }
      set {
        lobby_ = value;
      }
    }

    /// <summary>Field number for the "address" field.</summary>
    public const int AddressFieldNumber = 2;
    private pb::ByteString address_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Address {
      get { return address_; }
      set {
        address_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port" field.</summary>
    public const int PortFieldNumber = 3;
    private int port_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Port {
      get { return port_; }
      set {
        port_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as RegisterLobbyRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(RegisterLobbyRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Lobby, other.Lobby)) return false;
      if (Address != other.Address) return false;
      if (Port != other.Port) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (lobby_ != null) hash ^= Lobby.GetHashCode();
      if (Address.Length != 0) hash ^= Address.GetHashCode();
      if (Port != 0) hash ^= Port.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (lobby_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Lobby);
      }
      if (Address.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(Address);
      }
      if (Port != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Port);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (lobby_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Lobby);
      }
      if (Address.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Address);
      }
      if (Port != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Port);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(RegisterLobbyRequest other) {
      if (other == null) {
        return;
      }
      if (other.lobby_ != null) {
        if (lobby_ == null) {
          lobby_ = new global::Bearded.TD.Proto.Lobby();
        }
        Lobby.MergeFrom(other.Lobby);
      }
      if (other.Address.Length != 0) {
        Address = other.Address;
      }
      if (other.Port != 0) {
        Port = other.Port;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            if (lobby_ == null) {
              lobby_ = new global::Bearded.TD.Proto.Lobby();
            }
            input.ReadMessage(lobby_);
            break;
          }
          case 18: {
            Address = input.ReadBytes();
            break;
          }
          case 24: {
            Port = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class ListLobbiesRequest : pb::IMessage<ListLobbiesRequest> {
    private static readonly pb::MessageParser<ListLobbiesRequest> _parser = new pb::MessageParser<ListLobbiesRequest>(() => new ListLobbiesRequest());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<ListLobbiesRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bearded.TD.Proto.MessagesReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ListLobbiesRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ListLobbiesRequest(ListLobbiesRequest other) : this() {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ListLobbiesRequest Clone() {
      return new ListLobbiesRequest(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as ListLobbiesRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(ListLobbiesRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(ListLobbiesRequest other) {
      if (other == null) {
        return;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
        }
      }
    }

  }

  public sealed partial class IntroduceToLobbyRequest : pb::IMessage<IntroduceToLobbyRequest> {
    private static readonly pb::MessageParser<IntroduceToLobbyRequest> _parser = new pb::MessageParser<IntroduceToLobbyRequest>(() => new IntroduceToLobbyRequest());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<IntroduceToLobbyRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bearded.TD.Proto.MessagesReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IntroduceToLobbyRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IntroduceToLobbyRequest(IntroduceToLobbyRequest other) : this() {
      lobbyId_ = other.lobbyId_;
      token_ = other.token_;
      address_ = other.address_;
      port_ = other.port_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public IntroduceToLobbyRequest Clone() {
      return new IntroduceToLobbyRequest(this);
    }

    /// <summary>Field number for the "lobby_id" field.</summary>
    public const int LobbyIdFieldNumber = 1;
    private long lobbyId_;
    /// <summary>
    /// Identifier of the lobby to be introduced to.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long LobbyId {
      get { return lobbyId_; }
      set {
        lobbyId_ = value;
      }
    }

    /// <summary>Field number for the "token" field.</summary>
    public const int TokenFieldNumber = 2;
    private string token_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Token {
      get { return token_; }
      set {
        token_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "address" field.</summary>
    public const int AddressFieldNumber = 3;
    private pb::ByteString address_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Address {
      get { return address_; }
      set {
        address_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "port" field.</summary>
    public const int PortFieldNumber = 4;
    private int port_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Port {
      get { return port_; }
      set {
        port_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as IntroduceToLobbyRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(IntroduceToLobbyRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (LobbyId != other.LobbyId) return false;
      if (Token != other.Token) return false;
      if (Address != other.Address) return false;
      if (Port != other.Port) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (LobbyId != 0L) hash ^= LobbyId.GetHashCode();
      if (Token.Length != 0) hash ^= Token.GetHashCode();
      if (Address.Length != 0) hash ^= Address.GetHashCode();
      if (Port != 0) hash ^= Port.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (LobbyId != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(LobbyId);
      }
      if (Token.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Token);
      }
      if (Address.Length != 0) {
        output.WriteRawTag(26);
        output.WriteBytes(Address);
      }
      if (Port != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(Port);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (LobbyId != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(LobbyId);
      }
      if (Token.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Token);
      }
      if (Address.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Address);
      }
      if (Port != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Port);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(IntroduceToLobbyRequest other) {
      if (other == null) {
        return;
      }
      if (other.LobbyId != 0L) {
        LobbyId = other.LobbyId;
      }
      if (other.Token.Length != 0) {
        Token = other.Token;
      }
      if (other.Address.Length != 0) {
        Address = other.Address;
      }
      if (other.Port != 0) {
        Port = other.Port;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            LobbyId = input.ReadInt64();
            break;
          }
          case 18: {
            Token = input.ReadString();
            break;
          }
          case 26: {
            Address = input.ReadBytes();
            break;
          }
          case 32: {
            Port = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
