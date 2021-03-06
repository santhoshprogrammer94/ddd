﻿using Autofac;
using CompanyName.MyMeetings.BuildingBlocks.Application;
using CompanyName.MyMeetings.BuildingBlocks.Infrastructure.Emails;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.DataAccess;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Domain;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Email;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.EventsBus;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Logging;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Mediation;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Processing;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Processing.Outbox;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Quartz;
using CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration.Security;
using Serilog;
using Serilog.AspNetCore;

namespace CompanyName.MyMeetings.Modules.UserAccess.Application.Configuration
{
    public class UserAccessStartup
    {
        private static IContainer _container;

        public static void Initialize(string connectionString, 
            IExecutionContextAccessor executionContextAccessor,
            ILogger logger,
            EmailsConfiguration emailsConfiguration,
            string textEncryptionKey)
        {
            var moduleLogger = logger.ForContext("Module", "UserAccess");

            ConfigureCompositionRoot(connectionString,
                executionContextAccessor,
                logger,
                emailsConfiguration,
                textEncryptionKey);
            
            QuartzStartup.Initialize(moduleLogger);

            EventsBusStartup.Initialize(moduleLogger);
        }

        private static void ConfigureCompositionRoot(
            string connectionString, 
            IExecutionContextAccessor executionContextAccessor, 
            ILogger logger,
            EmailsConfiguration emailsConfiguration,
            string textEncryptionKey)
        {
            var containerBuilder = new ContainerBuilder();
          
            containerBuilder.RegisterModule(new LoggingModule(logger.ForContext("Module", "UserAccess")));
            
            var loggerFactory = new SerilogLoggerFactory(logger);
            containerBuilder.RegisterModule(new DataAccessModule(connectionString, loggerFactory));
            containerBuilder.RegisterModule(new DomainModule());
            containerBuilder.RegisterModule(new ProcessingModule());
            containerBuilder.RegisterModule(new EventsBusModule());
            containerBuilder.RegisterModule(new MediatorModule());
            containerBuilder.RegisterModule(new OutboxModule());
            containerBuilder.RegisterModule(new QuartzModule()); 
            containerBuilder.RegisterModule(new EmailModule(emailsConfiguration)); 
            containerBuilder.RegisterModule(new SecurityModule(textEncryptionKey)); 

            containerBuilder.RegisterInstance(executionContextAccessor);

            _container = containerBuilder.Build();

            UserAccessCompositionRoot.SetContainer(_container);
        }
    }
}