using System;
using Automatonymous;
// using MassTransit.RedisIntegration;//Redis
using MassTransit.MongoDbIntegration.Saga; //Mongo
using MongoDB.Bson.Serialization.Attributes; //Mongo

namespace Sample.Components.StateMachines
{
    public class OrderState :
        SagaStateMachineInstance,
        IVersionedSaga // Redis And Mongo Marten Doesnt need that 
    {
        [BsonId] 
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }

        public string CurrentState { get; set; }

        public string CustomerNumber { get; set; }

        public DateTime? Updated { get; set; }
        public DateTime? SubmitDate { get; set; }
    }
}