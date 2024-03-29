﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using MongoDB.Bson;
global using MongoDB.Bson.Serialization;
global using MongoDB.Driver;
global using Pulsar.BuildingBlocks.DDD.Abstractions;
global using System.Linq.Expressions;
global using Pulsar.BuildingBlocks.Emails.Abstractions;
global using MediatR;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Pulsar.BuildingBlocks.DDD.Mongo.Mapping;
global using Pulsar.BuildingBlocks.EventBus.Abstractions;
global using Pulsar.BuildingBlocks.UnitTests.Mocking.EventBus;
global using Pulsar.BuildingBlocks.UnitTests.Mocking.MongoDB;
global using System.Reflection;
global using Pulsar.BuildingBlocks.Utils;
global using Polly;
global using System.Collections.ObjectModel;
global using Pulsar.BuildingBlocks.FileSystem.Abstractions;
global using Pulsar.BuildingBlocks.UnitTests.Mocking.Emails;
global using Pulsar.BuildingBlocks.UnitTests.Mocking.FileSystem;