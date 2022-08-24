using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KafkaWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker1;

        public string topic = "test";

        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker1 = new BackgroundWorker();

            backgroundWorker1.WorkerReportsProgress = true;

            backgroundWorker1.WorkerSupportsCancellation = true;

            backgroundWorker1.DoWork += BackgroundWorker1_DoWork1;

            backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;

            topicName.Text = topic;
        }

        private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            consumetxt.AppendText(e.UserState.ToString());
        }

        private void BackgroundWorker1_DoWork1(object sender, DoWorkEventArgs e)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                 .AddIniFile("Config.Provider.ini")
                 .Build();

            configuration["group.id"] = "KafkaWPF";
            configuration["auto.offset.reset"] = "earliest";

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, b) =>
            {
                b.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            using (var consumer = new ConsumerBuilder<string, string>(
                configuration.AsEnumerable()).Build())
            {
                consumer.Subscribe(topic);
                

                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);

                        var str = $"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}\r\n";

                        backgroundWorker1.ReportProgress(0, str);


                        System.Threading.Thread.Sleep(2000);

                        if (backgroundWorker1.CancellationPending)
                        {
                            //display a message
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }
        }

        public MainWindow(string topic)
        {
            this.topic = topic;

        }

        
    
        private void Consume_Click(object sender, RoutedEventArgs e)
        {
            if(!backgroundWorker1.IsBusy)
            {
                //Display a message

                backgroundWorker1.RunWorkerAsync();
                Consumebtn.Content = "Stop";
            }
            else
            {
                backgroundWorker1.CancelAsync();

                Consumebtn.Content = "Consume";
            }



        }

        private void Produce_Click(object sender, RoutedEventArgs e)
        {
            
            IConfiguration configuration = new ConfigurationBuilder().AddIniFile("Config.Provider.ini").Build();

            string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
            string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

            using (var producer = new ProducerBuilder<string, string>(
                configuration.AsEnumerable()).Build())
            {
                var numProduced = 0;
                Random rnd = new Random();
                const int numMessages = 10;
                for (int i = 0; i < numMessages; ++i)
                {
                    var user = users[rnd.Next(users.Length)];
                    var item = items[rnd.Next(items.Length)];
                    var localTopic = topic;

                    producer.Produce(topic, new Message<string, string> { Key = user, Value = item },
                        (deliveryReport) =>
                        {
                            if (deliveryReport.Error.Code != ErrorCode.NoError)
                            {
                                producetxt.Text=$"Failed to deliver message: {deliveryReport.Error.Reason}";
                            }
                            else
                            {
                                //producetxt.Text=$"Produced event to topic {localTopic}: key = {user,-10} value = {item}";
                                numProduced += 1;
                            }
                        });
                }

                producer.Flush(TimeSpan.FromSeconds(10));
                producetxt.Text=$"{numProduced} messages were produced to topic {topic}";
            }
        
        }

       
    }
}
