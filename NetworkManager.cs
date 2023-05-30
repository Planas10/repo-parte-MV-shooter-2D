using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;
using MySqlX.XDevAPI;

public class NetworkManager
{
    private TcpListener serverListener;
    private List<Client> clients;

    private Mutex clientListMutex;

    private int lastTimePing;
    private List<Client> disconnectClients;

    /// DATABASE HERE ///
    Database_Manager database_manager = Database_Manager.DB_MANAGER();


    public NetworkManager() 
    {
        //Lista para almacenar clientes
        this.clients = new List<Client>();
        //Socket - Instancia de clase para aceptar conexiones de cualquier ip por un puerto especifico
        this.serverListener = new TcpListener(IPAddress.Any, 6543);

        //Instancia del Mutex
        this.clientListMutex = new Mutex(false);


        this.lastTimePing = Environment.TickCount;
        
        // Lista para clientes por eliminar
        this.disconnectClients= new List<Client>();
    }

    public void StartNetworkService()
    {
        try
        {
            this.serverListener.Start();
            StartListening();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    private void StartListening()
    {
        Console.WriteLine("Esperando nueva conexión");
        this.serverListener.BeginAcceptTcpClient(AcceptConnection, this.serverListener);
    }

    private void AcceptConnection(IAsyncResult ar)
    {
        Console.WriteLine("Recibo conexión");
        
        
        TcpListener listener = (TcpListener)ar.AsyncState;
        
        this.clientListMutex.WaitOne();

        this.clients.Add(new Client(listener.EndAcceptTcpClient(ar)));

        this.clientListMutex.ReleaseMutex();
    
        //Vuelvo a usar
        StartListening();

    }

    public void CheckMessage()
    {
        clientListMutex.WaitOne();
        foreach (Client client in this.clients) 
        {
            //Acceso al stream de datos
            NetworkStream netStream = client.GetTcpClient().GetStream();

            
            //Comprobación si la info esta lista para ser leída
            if (netStream.DataAvailable)
            {
                StreamReader reader = new StreamReader(netStream, true);
                string data = reader.ReadLine();

                //Comprobar si hay información
                if (data != null)
                {
                    ManageData(client, data);
                }
            }

        }
        clientListMutex.ReleaseMutex();
    }

    

    public void ManageData(Client client, string data)
    {
        string[] parameters = data.Split('/');

        Console.WriteLine("Data entering with parameter: "+ parameters[0]);
        Console.WriteLine(data);
        switch (parameters[0])
        {
            //Login 
            case "0":
                Login(client,parameters[1], parameters[2]);
                break;

            // Ping
            case "1":
                ReceivePing(client);
                break;

            //Register
            case "2":
                Register(client, parameters[1], parameters[2], parameters[3]);
                break;

            //GetData
            case "3":
                GetGameData(client);
                break;
  
            //Version Control
            case "4":
                GetVersion(client);
                break;

        }

    }
    void GetVersion(Client client)
    {

        try
        {
            string version = database_manager.GetLatestVersion();

            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            writer.WriteLine("4" + "/" +version);
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message + "con el cliente: " + client.GetNick());
            throw;
        }

        
    }
    public void GetGameData(Client client) 
    {
        //GET GAME DATA FROM SQL SERVER

        try
        {
            string data = database_manager.GetGameData();

            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            writer.WriteLine("3" + "/" + data);
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message + "con el cliente: " + client.GetNick());
            throw;
        }


    }

    public void Login(Client client, string username, string password)
    {
        Console.WriteLine("Loging user with credentials" +username+ ":" + password);
        try
        {
            int registeredUserID = database_manager.GetUserId(username, password);

            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            if (registeredUserID >= 0)
            {
                writer.WriteLine("3" + "/ Logged as: " + username);
                writer.Flush();
                writer.WriteLine("4" + "/ " + database_manager.GetPlayerStats(database_manager.GetPlayerRace(registeredUserID.ToString())));
                writer.Flush();
            }
            else
            {
                writer.WriteLine("2" + "/" + "User dosentExists");
                writer.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message + "con el cliente: " + client.GetNick());
            throw;
        }
    }

    public void Register(Client client, string username, string password,string race)
    {
        Console.WriteLine("Registering User with credentials: "+username+ ":" + password + " with race: " + race);
        try
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            if (database_manager.ExistsPlayer(username)) 
            {
                writer.WriteLine("2" + "/ Player with name " + username + " Already exists");
                writer.Flush();
                return;
            }


            int registeredUserID = database_manager.InsertUserData(username, password,race);
            writer.WriteLine("2" + "/ Registerd user with id : " + registeredUserID);
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message + "con el cliente: " + client.GetNick());
            throw;
        }
    }

    public void CheckConnection()
    {
        //Compruebo tiempo para ping
        if(Environment.TickCount - this.lastTimePing > 5000)
        {
            //bloquear mutex
            clientListMutex.WaitOne();

            foreach(Client client in this.clients)
            {
                if(client.GetWaitingPing() == true)
                {
                    disconnectClients.Add(client);
                }
                else
                {
                    SendPing(client);
                }

            }

            this.lastTimePing = Environment.TickCount;
            clientListMutex.ReleaseMutex();


        }

    }


    private void ReceivePing(Client client)
    {
        Console.WriteLine("Received Ping from: " + client.GetNick());
        client.SetWaitingPing(false);
    }

    private void SendPing(Client client)
    {
        try
        {
            Console.WriteLine("Sending Ping to: " + client.GetNick);

            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());
            writer.WriteLine("1" + "/");

            writer.Flush();
            client.SetWaitingPing(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message + "con el cliente: " + client.GetNick());
            throw;
        }
    }

    public void DisconnectClients()
    {
        clientListMutex.WaitOne();
        foreach(Client client in this.disconnectClients)
        {

            Console.WriteLine("Desconectando usuario: " + client.GetNick());
            client.GetTcpClient().Close();
            this.clients.Remove(client);

        }

        this.disconnectClients.Clear();

        clientListMutex.ReleaseMutex();

    }
}

