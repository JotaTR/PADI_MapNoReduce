using System;
using System.Collections.Generic;
using System.IO;

/// CHANGE TO CLIENT: Renato


namespace PADI_MapNoReduce
{
  
    /************************
     * Classes
    **************************/
    class Worker
    {
        public double rank;//classificação de fiabilidade (contactados primeiro)
        public String ip;
        public String id;
        public bool ready;

        public Worker(String ip, String id)
        {

            this.ip = ip;
            this.id = id;
            ready = true;

        }
    }

    class JobTracker
    {
        public String ip;
        public String id;

        public JobTracker(String ip, String id)
        {

            this.ip = ip;
            this.id = id;

        }
    }



    class Task
    {
        public Worker worker;//classificação de fiabilidade (contactados primeiro)
        public int TaskId;
        public String state;//waiting for worker, waiting for Task, in progress, finished, aborted

        public Task(Worker worker, int TaskId, String state)
        {

            this.worker = worker;
            this.TaskId = TaskId;
            this.state = state;
        }
    }





    //JobTacker interface
    public interface WorkerRegisterInterface
    {
        void registerWorker(String worker_ip, int id);
        List<Worker> getWorkers();
    }

    public interface JobAssignInterface
    {
        void assignJob(int Task_number, int id);
        List<Worker> getWorkers();
    }


    //Worker Interface
    public interface WorkerRegisterInterface
    {
        void registerWorker(String worker_ip, int id);
        List<Worker> getWorkers();
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


        // LOCAL METHOD INIT
        public void INIT()
        {

        }

        // LOCAL METHOD SUBMIT
        public void SUBMIT(string filepath, int nTasks, string outputPath, IMap mapImplementation)
        {
          // UserApplication (and PuppetMaster, for testing) call this method in order to send new mapping jobs to Server(Workers)
        }


        // REMOTE Provide Tasks
        public void ProvideTasks(string textbegin, string textEnd)
        {
          // Server(Workers) call this method in order to receive the Task they are supposed to work on
        }

        // REMOTE Receive Output
        public void ReceiveOutput(string processedTask)
        {
          // Server(Workers) call this method in order to send the client the result of processing the Tasks
        }
  }
}