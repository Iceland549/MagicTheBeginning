﻿using GameMicroservice.Application.Interfaces;
using GameMicroservice.Application.UseCases;
using GameMicroservice.Infrastructure;
using GameMicroservice.Infrastructure.AI;
using GameMicroservice.Infrastructure.Config;
using GameMicroservice.Infrastructure.Mapping;
using GameMicroservice.Infrastructure.Persistence;
using GameMicroservice.Infrastructure.Persistence.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text;

namespace GameMicroservice.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameMicroserviceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Config binding
            services.Configure<MongoDbConfig>(
              configuration.GetSection("Mongo"));

            // MongoClient singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                return new MongoClient(cfg.ConnectionString);
            });

            // IMongoDatabase singleton
            services.AddSingleton(sp => {
                var cfg = sp.GetRequiredService<IOptions<MongoDbConfig>>().Value;
                var client = sp.GetRequiredService<MongoClient>();
                return client.GetDatabase(cfg.DatabaseName);
            });

            // Repository MongoDB pour les sessions de jeu
            services.AddScoped<IGameSessionRepository, MongoGameSessionRepository>();

            // Clients HTTP pour les cartes et les decks
            services.AddHttpClient<ICardClient, CardHttpClient>();
            services.AddHttpClient<IDeckClient, DeckHttpClient>();

            // Moteur de règles du jeu
            services.AddScoped<IGameRulesEngine, GameRulesEngine>();

            // Moteur d'IA
            services.AddScoped<IAIEngine, RandomAIEngine>();

            // AutoMapper pour les mappings entre entités et DTOs
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Use Cases
            services.AddScoped<StartGameUseCase>();
            services.AddScoped<PlayCardUseCase>();
            services.AddScoped<GetGameStateUseCase>();
            services.AddScoped<AIPlayTurnUseCase>();

            return services;
        }
    }
}