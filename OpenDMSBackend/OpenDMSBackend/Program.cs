using OpenDMSBackend.Core.Constants;
using GRYLibrary.Core.Misc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using OpenDMSBackend.Core.Configuration;
using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using Microsoft.Extensions.Logging;
using OpenDMSBackend.Core.Database;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.APIServer.Utilities;
using System.Collections.Generic;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Mid.AuthS;
using GRYLibrary.Core.APIServer.MidT.Exception;
using OpenDMSBackend.Core.Miscellaneous;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using OpenDMSBackendUtilities = OpenDMSBackend.Core.Miscellaneous.Utilities;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Auth.R;
using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.Mid.AutS;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using GRYLibrary.Core.APIServer.Mid.Ex;
using OpenDMSBackend.Core.Services;
using GRYLibrary.Core.APIServer.Services.CredH;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using OpenDMSBackend.Core.BackgroundServices;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.FilePath;
using System;
using GRYLibrary.Core.APIServer.Services.Database.DatabaseInterator;

namespace OpenDMSBackend.Core
{
    internal class Program
    {
        internal static int Main(string[] commandlineArguments)
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
                        },
                        MaximalLengthOfResponseBodies = 50,
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.ConfigurationForExceptionManagerMiddleware = new ExceptionManagerConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.MaintenanceRoutesInformation = new MaintenanceRoutesInformation();
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthenticationConfiguration = new AuthSConfiguration()
                    {
                        RoutesWhereUnauthenticatedAccessIsAllowed = new HashSet<string>() {
                            @$"^/API/Other/Resources/APISpecification/*",
                            @$"^/API/Other/Maintenance/HealthCheck$",
                        },
                    };
                    runPersistent = initializationInformation.ApplicationConstants.Environment is not Development && initializationInformation.ApplicationConstants.ExecutionMode is RunProgram;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration = new DatabasePersistenceConfiguration()
                    {
                        DatabaseType = initializationInformation.CommandlineParameter.InitialDatabaseType,
                        DatabaseConnectionString = initializationInformation.CommandlineParameter.InitialDatabaseType ?? "[insert database-connectionstring here and set InitialDatabaseType accordingly]",
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration = GRYLogConfiguration.GetCommonConfiguration(AbstractFilePath.FromString("./AuditLog.log"), true);
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.CommonRoutesInformation = new CommonRoutesInformation()
                    {
                        ContactLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/Contact",
                        LicenseLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/License",
                        TermsOfServiceLink = $"https://information.{domain}/Products/{GeneralConstants.CodeUnitName}/TermsOfService"
                    };
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.AuthorizationConfiguration = new AutSRConfiguration();
                    runPersistent = initializationInformation.ApplicationConstants.Environment is not Development && initializationInformation.ApplicationConstants.ExecutionMode is RunProgram;
                    initializationInformation.InitialApplicationConfiguration.ApplicationSpecificConfiguration.HeaderServiceConfiguration = new HeaderServiceConfiguration();
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.HostAPISpecificationForInNonDevelopmentEnvironment = true;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Protocol = new HTTP();
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.Domain = domain;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePasswordHex = GeneralConstants.DevelopmentCertificatePasswordHex;
                    initializationInformation.InitialApplicationConfiguration.ServerConfiguration.DevelopmentCertificatePFXHex = GeneralConstants.DevelopmentCertificatePFXHex;
                };
                apiServerConfiguration.SetFunctionalInformationAction = (functionalInformation) =>
                {
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForLoggingMiddleware);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ConfigurationForDLoggingMiddleware);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration);
                    IGeneralLogger logger = functionalInformation.Logger;
                    IAuditLog auditLog = new AuditLog(functionalInformation.InitializationInformation.ApplicationConstants.ExecutionMode.Accept(new GetLoggerVisitor(functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.AuditLogConfiguration, functionalInformation.InitializationInformation.ApplicationConstants.GetLogFolder(), "AuditLog")));
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuditLog>(auditLog);
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IIdGenerator<ulong>, Services.IdGenerator>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITimeService, TimeService>();
                    bool useDatabase = functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType != null;
                    if (useDatabase)
                    {
                        logger.Log($"Run persistent using database \"{functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType}\".", LogLevel.Information);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, OpenDMSBackendPersistentAuthenticationService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<IPersistence>());
                        IGenericDatabaseInteractor genericDatabaseInteractor;
                        if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "PostgreSQL")
                        {
                            genericDatabaseInteractor = new PostgreSQLDatabaseInteractor();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, DatabasePostgreSQLPersistence>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IDatabaseManager, DatabaseManagerPostgreSQL>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderPostgreSQL>();
                            functionalInformation.WebApplicationBuilder.Services.AddDbContext<DatabaseContext>(options =>
                            {
                                string connectionString = functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseConnectionString;
                                Tools.ConnectToDatabaseWrapper(() =>
                                {
                                    options.UseNpgsql(connectionString, sqlOptions =>
                                    {
                                        sqlOptions.CommandTimeout(120);
                                    });
                                }, GeneralLogger.NoLog(), genericDatabaseInteractor.AdaptConnectionString(connectionString));
                            }, ServiceLifetime.Singleton);
                        }
                        else if (functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseType == "MariaDB")
                        {
                            genericDatabaseInteractor = new MariaDBDatabaseInteractor();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, DatabaseMariaDBPersistence>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<IDatabaseManager, DatabaseManagerMariaDB>();
                            functionalInformation.WebApplicationBuilder.Services.AddSingleton<ISQLProvider, SQLProviderMariaDB>();
                            functionalInformation.WebApplicationBuilder.Services.AddDbContext<DatabaseContext>(options =>
                            {
                                string connectionString = functionalInformation.PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DatabasePersistenceConfiguration.DatabaseConnectionString;
                                Tools.ConnectToDatabaseWrapper(() =>
                                {
                                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), sqlOptions =>
                                    {
                                        sqlOptions.CommandTimeout(120);
                                    });
                                }, GeneralLogger.NoLog(), genericDatabaseInteractor.AdaptConnectionString(connectionString));
                            }, ServiceLifetime.Singleton);
                        }
                        else
                        {
                            throw new NotSupportedException("Database not supported. For a list of supported databases see the documentation.");
                        }
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGenericDatabaseInteractor>(genericDatabaseInteractor);
                    }
                    else
                    {
                        logger.Log($"Run transient.", LogLevel.Information);
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IPersistence, TransientPersistence>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationService<Model.BusinessTypes.User>, OpenDMSBackendTransientAuthenticationService>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>, TransientAuthenticationServicePersistence>();
                        functionalInformation.WebApplicationBuilder.Services.AddSingleton<IAuthenticationServicePersistence<Model.BusinessTypes.User>>(sp => sp.GetRequiredService<ITransientAuthenticationServicePersistence<Model.BusinessTypes.User>>());
                    }
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IGeneralResourceLoader, Services.GeneralResourceLoader>();
                    functionalInformation.WebApplicationBuilder.Services.AddSingleton<IOCRService, OCRService>();
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
                };
                apiServerConfiguration.ConfigureWebApplication = (functionalInformationForWebApplication) =>
                {
                    IMetricsService metricsService = GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IMetricsService>());
                    functionalInformationForWebApplication.PreRun = () =>
                    {
                        //initialize
                        GUtilities.GetValue(functionalInformationForWebApplication.WebApplication.Services.GetService<IInitializationService<CommandlineParameter>>()).Initialize(apiServerConfiguration.CommandlineParameter);

                        //start background-services
                        metricsService.StartAsync();
                    };
                    functionalInformationForWebApplication.PostRun = () =>
                    {
                        metricsService.Stop().Wait();
                    };
                };
            });
        }
    }
}
