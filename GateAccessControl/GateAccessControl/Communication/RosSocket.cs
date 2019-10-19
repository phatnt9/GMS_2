/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

<http://www.apache.org/licenses/LICENSE-2.0>.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
//Install-Package WebSocketSharp -Version 1.0.3-rc11

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using WebSocketSharp;

namespace GateAccessControl
{
    public class RosSocket : INotifyPropertyChanged
    {
        public enum ConnectionStatus
        {
            CON_OK = 0,
            CON_FAILED,
            CON_CLOSED
        }

        #region Public

        private bool IsDisposed = false;
        protected bool onBinding = true;
        public int timeOutReConnection = 0;

        protected virtual void OnClosedEvent(object sender, CloseEventArgs e)
        {
            checkAlive.AutoReset = true;
            checkAlive.Enabled = true;
        }

        protected virtual void OnOpenedEvent()
        {
            checkAlive.AutoReset = true;
            checkAlive.Enabled = true;
        }

        public String Url { get; set; }

        public RosSocket()
        {
            checkAlive = new System.Timers.Timer();
            checkAlive.Interval = 5000;
            checkAlive.Elapsed += CheckConnection;
        }

        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            if (webSocket != null)
            {
                if (!webSocket.IsAlive)
                {
                    try
                    {
                        if (!IsDisposed)
                        {
                            Close();
                            webSocket.Connect();
                        }

                    }
                    catch { }
                }
            }
        }

        public void Start(string url)
        {
            if (webSocket == null)
            {
                new Thread(() =>
                {
                    this.Url = url;
                    IsDisposed = false;
                    webSocket = new WebSocket(url);
                    webSocket.OnClose += (sender, e) => OnClosedEvent((WebSocket)sender, e);
                    webSocket.OnOpen += (sender, e) => OnOpenedEvent();
                    webSocket.OnMessage += (sender, e) => RecievedOperation((WebSocket)sender, e);
                    webSocket.WaitTime = TimeSpan.FromMilliseconds(1000);
                    webSocket.Connect();
                }).Start();
            }
        }

        public virtual void Dispose()
        {
            try
            {
                if (webSocket != null)
                {
                    IsDisposed = true;
                    //webSocket.OnMessage -= (sender, e) => recievedOperation((WebSocket)sender, e);
                    webSocket.OnClose -= (sender, e) => OnClosedEvent((WebSocket)sender, e);
                    //webSocket.OnOpen -= (sender, e) => OnOpenedEvent();
                    if (webSocket.IsAlive)
                    {
                        Close();
                    }

                    webSocket = null;
                }
            }
            catch
            {
                Console.WriteLine("Error Dispose in RosSocket !!");
            }
        }

        public void Close()
        {
            while (publishers.Count > 0)
            {
                Unadvertize(publishers.First().Key);
            }

            while (subscribers.Count > 0)
            {
                Unsubscribe(subscribers.First().Key);
            }

            try
            {
                webSocket.Close();
            }
            catch { }
        }

        public delegate void ServiceHandler(object obj);

        public delegate void MessageHandler(Message message);

        public int Advertise(string topic, string type)
        {
            int id = GenerateId();
            publishers.Add(id, new Publisher(topic));

            SendOperation(new Adverisement(id, topic, type));
            return id;
        }

        public void Publish(int id, Message msg)
        {
            if (publishers.TryGetValue(id, out Publisher publisher))
            {
                SendOperation(new Publication(id, publisher.Topic, msg));
            }
        }

        public void Unadvertize(int id)
        {
            SendOperation(new Unadverisement(id, publishers[id].Topic));
            publishers.Remove(id);
        }

        public int Subscribe(string topic, string rosMessageType, MessageHandler messageHandler, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            Type messageType = MessageTypes.MessageType(rosMessageType);
            if (messageType == null)
            {
                return 0;
            }

            int id = GenerateId();
            subscribers.Add(id, new Subscriber(topic, messageType, messageHandler));
            SendOperation(new Subscription(id, topic, rosMessageType, throttle_rate, queue_length, fragment_size, compression));
            return id;
        }

        public int Subscribe(string topic, Type messageType, MessageHandler messageHandler, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            string rosMessageType = MessageTypes.RosMessageType(messageType);
            if (rosMessageType == null)
            {
                return 0;
            }

            return Subscribe(topic, messageType, messageHandler, throttle_rate, queue_length, fragment_size, compression);
        }

