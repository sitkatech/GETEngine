using AutoMapper;
using System;
using Olsson.GET.Common.DataContracts.Models;
using System.Collections.Generic;
using System.Linq;
using Olsson.GET.Common.DataContracts.Customers;
using Olsson.GET.Common.DataContracts.ReportTemplate;
using Olsson.GET.Common.DataContracts.Runs;
using Olsson.GET.Common.DataContracts.Scenarios;
using Olsson.GET.Common.Utilities;
using BaseflowTableProcessingConfiguration = Olsson.GET.Common.DataContracts.Models.BaseflowTableProcessingConfiguration;
using CustomerModelScenario = Olsson.GET.Common.DataContracts.Customers.CustomerModelScenario;
using Image = Olsson.GET.Common.DataContracts.Models.Image;
using Model = Olsson.GET.Common.DataContracts.Models.Model;
using ModelStressPeriodCustomStartDate = Olsson.GET.Common.DataContracts.Models.ModelStressPeriodCustomStartDate;
using Role = Olsson.GET.Common.DataContracts.Users.Role;
using Run = Olsson.GET.Common.DataContracts.Runs.Run;
using RunBucket = Olsson.GET.Common.DataContracts.Runs.RunBucket;
using Scenario = Olsson.GET.Common.DataContracts.Scenarios.Scenario;
using ScenarioFile = Olsson.GET.Common.DataContracts.Scenarios.ScenarioFile;
using User = Olsson.GET.Common.DataContracts.Users.User;
using vModelCountScenarioCountForCustomerID = Olsson.GET.Common.DataContracts.Customers.vModelCountScenarioCountForCustomerID;
using FileResourceInfo = Olsson.GET.Common.DataContracts.FileResource.FileResourceInfo;
using FileResourceData = Olsson.GET.Common.DataContracts.FileResource.FileResourceData;
using FileResourceMimeType = Olsson.GET.Common.DataContracts.FileResource.FileResourceMimeType;
using ModelDocumentationImage = Olsson.GET.Common.DataContracts.Models.ModelDocumentationImage;
using ScenarioDocumentationImage = Olsson.GET.Common.DataContracts.Scenarios.ScenarioDocumentationImage;
using RunStatus = Olsson.GET.Common.DataContracts.Runs.RunStatus;

namespace Olsson.GET.Accessors
{
    internal static class DTOMapper
    {
        static IMapper _mapper;
        private static IConfigurationProvider _config;

        public static IMapper Mapper => _mapper ?? (_mapper = Configuration.CreateMapper());

