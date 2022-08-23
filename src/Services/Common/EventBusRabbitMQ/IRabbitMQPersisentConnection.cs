using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBusRabbitMQ
{
    public interface IRabbitMQPersisentConnection: IDisposable
    {
        bool IsConnected { get; }  //Bağlantı olup olmadığını anlamamızı yarar
        bool TryConnect(); //Bağlantı başlar
        IModel CreateModel(); //QManagement işlemlerini oluşturcaz.

        //IModel varlıkların şekli, aralarındaki ilişki ve veritabaıyla nasıl eşleştiği hakkında
        // bilgi verir.
    }
}
