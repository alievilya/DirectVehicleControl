using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using UGCS.Sdk.Protocol;
using UGCS.Sdk.Protocol.Encoding;
using UGCS.Sdk.Tasks;
using TcpClient = UGCS.Sdk.Protocol.TcpClient;
using TcpClientt = System.Net.Sockets;

namespace DirectVehicleControl
{
    class Program
    {
        static void Main(string[] args)
        {
            //Connect
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect("localhost", 3334);
            MessageSender messageSender = new MessageSender(tcpClient.Session);
            MessageReceiver messageReceiver = new MessageReceiver(tcpClient.Session);
            MessageExecutor messageExecutor =
                new MessageExecutor(messageSender, messageReceiver, new InstantTaskScheduler());
            messageExecutor.Configuration.DefaultTimeout = 10000;
            var notificationListener = new NotificationListener();
            messageReceiver.AddListener(-1, notificationListener);

            //auth
            AuthorizeHciRequest request = new AuthorizeHciRequest();
            request.ClientId = -1;
            request.Locale = "en-US";
            var future = messageExecutor.Submit<AuthorizeHciResponse>(request);
            future.Wait();
            AuthorizeHciResponse AuthorizeHciResponse = future.Value;
            int clientId = AuthorizeHciResponse.ClientId;
            System.Console.WriteLine("AuthorizeHciResponse precessed");

            //login
            LoginRequest loginRequest = new LoginRequest();
            loginRequest.UserLogin = "admin";
            loginRequest.UserPassword = "admin";
            loginRequest.ClientId = clientId;
            var loginResponcetask = messageExecutor.Submit<LoginResponse>(loginRequest);
            loginResponcetask.Wait();

            // Id of the emu-copter is 2
            var vehicleToControl = new Vehicle {Id = 2};

            TcpClientt.TcpListener server = new TcpClientt.TcpListener(IPAddress.Any, 8080);
            server.Start(); // run server
            byte[] ok = new byte[100]; 
            ok = Encoding.Default.GetBytes("ok"); 
            while (true) // бесконечный цикл обслуживания клиентов
            {
                TcpClientt.TcpClient client = server.AcceptTcpClient(); // ожидаем подключение клиента
                TcpClientt.NetworkStream ns = client.GetStream(); // для получения и отправки сообщений
                // byte[] ok = new byte[100]; 
                // ok = Encoding.Default.GetBytes("ok"); TODO Connected
                //
                // ns.Write(ok, 0, ok.Length);
                while (client.Connected) // пока клиент подключен, ждем приходящие сообщения
                {
                    byte[] msg = new byte[100]; // готовим место для принятия сообщения
                    int count = ns.Read(msg, 0, msg.Length); // читаем сообщение от клиента
                    Console.Write(Encoding.Default.GetString(msg, 0,
                        count)); // выводим на экран полученное сообщение в виде строки
                    string all_message = Encoding.Default.GetString(msg);
                    string result = all_message.Substring(0, count-1);
                    var command_name = result.ToString().Split(":")[0];
                    
                    

                    switch (command_name)
                    {
                        case "takeoff_command":
                        {
                            Console.Write("got command: {0}", command_name);
                            
                            SendCommandRequest takeoff = new SendCommandRequest
                            {
                                ClientId = clientId,
                                Command = new UGCS.Sdk.Protocol.Encoding.Command
                                {
                                    Code = "takeoff_command",
                                    Subsystem = Subsystem.S_FLIGHT_CONTROLLER,
                                    Silent = true,
                                    ResultIndifferent = true
                                }
                            };
                            takeoff.Vehicles.Add(vehicleToControl);
                            var takeoffCmd = messageExecutor.Submit<SendCommandResponse>(takeoff);
                            takeoffCmd.Wait();
                            Thread.Sleep(5000);
                            ns.Write(ok, 0, ok.Length);
                            break;
                        }
                        case "direct_vehicle_control":
                        {
                            Console.Write("got command: {0}", command_name);
                            var command_args = result.ToString().Split(":")[1];
                            Console.Write("args of command: {0}", command_args);
                            // Vehicle control in joystick mode
                            SendCommandRequest vehicleJoystickControl = new SendCommandRequest
                            {
                                ClientId = clientId,
                                Command = new UGCS.Sdk.Protocol.Encoding.Command
                                {
                                    Code = "direct_vehicle_control",
                                    Subsystem = Subsystem.S_FLIGHT_CONTROLLER,
                                    Silent = true,
                                    ResultIndifferent = true
                                }
                            };
                            // ns.Write(ok, 0, ok.Length); // отправляем сообщени
                            // byte[] income = new byte[7]; // готовим место для принятия сообщения
                            // int counter = ns.Read(income, 0, income.Length); // читаем сообщение от клиента
                            // Console.Write(Encoding.Default.GetString(income, 0,
                            //     counter)); // выводим на экран полученное сообщение в виде строки
                            // //List of current joystick values to send to vehicle.
                            
                            
                            vehicleJoystickControl.Vehicles.Add(vehicleToControl);
                            
                            List<CommandArgument> listJoystickCommands = new List<CommandArgument>();
                            var direction_command = command_args.ToString().Split(",")[0];
                            string command_value_str = command_args.ToString().Split(",")[1];
                            double command_value = double.Parse(command_value_str, System.Globalization.CultureInfo.InvariantCulture);
                            
                            switch (direction_command)
                            {
                                case "roll":
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "roll",
                                        Value = new Value() {DoubleValue = command_value}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "pitch",
                                        Value = new Value() {DoubleValue = 0}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "yaw",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "throttle",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    break;
                                
                                case "pitch":
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "pitch",
                                        Value = new Value() {DoubleValue = command_value}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "roll",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "yaw",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "throttle",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    break;
                                
                                case "throttle":
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "throttle",
                                        Value = new Value() {DoubleValue = command_value}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "pitch",
                                        Value = new Value() {DoubleValue = command_value}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "roll",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "yaw",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    break;
                                
                                case "yaw":
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "yaw",
                                        Value = new Value() {DoubleValue = command_value}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "pitch",
                                        Value = new Value() {DoubleValue = 0}
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "roll",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    listJoystickCommands.Add(new CommandArgument
                                    {
                                        Code = "throttle",
                                        Value = new Value() { DoubleValue = 0 }
                                    });
                                    break;
                                
                                case "exit":
                                    break;
                            }

                            
                            
                            vehicleJoystickControl.Command.Arguments.AddRange(listJoystickCommands);
                            var sendJoystickCommandResponse =
                                messageExecutor.Submit<SendCommandResponse>(vehicleJoystickControl);
                            sendJoystickCommandResponse.Wait();
                            System.Console.WriteLine("Was sent {0}", command_value);
                            
                            Thread.Sleep(2000);
                            ns.Write(ok, 0, ok.Length);
                            break;
                        }
                        case "direct_payload_control":
                        {
                            break;
                        }
                        case "landcom":
                        {
                            SendCommandRequest land = new SendCommandRequest
                            {
                                ClientId = clientId,
                                Command = new UGCS.Sdk.Protocol.Encoding.Command
                                {
                                    Code = "land_command",
                                    Subsystem = Subsystem.S_FLIGHT_CONTROLLER,
                                    Silent = false,
                                    ResultIndifferent = false
                                }
                            };
                            land.Vehicles.Add(vehicleToControl);
                            var landCmd = messageExecutor.Submit<SendCommandResponse>(land);
                            landCmd.Wait();
                            Thread.Sleep(5000);
                            ns.Write(ok, 0, ok.Length);
                            break;
                        }
                        case "joystick":
                        {
                            SendCommandRequest joystickModeCommand = new SendCommandRequest
                            {
                                ClientId = clientId,
                                Command = new UGCS.Sdk.Protocol.Encoding.Command
                                {
                                    Code = "joystick",
                                    Subsystem = Subsystem.S_FLIGHT_CONTROLLER,
                                    Silent = false,
                                    ResultIndifferent = false
                                }
                            };
                            
                            joystickModeCommand.Vehicles.Add(vehicleToControl);
                            var joystickMode = messageExecutor.Submit<SendCommandResponse>(joystickModeCommand);
                            joystickMode.Wait();
                            ns.Write(ok, 0, ok.Length);
                            break;
                        }
                        case "manual":
                        {
                            break;
                        }
                    }

                    
                }
                System.Console.ReadKey();
                tcpClient.Close();
                messageSender.Cancel();
                messageReceiver.Cancel();
                messageExecutor.Close();
                notificationListener.Dispose();
            }
            
        }
    }
}