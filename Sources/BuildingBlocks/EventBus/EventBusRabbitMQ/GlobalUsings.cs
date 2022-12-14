global using Polly;
global using Polly.Retry;
global using RabbitMQ.Client;
global using RabbitMQ.Client.Events;
global using RabbitMQ.Client.Exceptions;
global using System;
global using System.IO;
global using System.Net.Sockets;
global using Microsoft.Extensions.DependencyInjection;
global using Pulsar.BuildingBlocks.EventBus;
global using Pulsar.BuildingBlocks.EventBus.Abstractions;
global using Pulsar.BuildingBlocks.EventBus.Events;
global using Pulsar.BuildingBlocks.EventBus.Extensions;
global using System.Text;
global using System.Threading.Tasks;
global using System.Text.Json;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;