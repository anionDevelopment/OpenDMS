using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.APIServer.Mid.AuthS;
using GRYLibrary.Core.APIServer.Mid.AutS;
using GRYLibrary.Core.APIServer.Mid.Ex;
using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.Services.Auth.R;
using GRYLibrary.Core.APIServer.Services.CredH;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.FilePath;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDMSBackend.Core.BackgroundServices;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Core.Services.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using OpenDMSBackendUtilities = OpenDMSBackend.Core.Misc.Utilities;

namespace OpenDMSBackend.Core
{
    internal class Program
    {
        internal Action<FunctionalInformation<CodeUnitSpecificConstants, CodeUnitSpecificConfiguration, CommandlineParameter>> SetupMocks { get; set; }
        internal bool ListenOnEveryIP { get; set; } = false;
        internal bool RunAsync { get; set; } = false;
        internal IBusinessLogicService? _BusinessLogicService;
        internal IGRYLog? _Log;

        internal IHostApplicationLifetime? _HostApplicationLifetime;

        internal static int Main(string[] commandlineArguments)
        {
            return new Program().MainImplementation(commandlineArguments);
        }
        internal async Task<int> MainImplementationAsync(string[] commandlineArguments)
        {
            return await Task.Run(() => this.MainImplementation(commandlineArguments));
        }
        internal int MainImplementation(string[] commandlineArguments)
        {
            bool runPersistent = false;
            return Tools.RunAPIServer<CommandlineParameter, CodeUnitSpecificConstants, CodeUnitSpecificConfiguration>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitDescription, Version3.Parse(GeneralConstants.CodeUnitVersion), OpenDMSBackendUtilities.GetEnvironmentTargetType(), GUtilities.GetExecutionMode(commandlineArguments), commandlineArguments, null, (apiServerConfiguration) =>
            {
                apiServerConfiguration.SetInitialzationInformationAction = (initializationInformation) =>
                {
                    string domain = Tools.GetDefaultDomainValue(GeneralConstants.CodeUnitName);
                    initializationInformation.ApplicationConstants.CommonRoutesHostInformation = new DoNotHostCommonRoutes();
                    initializationInformation.ApplicationConstants.HostMaintenanceInformation = new HostMaintenanceRoutes()
                    {
                        ControllerType = typeof(MaintenanceRoutesController)
                    };
                    initializationInformation.ApplicationConstants.ListenOnEveryIP = this.ListenOnEveryIP;
                    initializationInformation.ApplicationConstants.KnownTypes.Add(typeof(CodeUnitSpecificConfiguration));
                    initializationInformation.ApplicationConstants.AuthenticationMiddleware = typeof(AuthSMiddleware);
                    initializationInformation.ApplicationConstants.AuthorizationMiddleware = typeof(AutSRMiddleware);
                    initializationInformation.ApplicationConstants.LoggingMiddleware = typeof(DRequestLoggingMiddleware);
                    initializationInformation.ApplicationConstants.ExceptionManagerMiddleware = typeof(DefaultExceptionHandlerMiddleware);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = true;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.LoginIsEnabled = true;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware = new DRequestLoggingConfiguration()
                    {
                        NotLoggedRoutes = new HashSet<string>()
                        {
                            @$"^/favicon\.ico$",
                            @$"^/API/Other/Resources/APISpecification/*",
                            @$"^/API/Other/Maintenance/Metrics$",
                            @$"^/API/Other/Maintenance/HealthCheck$",
                        },
                        MaximalLengthofRequestBodies = 500,
                        MaximalLengthOfResponseBodies = 500,
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ConfigurationForExceptionManagerMiddleware = new ExceptionManagerConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.MaintenanceRoutesInformation = new MaintenanceRoutesInformation();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthenticationConfiguration = new AuthSConfiguration()
                    {
                        RoutesWhereUnauthenticatedAccessIsAllowed = new HashSet<string>()
                        {
                            @$"^/favicon\.ico$",
                            @$"^/API/Other/Resources/APISpecification/*",
                            @$"^/API/Other/Maintenance/Metrics$",
                            @$"^/API/Other/Maintenance/HealthCheck$",
                        },
                    };
                    runPersistent = initializationInformation.ApplicationConstants.Environment is not Development && initializationInformation.ApplicationConstants.ExecutionMode is RunProgram;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration = new DatabasePersistenceConfiguration()
                    {
                        DatabaseConnectionString = "Server=opendms_database;Port=5432;Database=OpenDMSDatabase;UID=root;PWD=R00tpa55w0rd;Search Path=public;",
                        DatabaseType = Debugger.IsAttached ? "Transient" : "PostgreSQL",
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./AuditLog.log"), true);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.CommonRoutesInformation = new CommonRoutesInformation()
                    {
                        ContactLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/Contact",
                        LicenseLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/License",
                        TermsOfServiceLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/TermsOfService"
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthorizationConfiguration = new AutSRConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ImportDefinitions = new HashSet<ImportDefinition>();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.DefaultOCRLanguages = new HashSet<string>() { "en" };
                    runPersistent = initializationInformation.ApplicationConstants.Environment is not Development && initializationInformation.ApplicationConstants.ExecutionMode is RunProgram;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.HeaderServiceConfiguration = new HeaderServiceConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.HostAPISpecificationForInNonDevelopmentEnvironment = true;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Protocol = new HTTP( HTTP.DefaultPort);
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Domain = domain;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePasswordHex = GeneralConstants.DevelopmentCertificatePasswordHex;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePFXHex = GeneralConstants.DevelopmentCertificatePFXHex;
                    if (!initializationInformation.CommandlineParameter.RealRun)
                    {
                        GRYLibrary.Core.Misc.Utilities.EnsureDirectoryDoesNotExist(initializationInformation.ApplicationConstants.BaseFolder);
                        GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(initializationInformation.ApplicationConstants.BaseFolder);
                    }
                };
                apiServerConfiguration.SetFunctionalInformationAction = (functionalInformation) =>
                {
                    try
                    {
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForLoggingMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration);
                        IGeneralLogger logger = functionalInformation.Logger;
                        IAuditLog auditLog = new AuditLog(functionalInformation.InitializationInformation.ApplicationConstants.ExecutionMode.Accept(new GetLoggerVisitor(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration, functionalInformation.InitializationInformation.ApplicationConstants.GetLogFolder(), "AuditLog")));
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuditLog>(auditLog);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IIdGenerator<ulong>, Services.IdGenerator>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITimeService, TimeService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<CommandlineParameter>(functionalInformation.InitializationInformation.CommandlineParameter);
                        bool useDatabase = functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType != null && functionalInformation.InitializationInformation.CommandlineParameter.RealRun && functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType != "Transient";
                        if (useDatabase)
                        {
                            logger.Log($"Run persistent using database \"{functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType}\".", LogLevel.Information);
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, PersistentAuthenticationService>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<IPersistence>());
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, DatabasePersistence>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IDatabasePersistenceConfiguration>(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration);
                            if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "PostgreSQL")
                            {
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderPostgreSQL>();
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGenericDatabaseInteractor, PostgreSQLDatabaseInteractor>();
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOpenDMSDatabaseInteractor, DatabaseInteractorPostgreSQL>();
                            }
                            else if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "MariaDB")
                            {
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderMariaDB>();
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGenericDatabaseInteractor, MariaDBDatabaseInteractor>();
                                functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOpenDMSDatabaseInteractor, DatabaseInteractorMariaDB>();
                            }
                            else
                            {
                                throw new NotSupportedException("Database not supported. For a list of supported databases see the documentation.");
                            }
                        }
                        else
                        {
                            logger.Log($"Run transient.", LogLevel.Information);
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, TransientPersistence>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServiceSettings>(new AuthenticationServiceSettings()
                            {
                                BaseRoleOfAllUser = CodeUnitSpecificConstants.RolenameUsers
                            });
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, OpenDMSBackendTransientAuthenticationService>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>, TransientAuthenticationServicePersistence>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>>());
                        }
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGeneralResourceLoader, Services.GeneralResourceLoader>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IManagementScheduler, ManagementScheduler>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOCRServiceWrapper, OCRServiceWrapper>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IBusinessLogicService, BusinessLogicService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<IAuthenticationService<Model.BusinessTypes.User>>());
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IRoleBasedAuthorizationService, StaticRoleBasedUserAuthorizationService<Model.BusinessTypes.User>>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IUserAuthorizationService>(sp => sp.GetRequiredService<IRoleBasedAuthorizationService>());
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthorizationService>(sp => sp.GetRequiredService<IUserAuthorizationService>());
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<ICredentialsProvider, HeaderService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IBusinessLogicService, BusinessLogicService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.HeaderServiceConfiguration);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.CommonRoutesInformation);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.MaintenanceRoutesInformation);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForLoggingMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuthenticationConfiguration);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForAuthenticationMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuthorizationConfiguration);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForAuthorizationMiddleware);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IInitializationService<CommandlineParameter>, InitializationService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IMetricsService, MetricsService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IHealthCheck, HealthCheck>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IExampleDataCreator, ExampleDataCreator>();
                        this.SetupMocks?.Invoke(functionalInformation);

                    }
                    catch
                    {
                        throw;
                    }
                };
                apiServerConfiguration.ConfigureWebApplication = (functionalInformationForWebApplication) =>
                {
                    try
                    {
                        this._BusinessLogicService = functionalInformationForWebApplication.WebApplication.Services.GetService<IBusinessLogicService>();
                        this._Log = functionalInformationForWebApplication.WebApplication.Services.GetService<IGRYLog>();
                        this._HostApplicationLifetime = functionalInformationForWebApplication.WebApplication.Services.GetService<IHostApplicationLifetime>();
                        IManagementScheduler managementScheduler = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IManagementScheduler>());
                        IMetricsService metricsService = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IMetricsService>());
                        functionalInformationForWebApplication.RunAsync = this.RunAsync;
                        bool runServices = functionalInformationForWebApplication.InitializationInformation.CommandlineParameter.RealRun;
                        functionalInformationForWebApplication.PreRun = () =>
                        {
                            //initialize
                            GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IInitializationService<CommandlineParameter>>()).Initialize(apiServerConfiguration.CommandlineParameter);
                            if (runServices)
                            {
                                //start background-services
                                metricsService.StartAsync();
                                managementScheduler.StartAsync();
                            }
                        };
                        functionalInformationForWebApplication.PostRun = () =>
                        {
                            if (runServices)
                            {
                                metricsService.Stop().Wait();
                                managementScheduler.Stop().Wait();
                            }
                        };

                    }
                    catch
                    {
                        throw;
                    }
                };
            });
        }

        internal void Stop()
        {
            GUtilities.AssertNotNull(this._HostApplicationLifetime, nameof(this._HostApplicationLifetime)).StopApplication();
        }
    }
}
