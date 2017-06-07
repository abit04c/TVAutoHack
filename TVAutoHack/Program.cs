using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace TVAutoHack
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Server sv = new Server();
			Application.Run();
		}
	}

	class Server
	{
		TcpListener listener;
		TcpClient client;
		System.Timers.Timer timer1;

		public Server()
		{
			Thread mainThr = new Thread(ServerRun);
			mainThr.IsBackground = true;
			mainThr.Start();
		}


		private void ServerRun()
		{
			listener = new TcpListener(IPAddress.Parse("103.56.164.217"), 60000);
			listener.Start();

			// Send Version string
			while (true)
			{
				try
				{
					client = listener.AcceptTcpClient();
					timer1 = new System.Timers.Timer();
					timer1.Interval = 1000;
					timer1.Elapsed += Timer1_Elapsed;
					timer1.Start();

					client.Client.Send(Encoding.ASCII.GetBytes("1040Tvpro 1.9.4\r\n"));
					StreamReader reader = new StreamReader(client.GetStream());

					// read
					while (true)
					{
						if (reader.BaseStream.CanRead)
						{
							string request = reader.ReadLine();

							request = request.Substring(0, 4);

							if (request.Equals("1002"))
							{
								client.Client.Send(Encoding.ASCII.GetBytes("1010"));
							}
							else if (request.Equals("1011"))
							{
								byte[] data = { 0x31, 0x30, 0x32, 0x30, 0x83, 0x30, 0x0d, 0x0a };
								client.Client.Send(data);

								for (int i = 1; i <= 9; i++)
									for (int j = 1; j <= 9; j++)
									{
										byte[] train = { 0x31, 0x30, 0x33, 0x38, 0x83, 0x31, 0x30, 0x31, 0x2f, 0x30, 0x33, 0x2f, 0x32, 0x30, 0x39, 0x39, 0x0d, 0x0a };
										train[2] = (byte)(0x30 + i);
										train[3] = (byte)(0x30 + j);
										client.Client.Send(train);
									}
							}
						}
					}
				}
				catch
				{
					timer1.Stop();
					continue;
				}
			}
		}

		// Update thời gian cho client
		private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
		{
			byte[] updateTime = { 0x31, 0x30, 0x30, 0x38, 0x83, 0x54, 0x68, 0xf8, 0x20, 0x00, 0x20, 0x30, 0x30, 0x3a, 0x34, 0x35, 0x3a, 0x30, 0x35, 0x20, 0x30, 0x33, 0x2f, 0x30, 0x32, 0x2f, 0x32, 0x30, 0x31, 0x37, 0x0d, 0x0a };
			updateTime[9] = (byte)(((byte)DateTime.Now.DayOfWeek) + 0x31);
			string time = DateTime.Now.ToString(" HH:mm:ss dd/MM/yyyy");
			Array.Copy(Encoding.ASCII.GetBytes(time), 0, updateTime, 10, 20);
			try
			{
				client.Client.Send(updateTime);
			}
			catch
			{
				timer1.Stop();
			}
		}
	}
}
