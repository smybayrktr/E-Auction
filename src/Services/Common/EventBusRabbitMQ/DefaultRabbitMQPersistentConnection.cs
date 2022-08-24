using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersisentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        //RabbitMQ hostuna bağlanmak için kullanılır.
        private IConnection _connection;
        private readonly int _retryCount;
        //Retray pattern kullanıldı böylece; Yapılan bir işlemin herhangi bir sebepten dolayı askıya uğraması
        //durumunda, o işlemi tekrar edebilmek için Retry Pattern’i kullanabiliriz.
        //Haliyle client’ın gönderdiği request’in server’da ki herhangi bir sebepten dolayı kaynaklanan
        //hatayla karşılaşması neticesinde Retry Pattern uygulanarak belirli periyotlarda bu istek tekrar
        //edilebilir ve meydana gelen hatanın getirdiği zaafiyet mümkün mertebe ortadan kaldırılmaya çalışılabilir.
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
        private bool _disposed;

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory,
            int retryCount, ILogger<DefaultRabbitMQPersistentConnection> logger)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
            _logger = logger;
        }

        public bool IsConnected {
            get{
                return _connection != null && _connection.IsOpen && !_disposed;
                //Bağlantı null değilse, bağlantı açıksa ve dispose edilmemişse bağlantı vardır.
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if(_disposed) return; //nesne dispose olmuşsa geri döndürüyoruz.
            _disposed = true; //Değilse dispose u true yap.

            try
            {
                _connection.Dispose(); //Kurduğumuz bağlantıyı dispose eder.
            }

            catch(IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");
            //Bir policy tanımlayıp bağlantı sağlıycaz. Bağlantı sağlanmayınca hata vermesin tekrar tekrar
            //bağlanmaya çalışsın gibi bir yapı oluşturduk.
            
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),(ex, time) =>
            {
                _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
            }
            //SocketException veya BrokerUnreachableException hataları aldığında benim verdiğim 
            //koşullarda bekle ve tekrar dene.
            );
            policy.Execute(() => { 
                _connection = _connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutDown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{Hostname}' and is subscribed to failure events",_connection);
                return true;
            }

            else
            {
                _logger.LogCritical("Fatal Error: RabbitMQ connections could not be created and opend");
                return false;
            }
        }
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs args)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect");
            TryConnect();
        }

        private void OnConnectionShutDown(object sender, ShutdownEventArgs args)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect");
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs args)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect");
            TryConnect();
        }


    }
}
