using System;
using OpenShare.Net.Library.Repository.Identities;

namespace OpenShare.Net.Library.Repository.IdentityKeys
{
    public class EntityIdentityKey_IdAsInt32 : EntityIdentityKey
    {

    }

    public interface EntityIdentityKey_idAsInt32 : IEntityIdentity_idAsInt32
    {
        int id { get; set; }
    }

    public interface EntityIdentityKey_IDAsInt32 : IEntityIdentity_IDAsInt32
    {
        int ID { get; set; }
    }

    public interface EntityIdentityKey_IdAsString : IEntityIdentity_IdAsString
    {
        string Id { get; set; }
    }

    public interface EntityIdentityKey_idAsString : IEntityIdentity_idAsString
    {
        string id { get; set; }
    }

    public interface EntityIdentityKey_IDAsString : IEntityIdentity_IDAsString
    {
        string ID { get; set; }
    }

    public interface EntityIdentityKey_IdAsGuid : IEntityIdentity_IdAsGuid
    {
        Guid Id { get; set; }
    }

    public interface EntityIdentityKey_idAsGuid : IEntityIdentity_idAsGuid
    {
        Guid id { get; set; }
    }

    public interface EntityIdentityKey_IDAsGuid : IEntityIdentity_IDAsGuid
    {
        Guid ID { get; set; }
    }
}
