using Buttplug.Client;
using Buttplug.Client.Connectors.WebsocketConnector;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

class Program
{
    // CLEAN UP LATEER MAYBE

   


    static async Task Main(string[] args)
    {
        

        var bot = new Bot();
        bot.Start();
        

        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
        {
        }
    }
}

class Bot
{
    TwitchClient client;
    private ButtplugClient buttClient;
    
    public Bot()
    {
        
    }

    public async void Start()
    {
        Console.WriteLine("Press ESC to stop");
        buttClient = new ButtplugClient("WoW Haptic Feedback Client");

        var connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345/buttplug"));
        await buttClient.ConnectAsync(connector);
//

        // Start scanning for devices
        await buttClient.StartScanningAsync();
        await Task.Delay(5000); // Wait for a few seconds to allow devices to be discovered
        await buttClient.StopScanningAsync();

//        // Check if any devices are connected
        if (buttClient.Devices.Length > 0)
        {
            Console.WriteLine("Haptic device found.");

            buttClient.ScanningFinished += (a, b) => { Console.WriteLine("Scanning finished."); };
        }
        else
        {
            Console.WriteLine("No haptic devices found.");
            return;
        }
        
        ConnectionCredentials credentials = new ConnectionCredentials("justinfan1393", "");
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
        client.Initialize(credentials, "headsup");

        client.OnLog += Client_OnLog;
        client.OnJoinedChannel += Client_OnJoinedChannel;
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnWhisperReceived += Client_OnWhisperReceived;
        client.OnNewSubscriber += Client_OnNewSubscriber;
        client.OnConnected += Client_OnConnected;

        client.Connect();
    }
    
    private void Client_OnLog(object sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        // Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
        // client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!");
    }

    private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        var clientDevice = buttClient.Devices[0];
        await StartVibration(clientDevice);
    }
    
    async Task StartVibration(ButtplugClientDevice device)
    {
        Console.WriteLine($"Starting vibration for device: {device.Name}");
        await device.VibrateAsync(1.0f);
        await Task.Delay(500); // Duration of the vibration
        await device.VibrateAsync(0);
    }

    private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
    {
        if (e.WhisperMessage.Username == "my_friend")
            client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
    }

    private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
    {
        if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
            client.SendMessage(e.Channel,
                $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
        else
            client.SendMessage(e.Channel,
                $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
    }
}