        public void Unsubscribe(int id)
        {
            SendOperation(new Unsubscription(id, subscribers[id].topic));
            subscribers.Remove(id);
        }

        public int CallService(string service, Type objectType, ServiceHandler serviceHandler, object args = null)
        {
            int id = GenerateId();
            serviceCallers.Add(id, new ServiceCaller(service, objectType, serviceHandler));

            SendOperation(new ServiceCall(id, service, args));
            return id;
        }

        #endregion Public

        #region Private

        internal struct Publisher
        {
            internal string Topic;

            internal Publisher(string topic)
            {
                Topic = topic;
            }
        }

        internal struct Subscriber
        {
            internal string topic;
            internal Type messageType;
            internal MessageHandler messageHandler;

            internal Subscriber(string Topic, Type MessageType, MessageHandler MessageHandler)
            {
                topic = Topic;
                messageType = MessageType;
                messageHandler = MessageHandler;
            }
        }

        internal struct ServiceCaller
        {
            internal string service;
            internal Type objectType;
            internal ServiceHandler serviceHandler;

            internal ServiceCaller(string Service, Type ObjectType, ServiceHandler ServiceHandler)
            {
                service = Service;
                objectType = ObjectType;
                serviceHandler = ServiceHandler;
            }
        }

        private WebSocket _webSocket;
        public WebSocket webSocket
        {
            get
            {
                return _webSocket;
            }
            set
            {
                _webSocket = value;
                OnPropertyChanged("webSocket");
            }
        }
        private Dictionary<int, Publisher> publishers = new Dictionary<int, Publisher>();
        private Dictionary<int, Subscriber> subscribers = new Dictionary<int, Subscriber>();
        private Dictionary<int, ServiceCaller> serviceCallers = new Dictionary<int, ServiceCaller>();
        private System.Timers.Timer checkAlive;

        private void RecievedOperation(object sender, MessageEventArgs e)
        {
            JObject operation = Deserialize(e.RawData);

#if DEBUG
            //  Console.WriteLine("Recieved " + operation.GetOperation());
            //  Console.WriteLine(JsonConvert.SerializeObject(operation, Formatting.Indented));
#endif

            switch (operation.GetOperation())
            {
                case "publish":
                {
                    RecievedPublish(operation, e.RawData);
                    return;
                }
                case "service_response":
                {
                    RecievedServiceResponse(operation, e.RawData);
                    return;
                }
            }
        }

        private void RecievedServiceResponse(JObject serviceResponse, byte[] rawData)
        {
            bool foundById = serviceCallers.TryGetValue(serviceResponse.GetServiceId(), out ServiceCaller serviceCaller);

            if (!foundById)
            {
                serviceCaller = serviceCallers.Values.FirstOrDefault(x => x.service.Equals(serviceResponse.GetService()));
            }

            JObject jObject = serviceResponse.GetValues();
            Type type = serviceCaller.objectType;
            if (type != null)
            {
                serviceCaller.serviceHandler?.Invoke(jObject.ToObject(type));
            }
            else
            {
                serviceCaller.serviceHandler?.Invoke(jObject);
            }
        }

        private void RecievedPublish(JObject publication, byte[] rawData)
        {
            bool foundById = subscribers.TryGetValue(publication.GetServiceId(), out Subscriber subscriber);

            if (!foundById)
            {
                subscriber = subscribers.Values.FirstOrDefault(x => x.topic.Equals(publication.GetTopic()));
            }

            subscriber.messageHandler?.Invoke((Message)publication.GetMessage().ToObject(subscriber.messageType));
        }

        private void SendOperation(Operation operation)
        {
#if DEBUG
            //     Console.WriteLine(JsonConvert.SerializeObject(operation, Formatting.Indented));
#endif
            try
            {
                webSocket.SendAsync(Serialize(operation), null);
            }
            catch { }
        }

        public static byte[] Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] buffer = Encoding.ASCII.GetBytes(json);
            int I = json.Length;
            int J = buffer.Length;
            return buffer;
        }

        public static JObject Deserialize(byte[] buffer)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            int I = ascii.Length;
            int J = buffer.Length;
            return JsonConvert.DeserializeObject<JObject>(ascii);
        }

        private static int GenerateId()
        {
            return Guid.NewGuid().GetHashCode();
        }

        #endregion Private

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
