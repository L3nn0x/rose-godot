// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace Rose.Network.Packets
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct CharacterCreateRequest : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static CharacterCreateRequest GetRootAsCharacterCreateRequest(ByteBuffer _bb) { return GetRootAsCharacterCreateRequest(_bb, new CharacterCreateRequest()); }
  public static CharacterCreateRequest GetRootAsCharacterCreateRequest(ByteBuffer _bb, CharacterCreateRequest obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public CharacterCreateRequest __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public string Name { get { int o = __p.__offset(4); return o != 0 ? __p.__string(o + __p.bb_pos) : null; } }
#if ENABLE_SPAN_T
  public Span<byte> GetNameBytes() { return __p.__vector_as_span<byte>(4, 1); }
#else
  public ArraySegment<byte>? GetNameBytes() { return __p.__vector_as_arraysegment(4); }
#endif
  public byte[] GetNameArray() { return __p.__vector_as_array<byte>(4); }
  public short HairId { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetShort(o + __p.bb_pos) : (short)0; } }
  public short FaceId { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetShort(o + __p.bb_pos) : (short)0; } }
  public short JobId { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetShort(o + __p.bb_pos) : (short)0; } }
  public byte GenderId { get { int o = __p.__offset(12); return o != 0 ? __p.bb.Get(o + __p.bb_pos) : (byte)0; } }

  public static Offset<Rose.Network.Packets.CharacterCreateRequest> CreateCharacterCreateRequest(FlatBufferBuilder builder,
      StringOffset nameOffset = default(StringOffset),
      short hair_id = 0,
      short face_id = 0,
      short job_id = 0,
      byte gender_id = 0) {
    builder.StartTable(5);
    CharacterCreateRequest.AddName(builder, nameOffset);
    CharacterCreateRequest.AddJobId(builder, job_id);
    CharacterCreateRequest.AddFaceId(builder, face_id);
    CharacterCreateRequest.AddHairId(builder, hair_id);
    CharacterCreateRequest.AddGenderId(builder, gender_id);
    return CharacterCreateRequest.EndCharacterCreateRequest(builder);
  }

  public static void StartCharacterCreateRequest(FlatBufferBuilder builder) { builder.StartTable(5); }
  public static void AddName(FlatBufferBuilder builder, StringOffset nameOffset) { builder.AddOffset(0, nameOffset.Value, 0); }
  public static void AddHairId(FlatBufferBuilder builder, short hairId) { builder.AddShort(1, hairId, 0); }
  public static void AddFaceId(FlatBufferBuilder builder, short faceId) { builder.AddShort(2, faceId, 0); }
  public static void AddJobId(FlatBufferBuilder builder, short jobId) { builder.AddShort(3, jobId, 0); }
  public static void AddGenderId(FlatBufferBuilder builder, byte genderId) { builder.AddByte(4, genderId, 0); }
  public static Offset<Rose.Network.Packets.CharacterCreateRequest> EndCharacterCreateRequest(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<Rose.Network.Packets.CharacterCreateRequest>(o);
  }
  public static void FinishCharacterCreateRequestBuffer(FlatBufferBuilder builder, Offset<Rose.Network.Packets.CharacterCreateRequest> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedCharacterCreateRequestBuffer(FlatBufferBuilder builder, Offset<Rose.Network.Packets.CharacterCreateRequest> offset) { builder.FinishSizePrefixed(offset.Value); }
};


}