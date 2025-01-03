﻿global using Pulsar.Services.ApiRegistry;
global using Pulsar.Services.Facility.API.Application.BaseTypes;
global using Pulsar.Services.Facility.Infrastructure.Repositories;
global using Pulsar.Services.Shared.API.Utils;
global using System.Text.Json.Serialization;
global using Pulsar.BuildingBlocks.DDD.Mongo;
global using Pulsar.BuildingBlocks.RedisCaching;
global using Pulsar.BuildingBlocks.DDD.Mongo.Implementations;
global using Pulsar.BuildingBlocks.Utils;
global using Microsoft.AspNetCore.Authorization;
global using Pulsar.Services.Shared.API.Authorization;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using Pulsar.BuildingBlocks.EventBus.Extensions;
global using Pulsar.Services.Shared.API.Filters;
global using Pulsar.Services.Shared.API.Services;
global using Constants = Pulsar.Services.Facility.Domain.Constants;
global using MongoDB.Driver;
global using Pulsar.BuildingBlocks.Caching;
global using Pulsar.Services.Facility.Contracts.Queries;
global using Pulsar.Services.Facility.API.Application.Queries;
global using Pulsar.BuildingBlocks.Caching.Abstractions;
global using Pulsar.Services.Facility.API.Utils;
global using MediatR;
global using Microsoft.AspNetCore.Mvc;