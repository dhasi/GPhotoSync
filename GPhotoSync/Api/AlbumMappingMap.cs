using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace GPhotoSync
{
    public class AlbumMappingMap : ClassMapping<AlbumMapping>
    {
        public AlbumMappingMap()
        {
            Id(x => x.Id, i => i.Generator(Generators.GuidComb));
            Property(x => x.AlbumId);
            Property(x => x.LocalPath);
        }
    }
}
