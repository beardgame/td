// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: lobby.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Bearded.TD.Proto {

  /// <summary>Holder for reflection information generated from lobby.proto</summary>
  public static partial class LobbyReflection {

    #region Descriptor
    /// <summary>File descriptor for lobby.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static LobbyReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgtsb2JieS5wcm90bxIKYmVhcmRlZC50ZBoJbW9kLnByb3RvIn0KBUxvYmJ5",
            "EgoKAmlkGAEgASgDEgwKBG5hbWUYAiABKAkSFwoPbWF4X251bV9wbGF5ZXJz",
            "GAMgASgFEhsKE2N1cnJlbnRfbnVtX3BsYXllcnMYBCABKAUSJAoLZW5hYmxl",
            "ZF9tb2QYBSADKAsyDy5iZWFyZGVkLnRkLk1vZEITqgIQQmVhcmRlZC5URC5Q",
            "cm90b2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Bearded.TD.Proto.ModReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Bearded.TD.Proto.Lobby), global::Bearded.TD.Proto.Lobby.Parser, new[]{ "Id", "Name", "MaxNumPlayers", "CurrentNumPlayers", "EnabledMod" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class Lobby : pb::IMessage<Lobby> {
    private static readonly pb::MessageParser<Lobby> _parser = new pb::MessageParser<Lobby>(() => new Lobby());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Lobby> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Bearded.TD.Proto.LobbyReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Lobby() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Lobby(Lobby other) : this() {
      id_ = other.id_;
      name_ = other.name_;
      maxNumPlayers_ = other.maxNumPlayers_;
      currentNumPlayers_ = other.currentNumPlayers_;
      enabledMod_ = other.enabledMod_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Lobby Clone() {
      return new Lobby(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private long id_;
    /// <summary>
    /// Unique identifier of the lobby. Assigned by the master server.
    /// Can be absent when the lobby is unknown to the master server.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "name" field.</summary>
    public const int NameFieldNumber = 2;
    private string name_ = "";
    /// <summary>
    /// Human-readable name of the lobby.
    /// Always present.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Name {
      get { return name_; }
      set {
        name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "max_num_players" field.</summary>
    public const int MaxNumPlayersFieldNumber = 3;
    private int maxNumPlayers_;
    /// <summary>
    /// Maximum number of players allowed in the lobby.
    /// Always present.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int MaxNumPlayers {
      get { return maxNumPlayers_; }
      set {
        maxNumPlayers_ = value;
      }
    }

    /// <summary>Field number for the "current_num_players" field.</summary>
    public const int CurrentNumPlayersFieldNumber = 4;
    private int currentNumPlayers_;
    /// <summary>
    /// Current number of players allowed in the lobby.
    /// Always present.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CurrentNumPlayers {
      get { return currentNumPlayers_; }
      set {
        currentNumPlayers_ = value;
      }
    }

    /// <summary>Field number for the "enabled_mod" field.</summary>
    public const int EnabledModFieldNumber = 5;
    private static readonly pb::FieldCodec<global::Bearded.TD.Proto.Mod> _repeated_enabledMod_codec
        = pb::FieldCodec.ForMessage(42, global::Bearded.TD.Proto.Mod.Parser);
    private readonly pbc::RepeatedField<global::Bearded.TD.Proto.Mod> enabledMod_ = new pbc::RepeatedField<global::Bearded.TD.Proto.Mod>();
    /// <summary>
    /// List of mods currently enabled in the lobby.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Bearded.TD.Proto.Mod> EnabledMod {
      get { return enabledMod_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Lobby);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Lobby other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Name != other.Name) return false;
      if (MaxNumPlayers != other.MaxNumPlayers) return false;
      if (CurrentNumPlayers != other.CurrentNumPlayers) return false;
      if(!enabledMod_.Equals(other.enabledMod_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0L) hash ^= Id.GetHashCode();
      if (Name.Length != 0) hash ^= Name.GetHashCode();
      if (MaxNumPlayers != 0) hash ^= MaxNumPlayers.GetHashCode();
      if (CurrentNumPlayers != 0) hash ^= CurrentNumPlayers.GetHashCode();
      hash ^= enabledMod_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(Id);
      }
      if (Name.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Name);
      }
      if (MaxNumPlayers != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(MaxNumPlayers);
      }
      if (CurrentNumPlayers != 0) {
        output.WriteRawTag(32);
        output.WriteInt32(CurrentNumPlayers);
      }
      enabledMod_.WriteTo(output, _repeated_enabledMod_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Id);
      }
      if (Name.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
      }
      if (MaxNumPlayers != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(MaxNumPlayers);
      }
      if (CurrentNumPlayers != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(CurrentNumPlayers);
      }
      size += enabledMod_.CalculateSize(_repeated_enabledMod_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Lobby other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0L) {
        Id = other.Id;
      }
      if (other.Name.Length != 0) {
        Name = other.Name;
      }
      if (other.MaxNumPlayers != 0) {
        MaxNumPlayers = other.MaxNumPlayers;
      }
      if (other.CurrentNumPlayers != 0) {
        CurrentNumPlayers = other.CurrentNumPlayers;
      }
      enabledMod_.Add(other.enabledMod_);
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
            Id = input.ReadInt64();
            break;
          }
          case 18: {
            Name = input.ReadString();
            break;
          }
          case 24: {
            MaxNumPlayers = input.ReadInt32();
            break;
          }
          case 32: {
            CurrentNumPlayers = input.ReadInt32();
            break;
          }
          case 42: {
            enabledMod_.AddEntriesFrom(input, _repeated_enabledMod_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
