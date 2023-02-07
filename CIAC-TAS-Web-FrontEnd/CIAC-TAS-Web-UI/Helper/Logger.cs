namespace CIAC_TAS_Web_UI.Helper
{
    public class Logger
    {
        public static void WriteLog(string mensaje, string handler, string page)
        {
            //Creamos la carpeta
            string PathLog = System.AppDomain.CurrentDomain.BaseDirectory + "LogsErrors\\" + DateTime.Now.ToString("dd-MM-yyyy") + " Log.txt";
            StreamWriter log;

            if (!File.Exists(PathLog))
            {
                new FileInfo(PathLog).Directory.Create();
                log = new StreamWriter(PathLog);
            }
            else
            {
                log = File.AppendText(PathLog);
            }

            log.WriteLine("[Fecha]: " + DateTime.Now + " [Excepcion/Evento]: " + mensaje + " [Handler]: " + handler + " [Page]: " + page);
            // Close the stream
            log.Close();
        }

        public static void WriteLogUnhandledException(string mensaje, string stackTrace, string endpoint, string path)
        {
            //Creamos la carpeta
            string PathLog = System.AppDomain.CurrentDomain.BaseDirectory + "LogsErrors\\" + DateTime.Now.ToString("dd-MM-yyyy") + " Log.txt";
            StreamWriter log;

            if (!File.Exists(PathLog))
            {
                new FileInfo(PathLog).Directory.Create();
                log = new StreamWriter(PathLog);
            }
            else
            {
                log = File.AppendText(PathLog);
            }

            log.WriteLine("[Fecha]: " + DateTime.Now + " [Excepcion/Evento]: " + mensaje + "[StackTrace]:" + stackTrace + " [Endpoint]: " + endpoint + " [Path]: " + path);
            // Close the stream
            log.Close();
        }
    }
}
