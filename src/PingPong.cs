// Copyright 2021 Siemens AG 
// This file is subject to the terms and conditions of the MIT License.   
// See LICENSE file in the top-level directory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using uPLibrary.Networking.M2Mqtt;          // including M2Mqtt library
using uPLibrary.Networking.M2Mqtt.Messages; // including M2Mqtt library
using Newtonsoft.Json;                      // including Json library


namespace PingPong
{
    // define json structure (PropertyNames must match name value pairs of JSON array)
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class JsonParams
    {
        [JsonProperty(PropertyName = "MQTT_USER")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "MQTT_PASSWORD")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "MQTT_IP")]
        public string BrokerIP { get; set; }
        
        [JsonProperty(PropertyName = "TOPIC_1")]
        public string Topic1 { get; set; }

        [JsonProperty(PropertyName = "TOPIC_2")]
        public string Topic2 { get; set; }
    }

    class Program
    {
        public static MqttClient client;
        public static string clientId;
        public static bool quit = false;

        public static string mqtt_broker;
        public static string mqtt_user;
        public static string mqtt_pw;
		public static string mqtt_topic1;
        public static string mqtt_topic2;

        public static string config_file = "/cfg-data/mqtt-config.json";

   
        public static JsonParams ReadJsonParams(string path)
        {
            JsonParams parameters = new JsonParams();

		try
            {	Console.WriteLine("Start reading config params from " + path + ":");
                string text = File.ReadAllText(path);
                //Console.WriteLine(text);
                parameters = JsonConvert.DeserializeObject<JsonParams>(text);

                Console.WriteLine("user= " + parameters.User);
                Console.WriteLine("password= " + parameters.Password);
                Console.WriteLine("broker ip= " + parameters.BrokerIP);
                Console.WriteLine("topic1= " + parameters.Topic1);
                Console.WriteLine("topic2= " + parameters.Topic2);
            }
            catch(Exception ex)
            {
		Console.WriteLine("Error while reading config file: " + ex);
            }
				             
            return parameters;
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting pingpong app...");

                // If a config file exists, get params from here (mqtt-config.json)
		if(File.Exists(config_file))
		{
                	Console.WriteLine("Reading parameters from configuration file");
			JsonParams configParams = ReadJsonParams(config_file);
				
		if(configParams != null)
		{
                        mqtt_broker = configParams.BrokerIP;
                        mqtt_user = configParams.User;
						mqtt_pw = configParams.Password;
						mqtt_topic1 = configParams.Topic1;
						mqtt_topic2 = configParams.Topic2;
					}
				}
                // Get params from environment variables in docker-compose.yml
				else
				{
					Console.WriteLine("Reading parameters from environment variables");

                    mqtt_broker = Environment.GetEnvironmentVariable("MQTT_IP");
                    mqtt_user = Environment.GetEnvironmentVariable("MQTT_USER");
                    mqtt_pw = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
                    mqtt_topic1 = Environment.GetEnvironmentVariable("TOPIC_1");
                    mqtt_topic2 = Environment.GetEnvironmentVariable("TOPIC_2");
				}

                Console.WriteLine("MQTT_IP =" + mqtt_broker);
                Console.WriteLine("MQTT_USER =" + mqtt_user);
                Console.WriteLine("MQTT_PASSWORD =" + mqtt_pw);
                Console.WriteLine("TOPIC_1 =" + mqtt_topic1);
                Console.WriteLine("TOPIC_2 =" + mqtt_topic2);

                if(mqtt_broker == null)
                {
                    Console.WriteLine("Parameter mqtt_broker not available!");
                    return;
                }

                if(mqtt_user == null)
                {
                    Console.WriteLine("Parameter mqtt_user not available!");
                    return;
                }
                if(mqtt_pw == null)
                {
                    Console.WriteLine("Parameter mqtt_pw not available!");
                    return;
                }
                if(mqtt_topic1 == null)
                {
                    Console.WriteLine("Parameter mqtt_topic1 not available!");
                    return;
                }
                if(mqtt_topic2 == null)
                {
                    Console.WriteLine("Parameter mqtt_topic2 not available!");
                    return;
                }
                 				
                Connect();
                Subscribe();

                while(!quit)
                {
                    Thread.Sleep(1000);
                };

                Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
  

        static void Connect()
        {
			Console.WriteLine("Create client instance");

            // create mqtt client instance (host name OR IP address work)
            client = new MqttClient(mqtt_broker);
			
            // register to message received
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

			Console.WriteLine("Connect to broker");
			
            // use a unique client id each time we start the application
            clientId = Guid.NewGuid().ToString();
		
            // connect to the broker
            //client.Connect(clientId);

            // connect with authentication
            client.Connect(clientId, mqtt_user, mqtt_pw);

            if (client.IsConnected == true)
                System.Console.WriteLine("Client successfully connected to broker!");
            else
                System.Console.WriteLine("ERROR - Client could not connect to broker!");
        }

        static void Subscribe()
        {
            client.Subscribe(new string[] { mqtt_topic1}, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            Console.WriteLine("Subscribed " + mqtt_topic1 + "...\n");
        }

        static void Publish(string message)
        {
            client.Publish(mqtt_topic2, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            Console.WriteLine("Send " + message + "\n");
        }

        static bool Disconnect()
        {
            Console.WriteLine("Disconnecting client");
            client.Disconnect();

            Console.WriteLine("Ending now...");
            Environment.Exit(-1);         

            return true;
        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // handle message received 
            //Console.WriteLine("Received message on topic \"" + e.Topic + "\": " + Encoding.UTF8.GetString(e.Message));

            // watch topic1
            if ((Encoding.UTF8.GetString(e.Message) == "Ping") || (Encoding.UTF8.GetString(e.Message) == "ping"))
            {
                Console.WriteLine("Received Ping");
                Publish("Pong");
            }
            else if ((Encoding.UTF8.GetString(e.Message) == "Pong") || (Encoding.UTF8.GetString(e.Message) == "pong"))
            {
                Console.WriteLine("Received Pong");
                Publish("Ping");
            }
            else if ((Encoding.UTF8.GetString(e.Message) == "quit") || (Encoding.UTF8.GetString(e.Message) == "Quit"))
            {
                //Console.WriteLine("Received Quit");
                //Publish("Ending now...");
                //quit = true;
            }
            else 
            {
                Publish("I only answer to Ping or Pong");
            }
        }
    }
}
