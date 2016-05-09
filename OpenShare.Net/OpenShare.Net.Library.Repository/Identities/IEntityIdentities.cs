using System;

namespace OpenShare.Net.Library.Repository.Identities
{
    public interface IEntityIdentity_IdAsInt32 : IEntityIdentity
    {

    }

    public interface IEntityIdentity_idAsInt32
    {
        int id { get; set; }
    }

    public interface IEntityIdentity_IDAsInt32
    {
        int ID { get; set; }
    }

    public interface IEntityIdentity_IdAsString
    {
        string Id { get; set; }
    }

    public interface IEntityIdentity_idAsString
    {
        string id { get; set; }
    }

    public interface IEntityIdentity_IDAsString
    {
        string ID { get; set; }
    }

    public interface IEntityIdentity_IdAsGuid
    {
        Guid Id { get; set; }
    }

    public interface IEntityIdentity_idAsGuid
    {
        Guid id { get; set; }
    }

    public interface IEntityIdentity_IDAsGuid
    {
        Guid ID { get; set; }
    }
}
