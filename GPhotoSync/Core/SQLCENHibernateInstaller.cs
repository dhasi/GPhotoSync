using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;

namespace GPhotoSync
{
    public class SQLCENHibernateInstaller : IWindsorInstaller
    {
        private readonly string _fileName;
        private readonly string _connectionString;
        private readonly bool _deleteExistingDb;

        public SQLCENHibernateInstaller(bool deleteExistingDb = false)
        {
            _fileName = "database.sdf";
            _connectionString = string.Format("data source={0}; LCID=1033;", _fileName);
            _deleteExistingDb = deleteExistingDb;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            ConfigureNHiberante(container);
        }

        private void ConfigureNHiberante(IWindsorContainer container)
        {
            var modelMapper = new ModelMapper();
            var asm = typeof(SQLCENHibernateInstaller).Assembly;
            modelMapper.AddMappings(asm
                        .GetTypes()
                        .Where(x => typeof(IConformistHoldersProvider).IsAssignableFrom(x) && !x.IsGenericTypeDefinition));

            var mapping = modelMapper.CompileMappingForAllExplicitlyAddedEntities();

            var cfg = new NHibernate.Cfg.Configuration();
            cfg.SetProperty(Environment.UseProxyValidator, "false");
            cfg.SetProperty(Environment.CurrentSessionContextClass, "thread_static");
            cfg.SetProperty(Environment.Dialect, typeof(MsSqlCeDialect).AssemblyQualifiedName);
            cfg.SetProperty(Environment.ConnectionString, _connectionString);
            cfg.SetProperty(Environment.ShowSql, "false");
            cfg.AddMapping(mapping);

            SetupDatabase(cfg);

            var factory = cfg.BuildSessionFactory();
            container.Register(Component.For<ISessionFactory>().Instance(factory));

            if (_deleteExistingDb)
                new SchemaExport(cfg).Execute(false, true, false);
        }

        private void SetupDatabase(NHibernate.Cfg.Configuration cfg)
        {
            if (_deleteExistingDb)
                File.Delete(_fileName);

            if (!File.Exists(_fileName))
            {
                var engine = new SqlCeEngine(_connectionString);
                engine.CreateDatabase();
                new SchemaExport(cfg).Execute(false, true, false);
            }
        }
    }
}
