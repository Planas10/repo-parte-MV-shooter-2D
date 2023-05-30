using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DataType { USERDATA, GAMEDATA, CHARACTERDATA, VERSION}
class Server
{
    static void Main(string[] args)
    {

        bool bServerOn = true;

        //Instancio los servicios de red del servidor
        Database_Manager db_Manager = Database_Manager.DB_MANAGER();
        NetworkManager network_service = new NetworkManager();
        

        //Empiezo los servicios del servidor
        StartService();

        //Mientras sea TRUE el servidor se mantiene PRENDÍO
        while (bServerOn)
        {
            //Network Service
            network_service.CheckConnection();
            network_service.CheckMessage();
            network_service.DisconnectClients();

            //Database Services
        }


        void StartService()
        {
            //Servicios de red
            network_service.StartNetworkService();
            
            //Servicio de base de Datos
        }

    }

}