        public static IConfigurationProvider Configuration
        {
            get
            {
                if (_config == null)
                {
                    var config = new AutoMapper.MapperConfiguration(cfg =>
                    {
                        cfg.CreateMap<EntityFramework.Customer, CustomerDto>()
                            .ForMember(dest => dest.CustomerModelScenarios, opt => opt.MapFrom(src => src.CustomerModelScenarios)).ReverseMap();

                        cfg.CreateMap<EntityFramework.CustomerModelScenario, CustomerModelScenario>().ReverseMap();
                        cfg.CreateMap<EntityFramework.CustomerModelScenario, CustomerModelScenarioDto>()
                            .ForMember(dest => dest.CustomerName, opts => opts.MapFrom(src => src.Customer.CustomerName))
                            .ForMember(dest => dest.ModelName, opts => opts.MapFrom(src => src.Model.ModelName))
                            .ForMember(dest => dest.ScenarioName, opts => opts.MapFrom(src => src.Scenario.ScenarioName))
                            .ReverseMap();
                        //cfg.CreateMap<EntityFramework.CustomerModelScenario, CustomerModelScenario>()
                        //    .ForMember(destination => destination.CustomerName,
                        //        opt => opt.MapFrom(source => source.Customer.Name));

                        cfg.CreateMap<EntityFramework.Model, ModelSimpleDto>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Scenario, ScenarioSimpleDto>().ReverseMap();
                        cfg.CreateMap<EntityFramework.Run, RunSimpleDto>()
                            .ForMember(dest => dest.RunBuckets,
                                opts => opts.MapFrom(source => source.RunBucketRuns.Select(x => x.RunBucket)))
                            .ReverseMap();

                        cfg.CreateMap<EntityFramework.Scenario, Scenario>()
                            .ForMember(dest => dest.CustomerModelScenarios, opts => opts.Ignore()).ReverseMap();
                        cfg.CreateMap<EntityFramework.Scenario, Scenario>().ForMember(
                                destination => destination.InputControlType,
                                opt => opt.MapFrom(source =>
                                    Enum.GetName(typeof(InputControlType), source.InputControlType)))
                            .ForMember(dest => dest.Models,
                                opts => opts.MapFrom(src => src.ModelScenarios.Select(x => x.Model)));


                        cfg.CreateMap<EntityFramework.ScenarioFile, ScenarioFile>().ReverseMap();
                        cfg.CreateMap<EntityFramework.ScenarioDocumentationImage, ScenarioDocumentationImage>()
                            .ReverseMap();
                        cfg.CreateMap<EntityFramework.BaseflowTableProcessingConfiguration,
                            BaseflowTableProcessingConfiguration>().ReverseMap();
                        cfg.CreateMap<EntityFramework.ModelExecutable, ModelExecutable>()
                            .ReverseMap();

                        cfg.CreateMap<EntityFramework.ModelStressPeriodCustomStartDate,
                            ModelStressPeriodCustomStartDate>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Model, Model>()
                            .ForMember(dest => dest.Scenarios, opts => opts.MapFrom(src => src.ModelScenarios.Select(x => x.Scenario)))
                            .ForMember(dest => dest.ModelStressPeriodCustomStartDates, opts => opts.MapFrom(src => src.ModelStressPeriodCustomStartDates))
                            .ForMember(dest => dest.BaseflowTableProcessingConfiguration, opts => opts.MapFrom(src => src.BaseflowTableProcessingConfiguration))
                            .ForMember(dest => dest.ModelExecutables, opts => opts.MapFrom(src => src.ModelExecutables))
                            .ForMember(dest => dest.MapModelArea, opts => opts.MapFrom(src => src.ModelMapAreaBoundary != null ? src.ModelMapAreaBoundary.MapAreaBoundary : null))
                            .ForMember(dest => dest.InputZoneData, opts => opts.MapFrom(src => src.ModelInputZoneData != null ? src.ModelInputZoneData.InputZoneData : null))
                            .ForMember(dest => dest.OutputZoneData, opts => opts.MapFrom(src => src.ModelOutputZoneData != null ? src.ModelOutputZoneData.OutputZoneData : null))
                            .ReverseMap();
                        cfg.CreateMap<EntityFramework.ModelDocumentationImage, ModelDocumentationImage>()
                            .ReverseMap();

                        cfg.CreateMap<EntityFramework.User, User>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Image, Image>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Role, Role>().ReverseMap();

                        cfg.CreateMap<EntityFramework.RunStatus, RunStatus>().ReverseMap();

                        cfg.CreateMap<EntityFramework.Run, Run>()
                            .ForMember(dest => dest.RunBuckets,
                                opts => opts.MapFrom(source => source.RunBucketRuns.Select(x => x.RunBucket)))
                            .ReverseMap();
                        cfg.CreateMap<Run, EntityFramework.Run>()
                            .ForMember(dest => dest.User, opts => opts.Ignore())
                            .ForMember(dest => dest.Model, opts => opts.Ignore())
                            .ForMember(dest => dest.Scenario, opts => opts.Ignore());

                        cfg.CreateMap<EntityFramework.RunBucket, RunBucket>()
                            .ForMember(dest => dest.Runs, opts => opts.MapFrom(src => src.RunBucketRuns == null ? src.RunBucketRuns : new List<EntityFramework.RunBucketRun>()));

                        cfg.CreateMap<RunBucket, EntityFramework.RunBucket>()
                            .ForMember(dest => dest.RunBucketRuns, opts => opts.Ignore());

                        cfg.CreateMap<EntityFramework.vModelAndScenarioCountForCustomerID,
                            vModelCountScenarioCountForCustomerID>().ReverseMap();

                        cfg.CreateMap<EntityFramework.FileResourceInfo, FileResourceInfo>().ReverseMap();
                        cfg.CreateMap<EntityFramework.FileResourceData, FileResourceData>().ReverseMap();
                        cfg.CreateMap<EntityFramework.FileResourceMimeType, FileResourceMimeType>().ReverseMap();
                        cfg.CreateMap<EntityFramework.ReportTemplate, Common.DataContracts.ReportTemplate.ReportTemplate>().ReverseMap();
                        cfg.CreateMap<EntityFramework.ReportTemplate, Common.DataContracts.ReportTemplate.ReportTemplate
                            >()
                            .ForMember(dest => dest.ReportTemplateModel,
                                opts => opts.MapFrom(source =>
                                    (ReportTemplateModelEnum)source.ReportTemplateModelID))
                            .ForMember(dest => dest.ReportTemplateModelType,
                                opts => opts.MapFrom(source =>
                                    (ReportTemplateModelTypeEnum)source.ReportTemplateModelTypeID));

                    });
                    _config = config;
                }
                return _config;
            }
        }
    }
}
