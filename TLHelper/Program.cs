using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TeleSharp.TL.Updates;
using TLSharp.Core;

namespace TLHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait() ;
        }

        static async Task MainAsync()
        {
            int apiId = 0;
            string apiHash = "",
                readPathApi = @"api.txt",
                readPathApiNash = @"apiHash.txt";

            if (File.Exists("api.txt") && File.Exists("apiHash.txt") && File.Exists("session.dat"))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(readPathApi, Encoding.Default))
                    {
                        apiId = Int32.Parse(sr.ReadLine());
                    }

                    using (StreamReader sr = new StreamReader(readPathApiNash, Encoding.Default))
                    {
                        apiHash = sr.ReadLine();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Enter api:");

                apiId = Int32.Parse(Console.ReadLine());
                Console.WriteLine();

                Console.WriteLine("Enter apiHash:");

                apiHash = Console.ReadLine();
                Console.WriteLine();

                try
                {
                    using (StreamWriter sw = new StreamWriter(readPathApi, false, Encoding.Default))
                    {
                        sw.WriteLine(apiId.ToString());
                    }
                    using (StreamWriter sw = new StreamWriter(readPathApiNash, false, Encoding.Default))
                    {
                        sw.WriteLine(apiHash);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            var store = new FileSessionStore();
            var client = new TelegramClient(apiId, apiHash, store);
            await client.ConnectAsync();
            bool authorized = client.IsUserAuthorized();

            if (authorized != true)
            {

                Console.WriteLine("Enter your phone number:");
                string phoneNumber = Console.ReadLine();
                Console.WriteLine();
                var hash = await client.SendCodeRequestAsync(phoneNumber);

                Console.WriteLine("Enter Login code:");
                string codeLogin = Console.ReadLine();
                Console.WriteLine();

                var user = await client.MakeAuthAsync(phoneNumber, hash, codeLogin);
            }

            //var found = await client.SearchUserAsync("msg_below", 1);
            //var u = found.Users.ToList().OfType<TLUser>().FirstOrDefault();

            //var peer = new TLInputPeerUser() { UserId = u.Id, AccessHash = (long)u.AccessHash };
            //Надсилаю повідомлення
            //Console.WriteLine("Enter ur test message: ");
            //string a = Console.ReadLine();
            //await client.SendMessageAsync(peer, a);


            var dialogs = (TLDialogs)await client.GetUserDialogsAsync();
            while (true)
            { 
                foreach (var dia in dialogs.Dialogs.ToList())
                {
                    if (dia.UnreadCount < 1) continue;

                    if (dia.Peer is TLPeerUser)
                    {
                        var peer = dia.Peer as TLPeerUser;
                        var chat = dialogs.Users.ToList().OfType<TLUser>().First(x => x.Id == peer.UserId);
                        var target = new TLInputPeerUser { UserId = chat.Id, AccessHash = chat.AccessHash ?? default(long) };
                        var hist = await client.GetHistoryAsync(target, 0, -1, dia.UnreadCount);
                        int firstMessage = 0;

                        if (hist is TLMessagesSlice)
                        {
                            int index = 0;
                            foreach (var m in ((TLMessagesSlice)hist).Messages.ToList())
                            {
                                TLMessage msg = m as TLMessage;

                                if (index == 0) firstMessage = msg.Id;

                                Console.WriteLine("{0} {1} {2}", msg.Id, msg.Message, msg.FromId);
                            }
                        }
                        else if (hist is TLMessages)
                        {
                            int index = 0;
                            foreach (var m in ((TLMessages)hist).Messages.ToList())
                            {
                                TLMessage msg = m as TLMessage;

                                if (index == 0) firstMessage = msg.Id;

                                Console.WriteLine("{0} {1} {2}", msg.Id, msg.Message, msg.FromId);
                            }
                        }
                        Thread.Sleep(5000);
                    }
                }
            }
        }

    }
}
