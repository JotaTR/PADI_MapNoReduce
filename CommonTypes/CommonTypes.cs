using System;
using System.IO;

namespace PADI_MapNoReduce
{
    public class clsPerson
    {
        public string FirstName;
        public string MI;
        public string LastName;
    }

	public class MyRemoteObject : MarshalByRefObject  {

        public void write_XML(clsPerson p) {
            
            TextWriter tw = new StreamWriter(@"D:\obj.txt");
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(p.GetType());
            x.Serialize(tw, p);
            Console.WriteLine("object written to file");
            Console.ReadLine();
            tw.Close();
	    }

        public string read_XML()
        {
            TextReader tr = new StreamReader(@"D:\obj.txt");
             System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(clsPerson));
            clsPerson fileP = (clsPerson)x.Deserialize(tr);
            String data = "The person in the file is called " + fileP.FirstName + " " + fileP.MI + " " + fileP.LastName + ".";
            Console.WriteLine(data);
            tr.Close();
            return data; 
        }

  }
}