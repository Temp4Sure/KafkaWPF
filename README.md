# KafkaWPF
A WPF application using Kafka to produce and consume messages

In order to produce and Consume messages you will have to create a config file like Config.Providers.ini but name it however you like. The Config file must be 
in the bin\debug of the solutions directory. From there you'll need to copy your api key generated on confluence and paste it into the config file. Then in the MainWindow.xaml.cs in both the Produce onClick and the BackgroundWorker1_DoWork1 events you'll have to change this: IConfiguration configuration = new ConfigurationBuilder().AddIniFile("Name_of_Config_File").Build();

change the string variable "topic" at the top of the class to the topic associated with your api key. and enjoy.
