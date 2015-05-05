using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using PADI_MapNoReduce;


namespace Worker_JobTracker
{
    class Program
    {
        //if JT == true -> node is jobtracker; if W == true -> node is worker;if JT == true and W = true -> node is Worker serving has a JT backp node
        private static bool JT;
        private static bool W;
        private static String ip;
        private static String id;
        private static String client_ip;
        private static List<Worker> workers_list;
        private static List<JobTracker> jobtracker_list;
        

        /************************
         * METODOS PARA O JOB TRACKER
        ************************/
        // This is the call that the AsyncCallBack delegate will reference.
        public void assignWorkers(byte[] code, string className ,int split_number)
        {
            
            int[] split = new int[split_number];
            //List<Worker> workers_list_sorted = workers_list.OrderBy(o=>o.rank).ToList();//Sort List of workers

            //iterate through workers
            foreach (var w in workers_list) {
                Console.WriteLine("Amount is {0} and type is {1}", w.id, w.ip);
            }
            

            if(workers_list.Count > split_number){//+ workers do que splits

                for (int i = 0; i < split_number; i++ )
                {
                    informWorker(workers_list[i], i);
                }

            }else if(workers_list.Count < split_number){//+ splits do que workers

                for (int i = 0; i < split_number; i++)
                {
                    informWorker(workers_list[i], i);
                }

            }else{//igual numero entre splits e workers

                   
            }        
     
        }

        //informa o worker que deve executar o trabalho (este de correr o metodo execute Job);
        private void informWorker(Worker worker, int split_id)
        {

        }








        private void informJoin(Worker worker)
        {
            foreach (var w in workers_list)
            {
                //send message to w informing that
            }


        }





        /************************
        * METODOS PARA O WORKER
        ************************/
        //comunica com a aplicacao cliente a pedir o trabalho que lhe foi atribuido pelo Job Tracker
        private Task getTask(Task task)
        {
            String task_string = "";
            return task;
        }

        //comunica com a aplicacao cliente a pedir o trabalho que lhe foi atribuido pelo Job Tracker
        private void sendTask(String job_output)
        {

        }

        //executa trabalho
        private void execute_task(Task task)
        {
            Task split_string = getTask(task);

            //codigo para tratar o split

            String split_output = "";
            sendTask(split_output);
        }

        /**************************
         * Metodos especifos para o Worker com funções de JobTrackerReplica
        ***************************/
        public void checkJobTracker(){

        }





        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10000);

            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(WorkerServices),
                "Worker",
                WellKnownObjectMode.Singleton);
            System.Console.WriteLine("Press <enter> to terminate server...");
            System.Console.ReadLine();

            //Criar server a agardar comunicação dos workers
            workers_list.Add(new Worker("192.168.1.1", "1"));
            workers_list.Add(new Worker("192.168.1.1", "2"));
            workers_list.Add(new Worker("192.168.1.1", "3"));

            int split_number = 2;
            assignWorkers(split_number);

        }
    }




    /********************************
     * Interfaces
    *********************************/ 

    public class WorkerRegisterService : MarshalByRefObject, WorkerRegisterInterface{

    }

     public class JobAssignService : MarshalByRefObject, JobAssignInterface{

    }


}
