using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


public class Client
{
    private TcpClient tcp;
    private string nick;
    private bool waitingPing;


    public Client(TcpClient tcp)
    {
        this.tcp = tcp;
        this.nick = "Guest";
        this.waitingPing = false;
    }

    public TcpClient GetTcpClient()
    {
        return this.tcp;
    }

    public string GetNick()
    {
        return this.nick;
    }
    public bool GetWaitingPing()
    {
        return this.waitingPing;
    }

    public void SetWaitingPing(bool waitingPing)
    {
        this.waitingPing = waitingPing;
    }

}
