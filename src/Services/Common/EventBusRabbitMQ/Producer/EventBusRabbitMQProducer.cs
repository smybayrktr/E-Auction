using EventBusRabbitMQ.Events.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ.Producer
{
    public class EventBusRabbitMQProducer 
        //Bu sınıf rabbitMQ ya bağlanıp istenilen eventi istenilen queue ye publish edecek arayüz olması
    {
        //Producer üzerinden event yollayabilmek için RabbitMQ ya bağlanmamız lazım
        private readonly IRabbitMQPersisentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQProducer> _logger;
        private readonly int _retryCount;

        public EventBusRabbitMQProducer(IRabbitMQPersisentConnection persistentConnection,
            ILogger<EventBusRabbitMQProducer> logger, int retryCount = 5)
        {
            _persistentConnection = persistentConnection;
            _logger = logger;
            _retryCount = retryCount;
        }

        public void Publish(string queueName, IEvent @event)
        {
            //Şu Queue ye şu eventi bırak şeklinde yapımız olacak
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect(); //Bağlantı var mı onu kontrol ettik bağlantı yoksa bağlanmaya zorladık. 
            }
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                });
            using(var channel = _persistentConnection.CreateModel())  
            {
                //Channel üzerinden queue deklare ettik. 
                //isim verdik, inmemory olarak kullan dedik, tek bağlantıda çalışmasını sağlasın dedik, otomatik silinmeyi kapattık 
                channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false ,arguments: null);
                var message = JsonConvert.SerializeObject(@event); //Eventi jsona dönüştürüyoruz.
                var body = Encoding.UTF8.GetBytes(message); //byte hale getirdik.
                policy.Execute(()=>
                {//Queue ye gönderme işlemi yapıyoruz. Anlık sorun olursa sürekli deneme yapıyor. 
                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;

                    channel.ConfirmSelect();
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body
                        );
                    channel.WaitForConfirmsOrDie();
                    channel.BasicAcks += (sender, eventArgs) =>
                    {
                        Console.WriteLine("Sent RabbitMQ");
                    };
                
                });
            }
        }
    }
}

